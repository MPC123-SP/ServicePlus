using Google.Apis.Auth.OAuth2;
using Google.Apis.Services;
using Google.Apis.Sheets.v4;
using Google.Apis.Sheets.v4.Data;
using Microsoft.Extensions.Caching.Memory;
using ServicePlusAPIs.Context;
using ServicePlusAPIs.Models.SportsModel;
using ServicePlusAPIs.ViewModels.PublicModel;
using System.Collections.Concurrent;

namespace ServicePlusAPIs.ExternalAPIs
{
    /// <summary>
    /// currently we are using google spreadsheet ,SheetName:FinalWinnerData  and Sheet 1 
    /// sheet is created by akash sir 
    /// </summary>
    public class GoogleSheetsService
    {
        private readonly ServicePlusContext _servicePlusContext;
        private static readonly string[] Scopes = { SheetsService.Scope.SpreadsheetsReadonly };
        private static readonly string ApplicationName = "Sports Service";
        private static readonly string SpreadsheetId = "1BuUjoz_01GH8PXdN1A02G54CMtiDBR5sZvWZgHq-Xnc"; // Your Google Sheet ID
        private static readonly string SheetName = "Sheet1"; // Adjust if needed
        private static readonly string CredentialsFilePath = "C:\\Users\\Mohit\\Documents\\GitHub\\ServicePlus\\ServicePlusAPIs\\ExternalAPIs\\SportsServiceAccount.json";


        public GoogleSheetsService(ServicePlusContext context)
        {
            _servicePlusContext = context;
        }
        public async Task<bool> GetFilteredPlayerCertificateDetails()
        {

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
                    return false;

                var records = values.Skip(1) // Assuming first row is headers
                    .Select(row => new PlayerCertificateDetails
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
                _servicePlusContext.PlayerCertificateDetails.RemoveRange(_servicePlusContext.PlayerCertificateDetails);
                await _servicePlusContext.PlayerCertificateDetails.AddRangeAsync(records);
                await _servicePlusContext.SaveChangesAsync();
                return true;

            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error accessing Google Sheets API: {ex.Message}");
                return false;
            } 
        }

    }
}
