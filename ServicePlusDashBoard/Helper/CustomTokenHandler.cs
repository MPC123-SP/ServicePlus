using Microsoft.AspNetCore.Http;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;

namespace ServicePlusDashBoard.Helper
{
    public class CustomTokenHandler : DelegatingHandler
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public CustomTokenHandler(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var context = _httpContextAccessor.HttpContext;
            if (context != null && context.Request.Cookies.TryGetValue("CRSPortal", out var token))
            {
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
               
                 
            }


            var response = await base.SendAsync(request, cancellationToken);

            if (response.StatusCode == HttpStatusCode.Unauthorized)
            {
                context.Response.Cookies.Delete("CRSPortal"); // Delete invalid token
                context.Response.Redirect("/Account/Login");
            }

            return response;
        }
    }
}
