using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;

namespace ServicePlusDashBoard.Helper
{
    public class PermissionService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public PermissionService(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public bool HasPermission(string requiredPermission)
        {
            var context = _httpContextAccessor.HttpContext;
            var JWToken = context.Request.Cookies["jwtToken"]; // Get the JWT token from the cookie
           
            if (string.IsNullOrEmpty(JWToken))
            {
                // Handle the case where the JWT token is missing or invalid.
                return false;
            }

            var tokenHandler = new JwtSecurityTokenHandler();
            var tokenClaim = tokenHandler.ReadJwtToken(JWToken);

            // Check if the user has the required permission based on the claims in the token.
            var claimsIdentity = tokenClaim.Claims;
            if (claimsIdentity.Any(c => c.Type == "Roles" && c.Value == "SuperAdmin"))
            {
                return true;
            }

            // Replace "your_claim_name" with the actual claim name you're using for permissions.
            if (claimsIdentity.Any(c => c.Type == "Permission" && c.Value == requiredPermission))
            {
                return true;
            }

            return false;
        }
    }
}
