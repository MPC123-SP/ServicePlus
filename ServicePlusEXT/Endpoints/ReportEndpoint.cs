using Microsoft.EntityFrameworkCore;
using ServicePlusEXT.Context;
using ServicePlusEXT.ResponseDtos;
using ServicePlusEXT.Shared;
using ServicePlusEXT.Shared.Services;

namespace ServicePlusEXT.Endpoints
{
    public class ReportEndpoint : IEndpoint
    {
        public void Map(IEndpointRouteBuilder app)
        {
            app.MapGet("/reports/sssb", GetReportAsync);
        }

        private static async Task<IResult> GetReportAsync(
            AppDbContext db,
            string? cursor,
            int? pageSize,
            CancellationToken cancellationToken)
        {
            if (!CursorPagination.TryDecodeInt32(cursor, out var lastSeenId))
            {
                return Results.BadRequest("Invalid cursor.");
            }

            var take = pageSize.GetValueOrDefault(50);
            take = Math.Clamp(take, 1, 200);

            var applications = await db.ServiceApplications
                .AsNoTracking()
                .Where(application => application.Id > lastSeenId)
                .OrderBy(application => application.Id)
                .Select(application => new
                {
                    application.Id,
                    application.ApplRefNo
                })
                .Take(take + 1)
                .ToListAsync(cancellationToken);

            var hasMore = applications.Count > take;
            var currentPage = hasMore ? applications.Take(take).ToList() : applications;

            var applicationIds = currentPage
                .Select(application => application.Id)
                .ToArray();

            var attributeLookup = applicationIds.Length == 0
                ? new Dictionary<int, Dictionary<string, string>>()
                : await db.ApplicationAttributes
                    .AsNoTracking()
                    .Where(attribute =>
                        applicationIds.Contains(attribute.ServiceApplicationId) &&
                        ServiceApplicationReportDefinition.AttributeKeys.Contains(attribute.Key))
                    .GroupBy(attribute => attribute.ServiceApplicationId)
                    .ToDictionaryAsync(
                        group => group.Key,
                        group => group.ToDictionary(
                            attribute => attribute.Key,
                            attribute => attribute.Value,
                            StringComparer.Ordinal),
                        cancellationToken);

            var rows = currentPage
                .Select(application =>
                {
                    attributeLookup.TryGetValue(application.Id, out var attributes);
                    attributes ??= new Dictionary<string, string>(StringComparer.Ordinal);

                    var values = new Dictionary<string, string>(StringComparer.Ordinal)
                    {
                        ["id"] = application.Id.ToString(),
                        ["applRefNo"] = application.ApplRefNo,
                        ["amount"] = string.Empty
                    };

                    foreach (var column in ServiceApplicationReportDefinition.Columns)
                    {
                        if (column.AttributeKey is null || !attributes.TryGetValue(column.AttributeKey, out var value))
                        {
                            continue;
                        }

                        values[column.Field] = ServiceApplicationReportDefinition.FormatAttributeValue(column.AttributeKey, value);
                    }

                    return new ServiceApplicationReportRowDto
                    {
                        Id = application.Id,
                        ApplRefNo = application.ApplRefNo,
                        Values = values
                    };
                })
                .ToList();

            var nextCursor = hasMore && currentPage.Count > 0
                ? CursorPagination.EncodeInt32(currentPage[^1].Id)
                : null;

            var response = new ServiceApplicationReportResponseDto
            {
                Columns = ServiceApplicationReportDefinition.Columns
                    .Select(column => new ReportColumnDto(column.Header, column.Field))
                    .ToList(),
                Items = rows,
                HasMore = hasMore,
                NextCursor = nextCursor,
                PageSize = take
            };

            return Results.Ok(response);
        }
    }
}
