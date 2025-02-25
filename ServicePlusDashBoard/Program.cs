using Microsoft.AspNetCore.Authentication.Cookies;
using ServicePlusDashBoard.Helper;

var builder = WebApplication.CreateBuilder(args);

ConfigurationManager configuration = builder.Configuration;

// Add services to the container.
builder.Configuration.AddJsonFile("appsettings.json");

// Add scoped services
builder.Services.AddScoped<PermissionService>();

// Add controllers and views
builder.Services.AddControllersWithViews();

// Configure Cookie Authentication
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Account/Login"; // Redirect to login page if unauthenticated
        options.LogoutPath = "/Account/Logout"; // Redirect to logout path
        options.Cookie.Name = "CRSPortal"; // Set the cookie name
        options.ExpireTimeSpan = TimeSpan.FromMinutes(2); // Corrected line
        options.SlidingExpiration = true; // Ensures the expiration time updates with activity
    });


// Register HttpClient with token handler
builder.Services.AddHttpClient("ServicePlusClient", client =>
{
    client.BaseAddress = new Uri("http://10.147.24.36:8082"); // API Base URL
})
.AddHttpMessageHandler<CustomTokenHandler>(); // Add a custom token handler to attach authorization tokens
 
builder.Services.AddScoped<CustomTokenHandler>(); // Ensure it's properly injected
builder.Services.AddHttpContextAccessor(); // For accessing cookies in controllers
 

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

// Enable authentication and authorization middleware
app.UseAuthentication(); // Must be before UseAuthorization
app.UseAuthorization();

// Configure the default route for MVC
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Account}/{action=Login}/{id?}");

// Run the application
app.Run();
