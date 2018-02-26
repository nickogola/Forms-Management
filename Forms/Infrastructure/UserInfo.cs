using Forms.Infrastructure.Interfaces;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Forms.Infrastructure
{
    public class UserInfo : IUserInfo
    {
        private readonly IHttpContextAccessor _context;

        public UserInfo(IHttpContextAccessor context)
        {
            _context = context;
        }

        public string UserName
        {
            get
            {
                return _context.HttpContext.User.GetUserName();
            }
        }
    }
}
