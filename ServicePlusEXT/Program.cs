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

app.UseHttpsRedirection(); app.MapPost("/get-service-details", async (
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

    if (!response.IsSuccessStatusCode)
        return Results.BadRequest(json);

    using var doc = JsonDocument.Parse(json);

    var root = doc.RootElement;

    if (root.ValueKind != JsonValueKind.Array)
        return Results.BadRequest("Expected array");

    foreach (var item in root.EnumerateArray().Take(3)) // first 3
    {
        var appEntity = new ServiceApplication
        {
            ApplRefNo = item.GetProperty("appl_ref_no").GetString() ?? "",
            ApplId = item.GetProperty("appl_id").GetString() ?? "",
            AppliedBy = item.GetProperty("applied_by").GetString() ?? ""
        };

        // 🔥 Dynamic attributes
        if (item.TryGetProperty("application_form_attributes", out var attrs) &&
            attrs.ValueKind == JsonValueKind.Object)
        {
            foreach (var prop in attrs.EnumerateObject())
            {
                appEntity.Attributes.Add(new ApplicationAttribute
                {
                    Key = prop.Name,
                    Value = prop.Value.ToString()
                });
            }
        }

       await db.ServiceApplications.AddAsync(appEntity);
    }

    await db.SaveChangesAsync();

    return Results.Ok("Saved successfully");
});
app.Run();

 
