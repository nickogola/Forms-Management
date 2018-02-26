
using Forms.Infrastructure.Interfaces;
using Microsoft.AspNetCore.Authentication;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Forms.Infrastructure
{
    public class ClaimsTransformer : IClaimsTransformation
    {
        private readonly IDbContext _context;

        public ClaimsTransformer(IDbContext context)
        {
            _context = context;
        }

        public async Task<ClaimsPrincipal> TransformAsync(ClaimsPrincipal principal)
        {
            var id = (ClaimsIdentity)principal.Identity;
            var ci = new ClaimsIdentity(id.Claims, id.AuthenticationType, id.NameClaimType, id.RoleClaimType);
            var permissions = await GetPermissions(principal);

            foreach (var permission in permissions)
            {
                ci.AddClaim(new Claim(ClaimTypes.Role, permission));
            }

            var cp = new ClaimsPrincipal(ci);
            return cp;
        }

        public async Task<IList<string>> GetPermissions(ClaimsPrincipal principal)
        {
            var sql = @"
                SELECT ap.PermissionName
                FROM sec.Users u
                INNER JOIN sec.UserPermissions up ON u.UserID = up.UserID
                INNER JOIN sec.ApplicationPermissions ap ON up.PermissionID = ap.PermissionID
                WHERE u.UserID = @UserID;";

            var query = await _context.QueryAsync<string>(sql, new { UserID = principal.GetUserName() });
            return query.ToList();
        }
    }
}
