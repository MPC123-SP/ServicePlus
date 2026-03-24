using Microsoft.EntityFrameworkCore;
using ServicePlusEXT.Context;
using ServicePlusEXT.ResponseDtos;
using ServicePlusEXT.Shared;
using ServicePlusEXT.Shared.Services;
using System.Text.RegularExpressions;

namespace ServicePlusEXT.Endpoints
{
    public class ReportEndpoint : IEndpoint
    {
        public void Map(IEndpointRouteBuilder app)
        {
            app.MapGet("/reports/sssb", GetReportAsync);
            app.MapPost("/reports/sssb/export-images", ExportImagesAsync);
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

        private static async Task<IResult> ExportImagesAsync(
            AppDbContext db,
            CancellationToken cancellationToken)
        {
            var applications = await db.ServiceApplications
                .AsNoTracking()
                .OrderBy(application => application.Id)
                .Select(application => new
                {
                    application.ApplRefNo,
                    Attributes = application.Attributes
                        .Where(attribute =>
                            attribute.Key == ServiceApplicationReportDefinition.ApplicantProfileAttributeKey ||
                            attribute.Key == ServiceApplicationReportDefinition.SignatureAttributeKey)
                        .Select(attribute => new
                        {
                            attribute.Key,
                            attribute.Value
                        })
                        .ToList()
                })
                .ToListAsync(cancellationToken);

            var rootDirectory = Path.Combine(Directory.GetCurrentDirectory(), "PSSSB");
            var photoDirectory = Path.Combine(rootDirectory, "Photo");
            var signatureDirectory = Path.Combine(rootDirectory, "Signature");
            Directory.CreateDirectory(rootDirectory);
            Directory.CreateDirectory(photoDirectory);
            Directory.CreateDirectory(signatureDirectory);

            var exportedApplicantImages = 0;
            var exportedSignatures = 0;
            var skippedImages = 0;

            foreach (var application in applications)
            {
                var safeFileName = BuildImageFileName(application.ApplRefNo);

                foreach (var attribute in application.Attributes)
                {
                    if (!TryDecodeImage(attribute.Value, out var imageBytes))
                    {
                        skippedImages++;
                        continue;
                    }

                    var isPhoto =
                        attribute.Key == ServiceApplicationReportDefinition.ApplicantProfileAttributeKey;

                    var fileName = isPhoto
                        ? $"{safeFileName}_photo.jpg"
                        : $"{safeFileName}_sign.jpg";

                    var targetDirectory = isPhoto ? photoDirectory : signatureDirectory;
                    var filePath = Path.Combine(targetDirectory, fileName);
                    await File.WriteAllBytesAsync(filePath, imageBytes, cancellationToken);

                    if (isPhoto)
                    {
                        exportedApplicantImages++;
                    }
                    else
                    {
                        exportedSignatures++;
                    }
                }
            }

            return Results.Ok(new
            {
                rootDirectory,
                photoDirectory,
                signatureDirectory,
                exportedApplicantImages,
                exportedSignatures,
                skippedImages
            });
        }

        private static string BuildImageFileName(string applicationReferenceNo)
        {
            var sanitizedReferenceNo = Regex.Replace(
                applicationReferenceNo,
                "[^A-Za-z0-9._-]",
                "",
                RegexOptions.CultureInvariant);

            return sanitizedReferenceNo;
        }

        private static bool TryDecodeImage(string value, out byte[] imageBytes)
        {
            imageBytes = Array.Empty<byte>();

            if (string.IsNullOrWhiteSpace(value))
            {
                return false;
            }

            var normalizedValue = value.StartsWith(
                ServiceApplicationReportDefinition.Base64ImagePrefix,
                StringComparison.OrdinalIgnoreCase)
                ? value[ServiceApplicationReportDefinition.Base64ImagePrefix.Length..]
                : value;

            try
            {
                imageBytes = Convert.FromBase64String(normalizedValue);
                return true;
            }
            catch (FormatException)
            {
                return false;
            }
        }
    }
}
