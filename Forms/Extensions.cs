using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Principal;
using System.Threading.Tasks;

namespace Forms
{
    public static class Extensions
    {
        public static string GetUserName(this IPrincipal principal)
        {
            string value = principal.Identity.Name;
            int index = value.IndexOf("\\");
            return (index > -1) ? value.Substring(index + 1) : value;
        }
        public static byte[] ToByteArray(this IFormFile file)
        {
            MemoryStream ms = new MemoryStream();
            file.CopyTo(ms);
            return ms.ToArray();
        }
    }
}
