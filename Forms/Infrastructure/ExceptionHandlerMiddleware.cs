using FluentValidation;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Forms.Infrastructure
{
    public class ExceptionHandlerMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger _logger;

        public ExceptionHandlerMiddleware(RequestDelegate next, ILoggerFactory loggerFactory)
        {
            _next = next;
            _logger = loggerFactory.CreateLogger<ExceptionHandlerMiddleware>();
        }

        public async Task Invoke(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                try
                {
                    bool isAjaxRequest = IsAjaxRequest(context.Request);

                    if (isAjaxRequest)
                    {
                        await HandleApiException(context, ex);
                    }
                    else
                    {
                        _logger.LogError(0, ex, $"An unhandled exception has occurred: {ex.Message}");

                        context.Request.Path = new PathString("/Error/Index/500");
                        context.Response.Clear();
                        context.Response.StatusCode = 500;

                        await _next(context);
                    }

                    return;
                }
                catch (Exception ex2)
                {
                    _logger.LogError(0, ex2, "An exception was thrown attempting to execute the error handler.");
                }

                throw;
            }
        }

        private bool IsAjaxRequest(HttpRequest request)
        {
            var query = request.Query;
            if (query != null)
            {
                if (query["X-Requested-With"] == "XMLHttpRequest")
                {
                    return true;
                }
            }

            var headers = request.Headers;
            if (headers != null)
            {
                if (headers["X-Requested-With"] == "XMLHttpRequest")
                {
                    return true;
                }
            }

            return false;
        }

        private async Task HandleApiException(HttpContext context, Exception ex)
        {
            bool writeToErrorLog = true;
            string message = "A critical error has occurred -- please contact the systems administrator.";

            if (ex is ValidationException)
            {
                writeToErrorLog = false;
                message = ex.Message;
            }

            if (writeToErrorLog)
                _logger.LogError(0, ex, $"An unhandled exception has occurred: {ex.Message}");

            var result = JsonConvert.SerializeObject(
                           new { message = message },
                           new JsonSerializerSettings
                           {
                               ReferenceLoopHandling = ReferenceLoopHandling.Ignore
                           }
                       );

            context.Response.StatusCode = 500;
            context.Response.ContentType = "application/json";
            await context.Response.WriteAsync(result);
        }
    }
}
