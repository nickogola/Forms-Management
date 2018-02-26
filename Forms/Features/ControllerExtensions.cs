using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Forms.Features
{
    public static class ControllerExtensions
    {
        public static IActionResult RedirectToActionJson<TController>(this TController controller, string action) where TController : Controller
        {
            var model = new
            {
                redirect = controller.Url.Action(action)
            };

            return controller.RedirectToJson(model);
        }

        public static IActionResult RedirectToActionJson<TController>(this TController controller, string action, object values) where TController : Controller
        {
            var model = new
            {
                redirect = controller.Url.Action(action, values)
            };

            return controller.RedirectToJson(model);
        }

        private static ContentResult RedirectToJson(this Controller controller, object model)
        {
            var serialized = JsonConvert.SerializeObject(
                model,
                new JsonSerializerSettings
                {
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore
                }
            );

            return new ContentResult
            {
                Content = serialized,
                ContentType = "application/json"
            };
        }
    }
}
