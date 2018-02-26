using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Forms.Filters
{
    public class ValidateModelActionFilter : IActionFilter
    {
        public void OnActionExecuting(ActionExecutingContext filterContext)
        {
            if (!filterContext.ModelState.IsValid)
            {
                if (filterContext.HttpContext.Request.Method == "GET")
                {
                    var result = new BadRequestResult();
                    filterContext.Result = result;
                }
                else
                {
                    string content = JsonConvert.SerializeObject(filterContext.ModelState,
                        new JsonSerializerSettings
                        {
                            ReferenceLoopHandling = ReferenceLoopHandling.Ignore
                        });

                    var result = new ContentResult
                    {
                        Content = content,
                        ContentType = "application/json"
                    };

                    filterContext.HttpContext.Response.StatusCode = 400;
                    filterContext.Result = result;
                }
            }
        }

        public void OnActionExecuted(ActionExecutedContext filterContext)
        {

        }
    }
}
