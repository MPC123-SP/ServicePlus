using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Text;

namespace ServicePlusDashBoard.Helper
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public class CustomAuthorizeAttribute : Attribute, IAuthorizationFilter
    {
        private readonly string _role;
      
        public CustomAuthorizeAttribute(string role)
        {
            _role = role; 
        }


        public void OnAuthorization(AuthorizationFilterContext context)
        {
            
            var actionName = context.ActionDescriptor.RouteValues["action"];
            var token = context.HttpContext.Request.Headers["Authorization"].FirstOrDefault()?.Split(" ").Last();
            if (string.IsNullOrEmpty(token))
            {
                context.Result = new JsonResult(new { message = "Wow! Wow! Please login, you are Unauthorized" }) { StatusCode = StatusCodes.Status401Unauthorized };
                context.Result = new RedirectToActionResult("Login", "Account", null);

                return;
            }

            // Retrieve the RoleManager from the service provider
            
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes("ServicePlusApiMadeByNIC");
            tokenHandler.ValidateToken(token, new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ValidateIssuer = false,
                ValidateAudience = false,
                ClockSkew = TimeSpan.Zero
            }, out SecurityToken validatedToken);

            var jwtToken = (JwtSecurityToken)validatedToken;
            var rolesString = jwtToken.Claims.FirstOrDefault(x => x.Type == "Roles")?.Value;

            if (string.IsNullOrEmpty(rolesString))
            {
                context.Result = new JsonResult(new { message = "Unauthorized" }) { StatusCode = StatusCodes.Status401Unauthorized };
                return;
            }

            var roles = rolesString.Split(',').ToList();


            if (roles.Last() == "")
            {
                roles.Remove(roles.Last().ToString());
            }
            // Check if the user is a SuperAdmin
            if (roles.Contains("SuperAdmin"))
            {
                return; // SuperAdmin is allowed access without permission check
            }
            else
            {
                context.Result = new RedirectToActionResult("Login", "Account", null);

                return;
            }
             

             }
    }
}
