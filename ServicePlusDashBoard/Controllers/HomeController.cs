using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Newtonsoft.Json;
using OfficeOpenXml;
using PdfSharpCore;
using PdfSharpCore.Drawing;
using PdfSharpCore.Pdf;
using ServicePlusDashBoard.Helper;
using ServicePlusDashBoard.ViewModel;
using System.Security.Authentication;
using ServicePlusDashBoard.Models;

namespace ServicePlusDashBoard.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        private readonly PermissionService _permissionService;
        private readonly HttpClient _httpClient;
        private readonly IHttpContextAccessor _httpContextAccessor;
        public HomeController(PermissionService permissionService, IHttpClientFactory httpClientFactory, HttpClient httpClient, IHttpContextAccessor httpContextAccessor)
        {
            _permissionService = permissionService;
            _httpClient = httpClientFactory.CreateClient("ServicePlusClient");
            _httpContextAccessor = httpContextAccessor;
        }
      
         
        public async Task<IActionResult> Index()
        {
            string url = "http://10.147.24.36:8082/api/ServicePlus/PendencyReport"; // Replace with your desired URL
            using (var httpClientHandler = new HttpClientHandler())
            {
                // Set TLS version 
                httpClientHandler.SslProtocols = SslProtocols.Tls12 | SslProtocols.Tls13;

                // Ignore SSL certificate validation (not recommended for production)
                httpClientHandler.ServerCertificateCustomValidationCallback =
                    (sender, certificate, chain, sslPolicyErrors) => true;

                using (var httpClient = new HttpClient(httpClientHandler))
                {
                    HttpResponseMessage response = await httpClient.GetAsync(url);
                    if (response.IsSuccessStatusCode)
                    {
                        string content = await response.Content.ReadAsStringAsync();
                        List<PendencyReportViewModel> dataList = JsonConvert.DeserializeObject<List<PendencyReportViewModel>>(content);
                        ViewBag.DataPoints = dataList;
                        // Calculate the sum of "Deliverd" counts
                        int totalDeliverdCount = dataList.Sum(item => item.Deliverd);
                        int totalRejectedCount = dataList.Sum(item => item.Rejected);
                        int totalInProcessCount = dataList.Sum(item => item.InProcess);
                        int totalRecieved = dataList.Sum(item => item.ApplicationRecieved);
                        int totalSendBack = dataList.Sum(item => item.SendBack);

                        // You can use totalDeliverdCount as needed, for example, passing it to the view
                        ViewBag.TotalRecievedCount = totalRecieved;
                        ViewBag.TotalDeliverdCount = totalDeliverdCount;
                        ViewBag.TotalInProcessCount = totalInProcessCount;
                        ViewBag.TotalRejectedCount = totalRejectedCount;    
                         
                    }
                    

                }
            }

            return View();
        }


        public async Task<IActionResult>PendencyReport()
        {
            string url = "http://10.147.24.36:8083/api/ServicePlus/PendencyReport"; // Replace with your desired URL
            using (var httpClientHandler = new HttpClientHandler())
            {
                // Set TLS version 
                httpClientHandler.SslProtocols = SslProtocols.Tls12 | SslProtocols.Tls13;

                // Ignore SSL certificate validation (not recommended for production)
                httpClientHandler.ServerCertificateCustomValidationCallback =
                    (sender, certificate, chain, sslPolicyErrors) => true;

                using (var httpClient = new HttpClient(httpClientHandler))
                {
                    var jwtToken = Request.Cookies["jwtToken"];

                    httpClient.DefaultRequestHeaders.Add("Authorization", "Bearer " + jwtToken);

                    HttpResponseMessage response = await httpClient.GetAsync(url);
                    if (response.IsSuccessStatusCode)
                    {
                        var content = await response.Content.ReadAsStringAsync();
                        return Content(content, "application/json");
                    }
                    else
                    {
                        // Handle the error as needed
                        return StatusCode((int)response.StatusCode);
                    }
                }
            } 
        }

        public async Task<IActionResult> GetSewaKendraWiseReport(int draw, int start, int length, string searchValue, int sortColumn, string sortDirection, string fromDate, string toDate)
        {
            string url = $"http://10.147.24.36:8083/api/ServicePlus/GetSewaKendraWiseReport?draw={draw}&start={start}&length={length}&searchValue={searchValue}&sortColumn={sortColumn}&sortDirection={sortDirection}&fromDate={fromDate}&toDate={toDate}";
            using (var httpClientHandler = new HttpClientHandler())
            {
                // Set TLS version 
                httpClientHandler.SslProtocols = SslProtocols.Tls12 | SslProtocols.Tls13;

                // Ignore SSL certificate validation (not recommended for production)
                httpClientHandler.ServerCertificateCustomValidationCallback =
                    (sender, certificate, chain, sslPolicyErrors) => true;

                using (var httpClient = new HttpClient(httpClientHandler))
                {
                    var jwtToken = Request.Cookies["jwtToken"];

                    httpClient.DefaultRequestHeaders.Add("Authorization", "Bearer " + jwtToken);

                    HttpResponseMessage response = await httpClient.GetAsync(url);
                    if (response.IsSuccessStatusCode)
                    {
                        var content = await response.Content.ReadAsStringAsync();
                        return Content(content, "application/json");
                    }
                    else
                    {
                        // Handle the error as needed
                        return StatusCode((int)response.StatusCode);
                    }
                }
            } 
        }

        public async Task<IActionResult> GetSewaKendraZoneWiseReport(int draw, int start, int length, string searchValue, int sortColumn, string sortDirection, DateTime? fromDate, DateTime? toDate, string zoneType)
        {
            string formattedFromDate = fromDate.HasValue ? fromDate.Value.ToString("yyyy-MM-dd") : null;
            string formattedToDate = toDate.HasValue ? toDate.Value.ToString("yyyy-MM-dd") : null;

            string url = $"http://10.147.24.36:8083/api/ServicePlus/GetSewaKendraZoneWiseReport?draw={draw}&start={start}&length={length}&searchValue={searchValue}&sortColumn={sortColumn}&sortDirection={sortDirection}&fromDate={formattedFromDate}&toDate={formattedToDate}&zoneType={zoneType}";

            using (var httpClientHandler = new HttpClientHandler())
            {
                // Set TLS version 
                httpClientHandler.SslProtocols = SslProtocols.Tls12 | SslProtocols.Tls13;

                // Ignore SSL certificate validation (not recommended for production)
                httpClientHandler.ServerCertificateCustomValidationCallback =
                    (sender, certificate, chain, sslPolicyErrors) => true;

                using (var httpClient = new HttpClient(httpClientHandler))
                {
                    var jwtToken = Request.Cookies["jwtToken"];

                    httpClient.DefaultRequestHeaders.Add("Authorization", "Bearer " + jwtToken);

                    HttpResponseMessage response = await httpClient.GetAsync(url);
                    if (response.IsSuccessStatusCode)
                    {
                        var content = await response.Content.ReadAsStringAsync();
                        return Content(content, "application/json");
                    }
                    else
                    {
                        // Handle the error as needed
                        return StatusCode((int)response.StatusCode);
                    }
                }
            } 
        }
  
        public IActionResult Profile()
        {
            return View();
        }
        public IActionResult Setting()
        {
            return View();
        }
        public IActionResult TransationDetail()
        {
            return View();
        }
        public async Task<IActionResult> Reports()
        {
            var reportNameUrl = "http://10.147.24.36:8083/api/Authenticate/GetReportName";
            var reportsName =await  SendHttpGetRequest<string>(reportNameUrl);           

            List<ZoneTypes> ZoneTypes = new List<ZoneTypes>()
            {
                new ZoneTypes(){ZoneTypeId="1~ZONE I",ZoneTypeName="ZONE I" },
                new ZoneTypes(){ZoneTypeId="2~ZONE II",ZoneTypeName="ZONE II" },
                new ZoneTypes(){ZoneTypeId="3~ZONE III",ZoneTypeName="ZONE III" }
            };
            // Convert the reportsName list to a SelectList of SelectListItem
         
            SelectList zoneTypesSelectList= new SelectList(ZoneTypes, "ZoneTypeId", "ZoneTypeName");

            
            ViewBag.zoneName = zoneTypesSelectList;
            return View();
        }
        
        public async Task<IActionResult> PendencyExcel()
        {
            string url = "http://10.147.24.36:8082/api/ServicePlus/PendencyReport"; // Replace with your desired URL
            using (var httpClientHandler = new HttpClientHandler())
            {
                // Set TLS version 
                httpClientHandler.SslProtocols = SslProtocols.Tls12 | SslProtocols.Tls13;

                // Ignore SSL certificate validation (not recommended for production)
                httpClientHandler.ServerCertificateCustomValidationCallback =
                    (sender, certificate, chain, sslPolicyErrors) => true;

                using (var httpClient = new HttpClient(httpClientHandler))
                {
                    HttpResponseMessage response = await httpClient.GetAsync(url);
                    if (response.IsSuccessStatusCode)
                    {
                        string content = await response.Content.ReadAsStringAsync();
                        List<PendencyReportViewModel> dataList = JsonConvert.DeserializeObject<List<PendencyReportViewModel>>(content);

                        // ...

                        //// Create a new Excel package
                        using (var package = new ExcelPackage())
                        {
                            var worksheet = package.Workbook.Worksheets.Add("Report");

                            // Add headers
                            if (dataList.Count > 0)
                            {
                                var headers = dataList[0].GetType().GetProperties(); // Get properties of the object
                                int col = 1;
                                foreach (var header in headers)
                                {
                                    worksheet.Cells[1, col].Value = header.Name; // Use property name as header
                                    col++;
                                }

                                // Add data
                                int row = 2;
                                foreach (var rowData in dataList)
                                {
                                    col = 1;
                                    foreach (var header in headers)
                                    {
                                        var value = header.GetValue(rowData); // Get property value using reflection
                                        worksheet.Cells[row, col].Value = value;
                                        col++;
                                    }
                                    row++;
                                }
                            }

                            // Save the Excel package to a file
                            var excelBytes = package.GetAsByteArray();
                            var fileName = "PendencyReport.xlsx";

                            // Provide the Excel data and file name to the FileResult
                            return File(excelBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);

                        }



                    }
                    else
                    {

                    }

                }
            }

            return View();
        }
        public async Task<IActionResult> PendencyPDF()
        {
            var pdfDocument = new PdfDocument();
            var page = pdfDocument.AddPage();
            // Set the page orientation to Landscape
            page.Orientation = PageOrientation.Landscape;
            var gfx = XGraphics.FromPdfPage(page);
            var font = new XFont("Arial", 6); 
        
            int yOffset = 10;
            int columnWidth = 75;

            string url = "http://10.147.24.36:8082/api/ServicePlus/PendencyReport"; // Replace with your desired URL
            using (var httpClientHandler = new HttpClientHandler())
            {
                // Set TLS version 
                httpClientHandler.SslProtocols = SslProtocols.Tls12 | SslProtocols.Tls13;

                // Ignore SSL certificate validation (not recommended for production)
                httpClientHandler.ServerCertificateCustomValidationCallback =
                    (sender, certificate, chain, sslPolicyErrors) => true;

                using (var httpClient = new HttpClient(httpClientHandler))
                {
                    HttpResponseMessage response = await httpClient.GetAsync(url);
                    if (response.IsSuccessStatusCode)
                    {
                        string content = await response.Content.ReadAsStringAsync();
                        List<PendencyReportViewModel> dataList = JsonConvert.DeserializeObject<List<PendencyReportViewModel>>(content);

                        // Draw table headers
                        gfx.DrawString("District Name", font, XBrushes.Black, 20, yOffset);
                        gfx.DrawString("Total Application Received", font, XBrushes.Black, 20 + columnWidth * 1, yOffset);
                        gfx.DrawString("Total Application Deliverd", font, XBrushes.Black, 20 + columnWidth * 2, yOffset);
                        gfx.DrawString("Total Application Rejected", font, XBrushes.Black, 20 + columnWidth * 3, yOffset);
                        gfx.DrawString("Total Application InProcess", font, XBrushes.Black, 20 + columnWidth * 4, yOffset);
                        gfx.DrawString("Pending b/w 1 to 5 Days", font, XBrushes.Black, 20 + columnWidth * 5, yOffset);
                        gfx.DrawString("Pending b/w 6 to 30 Days", font, XBrushes.Black, 20 + columnWidth * 6, yOffset);
                        gfx.DrawString("Pending b/w 31 to 60 Days", font, XBrushes.Black, 20 + columnWidth * 7, yOffset);
                        gfx.DrawString("Pending b/w 91 And Days", font, XBrushes.Black, 20 + columnWidth * 8, yOffset);
                        gfx.DrawString("Send Back", font, XBrushes.Black, 20 + columnWidth * 9, yOffset);
                        gfx.DrawString("Pendency of Total Application", font, XBrushes.Black, 20 + columnWidth * 10, yOffset);
                        // Add more header columns as needed
                       
                        yOffset += 20;

                        // Draw data rows
                        foreach (var report in dataList)
                        {
                            gfx.DrawString(report.DistrictName, font, XBrushes.Black, 20, yOffset);
                            gfx.DrawString(report.ApplicationRecieved.ToString(), font, XBrushes.Black, 20 + columnWidth * 1, yOffset);
                            gfx.DrawString(report.Deliverd.ToString(), font, XBrushes.Black, 20 + columnWidth * 2, yOffset);
                            gfx.DrawString(report.Rejected.ToString(), font, XBrushes.Black, 20 + columnWidth * 3, yOffset);
                            gfx.DrawString(report.InProcess.ToString(), font, XBrushes.Black, 20 + columnWidth * 4, yOffset);
                            gfx.DrawString(report.Day1to5.ToString(), font, XBrushes.Black, 20 + columnWidth * 5, yOffset);
                            gfx.DrawString(report.Day31to60.ToString(), font, XBrushes.Black, 20 + columnWidth * 6, yOffset);
                            gfx.DrawString(report.Day61to90.ToString(), font, XBrushes.Black, 20 + columnWidth * 7, yOffset);
                            gfx.DrawString(report.Day91toAbove.ToString(), font, XBrushes.Black, 20 + columnWidth * 8, yOffset);
                            gfx.DrawString(report.SendBack.ToString(), font, XBrushes.Black, 20 + columnWidth * 9, yOffset);
                            gfx.DrawString(report.PendencyPercentage.ToString(), font, XBrushes.Black, 20 + columnWidth * 10, yOffset);
                            // Add more data columns as needed

                            yOffset += 20;
                        }

                        var pdfFileName = "PendencyReport.pdf";
                        pdfDocument.Save(pdfFileName);

                        // Return the PDF file as a download
                        byte[] pdfBytes;
                        using (MemoryStream stream = new MemoryStream())
                        {
                            pdfDocument.Save(stream, false);
                            pdfBytes = stream.ToArray();
                        }

                        return File(pdfBytes, "application/pdf", pdfFileName);
                    }
                    else
                    {
                        // Handle error response
                        // For example, return an error view
                        return View("Error");
                    }
                }
            }
        }

        private async Task<List<string>> SendHttpGetRequest<T>(string url)
        {

            var jwtToken = Request.Cookies["jwtToken"];
            var httpClientHandler = new HttpClientHandler
            {
                // Set TLS version 
                SslProtocols = SslProtocols.Tls12 | SslProtocols.Tls13,

                // Ignore SSL certificate validation (not recommended for production)
                ServerCertificateCustomValidationCallback =
                    (sender, certificate, chain, sslPolicyErrors) => true
            };

            using (var httpClient = new HttpClient(httpClientHandler))
            {
                httpClient.DefaultRequestHeaders.Add("Authorization", "Bearer " + jwtToken);
                HttpResponseMessage response = await httpClient.GetAsync(url);
                if (response.IsSuccessStatusCode)
                {
                    string content = await response.Content.ReadAsStringAsync();
                    return JsonConvert.DeserializeObject<List<string>>(content);
                }
                else
                {
                    // Handle the error here if needed
                    return null;
                }
            }
        }

        [HttpGet]
        public async Task<IActionResult>SewaKendraWiseIncomeReport()
        {
            return View();
        }
        [HttpGet]
        public async Task<IActionResult> SewaKendraZoneWiseIncomeReport()
        {
            List<ZoneTypes> ZoneTypes = new List<ZoneTypes>()
            {
                new ZoneTypes(){ZoneTypeId="1~ZONE I",ZoneTypeName="ZONE I" },
                new ZoneTypes(){ZoneTypeId="2~ZONE II",ZoneTypeName="ZONE II" },
                new ZoneTypes(){ZoneTypeId="3~ZONE III",ZoneTypeName="ZONE III" }
            }; 
            SelectList zoneTypesSelectList = new SelectList(ZoneTypes, "ZoneTypeId", "ZoneTypeName");
             
            ViewBag.zoneName = zoneTypesSelectList;
            return View();
        }
       
        [HttpGet]
        public async Task< IActionResult> DynamicReport()
        {
            var initiatedDataColumns = GetPropertyNames(typeof(InitiatedData));
            ViewBag.InitiatedColumns = initiatedDataColumns;
            string urlServiceName = "http://10.147.24.36:8083/api/ServicePlus/GetServicesName";
            var serviceNames = await SendHttpGetRequest<string>(urlServiceName);            
            ViewBag.ServiceNames = serviceNames;
            return View();
        }

        [HttpPost]
        public IActionResult DynamicReport(List<string> selectedColumns,string serviceName , string fromDate, string toDate, int draw, int start, int length)
        {
            // Process the selected columns as needed
            if (selectedColumns != null)
            {
                foreach (var column in selectedColumns)
                {
                    // Process each selected column
                }
            }
            

            return View();
        }

        public static List<string> GetPropertyNames(Type type)
        {
            return type.GetProperties()
                .Select(propertyInfo => propertyInfo.Name)
                .ToList();
        }

    }
}
