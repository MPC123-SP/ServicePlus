using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using ServicePlusAPIs.Context;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Text;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public class CustomAuthorizeAttribute : Attribute, IAuthorizationFilter
{
    private readonly List<string> _roles;
    private readonly ServicePlusContext _servicePlusContext;
 

    public void OnAuthorization(AuthorizationFilterContext context)
    {
        var actionName = context.ActionDescriptor.RouteValues["action"];     

        var token = context.HttpContext.Request.Headers["Authorization"].FirstOrDefault()?.Split(" ").Last();
        if (token == null)
        {
            context.Result = new JsonResult(new { message = "Unauthorized" }) { StatusCode = StatusCodes.Status401Unauthorized };
            return;
        }

        // Retrieve the RoleManager from the service provider
        var roleManager = context.HttpContext.RequestServices.GetRequiredService<RoleManager<IdentityRole>>();

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

        foreach (var roleName in roles)
        {
            // Check if the user's roles match any of the allowed roles
            var role = roleManager.FindByNameAsync(roleName).Result;

            if (role != null)
            {
                // Role found, you can access its ID
                var roleId = role.Id;
                var servicePlusContext = context.HttpContext.RequestServices.GetRequiredService<ServicePlusContext>();
                var methodPermission = servicePlusContext.RolePermissions.Where(d => d.RoleId == roleId).Select(d => d.Permission).ToList();

                if (methodPermission.Contains(actionName))
                {
                    
                    return; // Authorized
                }
                else
                {
                    context.Result = new JsonResult(new { message = "Unauthorized" }) { StatusCode = StatusCodes.Status401Unauthorized };


                }
            }
        }

        context.Result = new JsonResult(new { message = "Unauthorized" }) { StatusCode = StatusCodes.Status401Unauthorized };
    }
}
