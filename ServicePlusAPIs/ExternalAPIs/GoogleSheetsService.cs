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
        private static readonly string SpreadsheetId = "1EJQ1YPh-lD9bihwXW2M_CCsETJMBzXbWZ5F4cBVnZbg"; // Your Google Sheet ID
        private static readonly string SheetName = "Sheet1"; // Adjust if needed
        private static readonly string CredentialsFilePath = "C:\\Users\\Mohit\\Documents\\GitHub\\ServicePlus\\ServicePlusAPIs\\ExternalAPIs\\SportsServiceAccount.json";

        private static readonly MemoryCache _cache = new MemoryCache(new MemoryCacheOptions());

        // Cache Key
        private const string PlayerCertificateCacheKey = "PlayerCertificateDetailsCache";

        public static async Task<List<PlayerCertificateDetail>> GetFilteredPlayerCertificateDetails(
            string playerName = null, string game = null, string ageGroup = null)
        {
            // Try to get the cached records
            if (_cache.TryGetValue(PlayerCertificateCacheKey, out List<PlayerCertificateDetail> cachedRecords))
            {
                // If cache is found, return it
                return cachedRecords;
            }

            // If cache is not found, call Google Sheets API to fetch data
            try
            {
                GoogleCredential credential;
                using (var stream = new FileStream(CredentialsFilePath, FileMode.Open, FileAccess.Read))
                {
                    credential = GoogleCredential.FromStream(stream).CreateScoped(Scopes);
                }

                var service = new SheetsService(new BaseClientService.Initializer()
                {
                    HttpClientInitializer = credential,
                    ApplicationName = ApplicationName,
                });

                // Define range to fetch all columns (A to M)
                var range = $"'{SheetName}'!A1:M";
                var request = service.Spreadsheets.Values.Get(SpreadsheetId, range);
                ValueRange response = await request.ExecuteAsync();
                var values = response.Values;

                if (values == null || values.Count == 0)
                    return new List<PlayerCertificateDetail>();

                var filteredRecords = new ConcurrentBag<PlayerCertificateDetail>(); // Thread-safe collection

                // Use Parallel.ForEach for concurrent processing
                await Task.WhenAll(values.Select(async row =>
                {
                    var record = new PlayerCertificateDetail
                    {
                        ApplicantFullName = row.Count > 1 ? row[1]?.ToString() ?? "" : "",
                        ApplicantFatherName = row.Count > 2 ? row[2]?.ToString() ?? "" : "",
                        ApplicantDOB = row.Count > 3 ? row[3]?.ToString() ?? "" : "",
                        ApplicantMobileNo = row.Count > 4 ? row[4]?.ToString() ?? "" : "",
                        GameHeldDistrict = row.Count > 5 ? row[5]?.ToString() ?? "" : "",
                        GameRepresentingDistrict = row.Count > 6 ? row[6]?.ToString() ?? "" : "",
                        ApplicantGame = row.Count > 7 ? row[7]?.ToString() ?? "" : "",
                        ApplicantEvent = row.Count > 8 ? row[8]?.ToString() ?? "" : "",
                        ApplicantAgeGroup = row.Count > 9 ? row[9]?.ToString() ?? "" : "",
                        Score = row.Count > 10 ? row[10]?.ToString() ?? "" : "",
                        Position = row.Count > 11 ? row[11]?.ToString() ?? "" : "",
                        ConveyorName = row.Count > 12 ? row[12]?.ToString() ?? "" : ""
                    };

                    // Add record to the collection
                    filteredRecords.Add(record);
                }));

                // Convert to List and store in cache with expiration time (30 minutes)
                var result = filteredRecords.ToList();
                var cacheExpiration = DateTime.Now.AddMinutes(1);
                _cache.Set(PlayerCertificateCacheKey, result, cacheExpiration);

                return result;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error accessing Google Sheets API: {ex.Message}");
                return new List<PlayerCertificateDetail>();
            }
        }
    }
}
