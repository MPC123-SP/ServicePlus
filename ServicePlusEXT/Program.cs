using Microsoft.EntityFrameworkCore;
using Scalar.AspNetCore;
using ServicePlusEXT.Context;
using ServicePlusEXT.Dtos;
using ServicePlusEXT.Entities;
using ServicePlusEXT.ResponseDtos;
using System.Text;
using System.Text.Json;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
builder.Services.AddHttpClient();
builder.Services.AddDbContextPool<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));
var app = builder.Build();

 
    app.MapOpenApi(); 

// ✅ Scalar UI
app.MapScalarApiReference(options =>
{
    options.Title = "ServicePlus API";
    options.Theme = ScalarTheme.Laserwave;
});

app.UseHttpsRedirection(); 
app.Run();

 
