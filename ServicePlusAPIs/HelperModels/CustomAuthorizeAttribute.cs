using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using ServicePlusAPIs.Context;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net.Http;
using System.Text;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public class CustomAuthorizeAttribute : Attribute, IAsyncAuthorizationFilter
{
    private readonly List<string> _roles;
    private readonly ServicePlusContext _servicePlusContext;
    private IConfiguration _configuration;

    public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
    {
        var actionName = context.ActionDescriptor.RouteValues["action"];
        var token = context.HttpContext.Request.Headers["Authorization"].FirstOrDefault()?.Split(" ").Last();

        if (token == null)
        {
            context.Result = new JsonResult(new { message = "Unauthorized" }) { StatusCode = StatusCodes.Status401Unauthorized };
            return;
        }

        _configuration = context.HttpContext.RequestServices.GetService<IConfiguration>();
        var roleManager = context.HttpContext.RequestServices.GetRequiredService<RoleManager<IdentityRole>>();

        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.ASCII.GetBytes(_configuration["JWT:Secret"]);

        // Validate the token asynchronously
        var validationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(key),
            ValidateIssuer = false,
            ValidateAudience = false,
            ClockSkew = TimeSpan.Zero
        };

        tokenHandler.ValidateToken(token, validationParameters, out SecurityToken validatedToken);
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

        if (roles.Contains("SuperAdmin"))
        {
            return; // SuperAdmin is allowed access without permission check
        }

        foreach (var roleName in roles)
        {
            // Find the role asynchronously
            var role = await roleManager.FindByNameAsync(roleName);

            if (role != null)
            {
                // Fetch permissions asynchronously
                var servicePlusContext = context.HttpContext.RequestServices.GetRequiredService<ServicePlusContext>();
                var methodPermission = await servicePlusContext.RolePermissions
                                                             .Where(d => d.RoleId == role.Id)
                                                             .Select(d => d.Permission)
                                                             .ToListAsync();

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
