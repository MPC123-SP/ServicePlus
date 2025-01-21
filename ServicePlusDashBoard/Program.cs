using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using ServicePlusDashBoard.Helper;
using ServicePlusDashBoard.Middleware;
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
    client.BaseAddress = new Uri("http://10.147.24.36:8082");
})
.AddHttpMessageHandler(() =>
{
    return new CustomTokenHandler();
});
builder.Services.AddHttpClient();  // Registers HttpClient for injection
builder.Services.AddHttpContextAccessor(); // For accessing HttpContext to get cookies

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
        .AddJwtBearer(options =>
        {
            options.RequireHttpsMetadata = false;
            options.SaveToken = true;
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = configuration["JWT:ValidAudience"], // Set your issuer
                ValidAudience = configuration["JWT:ValidIssuer"], // Set your audience
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("yourSecretKey"))
            };
            options.Events = new JwtBearerEvents
            {
                OnAuthenticationFailed = context =>
                {
                    Console.WriteLine($"Authentication failed: {context.Exception.Message}");
                    return Task.CompletedTask;
                },
                OnTokenValidated = context =>
                {
                    Console.WriteLine($"Token validated: {context.SecurityToken}");
                    return Task.CompletedTask;
                }
            };
        });

var app = builder.Build(); 
app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseMiddleware<FirstMiddleware>();
app.UseAuthentication(); // Enable authentication middleware
app.UseAuthorization(); // Enable authorization middleware
 
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Account}/{action=Login}/{id?}");

app.Run();
