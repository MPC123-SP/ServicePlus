using Google.Apis.Auth.OAuth2;
using Google.Apis.Services;
using Google.Apis.Sheets.v4;
using Google.Apis.Sheets.v4.Data;
using Microsoft.Extensions.Caching.Memory;
using ServicePlusAPIs.ViewModels.PublicModel;
using System.Collections.Concurrent;

namespace ServicePlusAPIs.ExternalAPIs
{
    public class GoogleSheetsService
    {
        private static readonly string[] Scopes = { SheetsService.Scope.SpreadsheetsReadonly };
        private static readonly string ApplicationName = "Sports Service";
        private static readonly string SpreadsheetId = "1BuUjoz_01GH8PXdN1A02G54CMtiDBR5sZvWZgHq-Xnc"; // Your Google Sheet ID
        private static readonly string SheetName = "Sheet1"; // Adjust if needed
        private static readonly string CredentialsFilePath = "C:\\Users\\HP\\OneDrive\\Documents\\GitHub\\ServicePlus\\ServicePlusAPIs\\ExternalAPIs\\SportsServiceAccount.json";

        private static readonly MemoryCache _cache = new MemoryCache(new MemoryCacheOptions());

        // Cache Key
        private const string PlayerCertificateCacheKey = "PlayerCertificateDetailsCache";

        public static async Task<PlayerCertificateDetail?> GetFilteredPlayerCertificateDetails(
    string dob = null, string game = null, string gameEvent = null, string ageGroup = null)
        {
            // Ensure parameters are normalized
            dob = dob?.Trim().ToLowerInvariant();
            game = game?.Trim().ToLowerInvariant();
            gameEvent = gameEvent?.Trim().ToLowerInvariant();
            ageGroup = ageGroup?.Trim().ToLowerInvariant();

            // Try to get cached records
            if (_cache.TryGetValue(PlayerCertificateCacheKey, out List<PlayerCertificateDetail> cachedRecords))
            {
                return cachedRecords.FirstOrDefault(d =>
                    d.ApplicantDOB?.Trim().ToLowerInvariant() == dob &&
                    d.ApplicantGame?.Trim().ToLowerInvariant() == game &&
                    d.ApplicantEvent?.Trim().ToLowerInvariant() == gameEvent &&
                    d.ApplicantAgeGroup?.Trim().ToLowerInvariant() == ageGroup
                );
            }

            // If cache is empty, fetch data from Google Sheets
            try
            {
                GoogleCredential credential;
                using (var stream = new FileStream(CredentialsFilePath, FileMode.Open, FileAccess.Read))
                {
                    credential = GoogleCredential.FromStream(stream).CreateScoped(Scopes);
                }

                var service = new SheetsService(new BaseClientService.Initializer
                {
                    HttpClientInitializer = credential,
                    ApplicationName = ApplicationName,
                });

                var range = $"'{SheetName}'!A1:M";
                var request = service.Spreadsheets.Values.Get(SpreadsheetId, range);
                ValueRange response = await request.ExecuteAsync();
                var values = response.Values;

                if (values == null || values.Count == 0)
                    return null;

                var records = values.Skip(1) // Assuming first row is headers
                    .Select(row => new PlayerCertificateDetail
                    {
                        ApplicantFullName = row.ElementAtOrDefault(1)?.ToString()?.Trim(),
                        ApplicantFatherName = row.ElementAtOrDefault(2)?.ToString()?.Trim(),
                        ApplicantDOB = row.ElementAtOrDefault(3)?.ToString()?.Trim(),
                        ApplicantMobileNo = row.ElementAtOrDefault(4)?.ToString()?.Trim(),
                        GameHeldDistrict = row.ElementAtOrDefault(5)?.ToString()?.Trim(),
                        GameRepresentingDistrict = row.ElementAtOrDefault(6)?.ToString()?.Trim(),
                        ApplicantGame = row.ElementAtOrDefault(7)?.ToString()?.Trim(),
                        ApplicantEvent = row.ElementAtOrDefault(8)?.ToString()?.Trim(),
                        ApplicantAgeGroup = row.ElementAtOrDefault(9)?.ToString()?.Trim(),
                        Score = row.ElementAtOrDefault(10)?.ToString()?.Trim(),
                        Position = row.ElementAtOrDefault(11)?.ToString()?.Trim(),
                        ConveyorName = row.ElementAtOrDefault(12)?.ToString()?.Trim()
                    })
                    .ToList();

                // Store in cache for 30 minutes
                _cache.Set(PlayerCertificateCacheKey, records, TimeSpan.FromMinutes(10));

                // Filter and return only the matching record
                return records.FirstOrDefault(d =>
                    d.ApplicantDOB?.ToLowerInvariant() == dob &&
                    d.ApplicantGame?.ToLowerInvariant() == game &&
                    d.ApplicantEvent?.ToLowerInvariant() == gameEvent &&
                    d.ApplicantAgeGroup?.ToLowerInvariant() == ageGroup
                );
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error accessing Google Sheets API: {ex.Message}");
                return null;
            }
        }

    }
}
