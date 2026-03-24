using Microsoft.EntityFrameworkCore;
using ServicePlusEXT.Context;
using ServicePlusEXT.Dtos;
using ServicePlusEXT.Entities;
using ServicePlusEXT.Shared;
using System.Text;
using System.Text.Json;

namespace ServicePlusEXT.Endpoints
{
    public   class Endpoint : IEndpoint
    {
        public void Map(IEndpointRouteBuilder app)
        {
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

                if (!response.IsSuccessStatusCode)
                    return Results.BadRequest(json);

                using var doc = JsonDocument.Parse(json);
                var root = doc.RootElement;

                if (root.ValueKind != JsonValueKind.Array)
                    return Results.BadRequest("Expected array");

                // 🔥 STEP 1: Get incoming IDs
                var incomingRefNos = new HashSet<string>();
                var incomingApplIds = new HashSet<string>();

                foreach (var item in root.EnumerateArray())
                {
                    incomingRefNos.Add(item.GetProperty("appl_ref_no").GetString() ?? "");
                    incomingApplIds.Add(item.GetProperty("appl_id").GetString() ?? "");
                }

                // 🔥 STEP 2: Fetch existing records (single DB call)
                var existing = await db.ServiceApplications
                    .Where(x => incomingRefNos.Contains(x.ApplRefNo) || incomingApplIds.Contains(x.ApplId))
                    .Select(x => new { x.ApplRefNo, x.ApplId })
                    .ToListAsync();

                var existingRefNos = existing.Select(x => x.ApplRefNo).ToHashSet();
                var existingApplIds = existing.Select(x => x.ApplId).ToHashSet();

                var newApplications = new List<ServiceApplication>();

                // 🔥 STEP 3: Filter + build entities
                foreach (var item in root.EnumerateArray())
                {
                    var refNo = item.GetProperty("appl_ref_no").GetString() ?? "";
                    var applId = item.GetProperty("appl_id").GetString() ?? "";

                    if (existingRefNos.Contains(refNo) || existingApplIds.Contains(applId))
                        continue; // 🚫 skip duplicates

                    var appEntity = new ServiceApplication
                    {
                        ApplRefNo = refNo,
                        ApplId = applId,
                        AppliedBy = item.GetProperty("applied_by").GetString() ?? ""
                    };

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

                    newApplications.Add(appEntity);
                }

                // 🔥 STEP 4: Bulk insert
                if (newApplications.Count > 0)
                {
                    await db.ServiceApplications.AddRangeAsync(newApplications);
                    await db.SaveChangesAsync();
                }

                return Results.Ok(new
                {
                    inserted = newApplications.Count,
                    skipped = incomingRefNos.Count - newApplications.Count
                });
            });


        }
    }
}
