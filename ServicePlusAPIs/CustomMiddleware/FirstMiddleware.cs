using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using ServicePlusAPIs.Controllers;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using DocumentFormat.OpenXml.InkML;
using ClosedXML;

namespace ServicePlusAPIs.CustomMiddleware
{
    // You may need to install the Microsoft.AspNetCore.Http.Abstractions package into your project
    public class FirstMiddleware
    {
        private readonly RequestDelegate _next;

        private readonly ILogger<ServicePlusController> _logger;
        public FirstMiddleware(RequestDelegate next, ILogger<ServicePlusController> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task Invoke(HttpContext httpContext)
        {
           
                httpContext.Request.EnableBuffering();

                using (StreamReader reader = new StreamReader(httpContext.Request.Body, Encoding.UTF8, true, 1024, true))
                {
                    string requestBody = await reader.ReadToEndAsync();
                    //_logger.LogInformation("Json ||Sent|| Over|| API ||First MiddleWare Recieved-format "+ requestBody);
                    _logger.LogInformation("Json ||Sent|| Over|| API ||First MiddleWare Recieved-format ");
                //string decodedData = System.Web.HttpUtility.UrlDecode(requestBody);
                //_logger.LogInformation("Json ||Sent|| Over|| API ||First MiddleWare " + decodedData);
                
                    httpContext.Request.Body.Position = 0; // rewind the stream to allow further processing
                }
            // Get the endpoint name using reflection 
            string apiPath = httpContext.Request.Path;  


            await _next(httpContext);
           
            
         
        }
    }

    // Extension method used to add the middleware to the HTTP request pipeline.
    public static class MiddlewareExtensions
    {
        public static IApplicationBuilder UseMiddleware(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<FirstMiddleware>();
        }
    }
}
