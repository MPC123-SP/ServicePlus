namespace ServicePlusDashBoard.Middleware
{
    // You may need to install the Microsoft.AspNetCore.Http.Abstractions package into your project
    public class FirstMiddleware
    {
        private readonly RequestDelegate _next;

        public FirstMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task Invoke(HttpContext httpContext)
        { 
            var jwtToken = httpContext.Request.Cookies["jwtToken"];
            if (!string.IsNullOrWhiteSpace(jwtToken))
            {
                httpContext.Request.Headers.Add("Authorization", "Bearer " + jwtToken);

            } 


            await _next(httpContext);
        }
    }

    // Extension method used to add the middleware to the HTTP request pipeline.
    public static class FirstMiddlewareExtensions
    {
        public static IApplicationBuilder UseFirstMiddleware(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<FirstMiddleware>();
        }
    }
}
