using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using ServicePlusDashBoard.Helper;
using ServicePlusDashBoard.Middleware;
using System.IdentityModel.Tokens.Jwt;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

ConfigurationManager configuration = builder.Configuration;

// Add services to the container.
builder.Configuration.AddJsonFile("appsettings.json");
builder.Services.AddTransient<PermissionService>();
builder.Services.AddHttpContextAccessor();
builder.Services.AddControllersWithViews();
 
// Register HttpClient with a custom handler to include the token
builder.Services.AddHttpClient("ServicePlusClient", client =>
{
    client.BaseAddress = new Uri("http://10.147.24.36:8083/api/ServicePlus/");
})
.AddHttpMessageHandler(() =>
{
    return new CustomTokenHandler();
});
builder.Services.AddHttpClient();  // Registers HttpClient for injection
builder.Services.AddHttpContextAccessor(); // For accessing HttpContext to get cookies
var app = builder.Build();

app.UseMiddleware<FirstMiddleware>();
app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication(); // Enable authentication middleware
app.UseAuthorization(); // Enable authorization middleware
 
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Account}/{action=Login}/{id?}");

app.Run();
