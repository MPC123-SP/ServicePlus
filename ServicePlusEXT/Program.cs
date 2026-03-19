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
    options.Theme = ScalarTheme.BluePlanet;
});

app.UseHttpsRedirection();

app.MapPost("/get-service-details", async (
    HttpClient http,
    ServiceDetailsRequest dto,
    AppDbContext db) =>
{
    var url = $"https://eservices.punjab.gov.in/v1/application/details/?serviceId={dto.serviceId}&date={dto.date}";

    var request = new HttpRequestMessage(HttpMethod.Post, url)
    {
        Content = new StringContent(string.Empty, Encoding.UTF8, "application/json")
    };

    request.Headers.Add("client_id", dto.clientId);
    request.Headers.Add("client_secret", dto.secretId);

    var response = await http.SendAsync(request);
    var json = await response.Content.ReadAsStringAsync();
    Console.WriteLine(json);
    if (!response.IsSuccessStatusCode)
        return Results.BadRequest(json);

    // ✅ Parse raw JSON (no DTO)
    using var doc = JsonDocument.Parse(json);

    // ⚠️ Adjust this path based on actual API response
    // assuming response like: { data: [ ... ] }
    if (!doc.RootElement.TryGetProperty("data", out var dataArray) || dataArray.ValueKind != JsonValueKind.Array)
    {
        return Results.BadRequest("Invalid JSON structure");
    }

    // ✅ Take first 3 records
    var first3 = dataArray.EnumerateArray().Take(3);

    // Convert back to JSON
    var resultList = new List<JsonElement>();
    foreach (var item in first3)
    {
        resultList.Add(item);
    }

    return Results.Json(resultList);
});
app.Run();

 
