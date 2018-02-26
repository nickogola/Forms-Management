using Forms.Infrastructure.Interfaces;
using Microsoft.AspNetCore.Authorization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Forms.Infrastructure
{
    public class PermissionRequirementHandler : AuthorizationHandler<PermissionRequirement>
    {
        private readonly IDbContext _context;

        public PermissionRequirementHandler(IDbContext context)
        {
            _context = context;
        }

        protected async override Task HandleRequirementAsync(AuthorizationHandlerContext context, PermissionRequirement requirement)
        {
            var havePermission = await CheckPermission(context.User, requirement.Permission);

            if (havePermission)
                context.Succeed(requirement);
        }

        private async Task<bool> CheckPermission(ClaimsPrincipal principal, string permission)
        {
            var sql = @"
SELECT ap.PermissionName
FROM sec.Users u
INNER JOIN sec.UserPermissions up ON u.UserID = up.UserID
INNER JOIN sec.ApplicationPermissions ap ON up.PermissionID = ap.PermissionID
WHERE u.UserID = @UserID AND ap.PermissionName = @Permission";

            var query = await _context.QueryAsync<string>(
                sql,
                new
                {
                    UserID = principal.GetUserName(),
                    Permission = permission
                }
            );

            return query.Any();
        }
    }
}
