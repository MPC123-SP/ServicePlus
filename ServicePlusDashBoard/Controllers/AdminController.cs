using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ServicePlusDashBoard.Helper;
using System.Security.Authentication;

namespace ServicePlusDashBoard.Controllers
{
    public class AdminController : Controller
    {
        private readonly HttpClient _httpClient;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public AdminController(IHttpClientFactory httpClientFactory, HttpClient httpClient, IHttpContextAccessor httpContextAccessor)
        {
            _httpClient = httpClientFactory.CreateClient("ServicePlusClient");
            _httpContextAccessor = httpContextAccessor;
        }
        [Authorize(Roles ="SuperAdmin")]
        public IActionResult Index()
        {
            return View();
        } 
        public IActionResult CreateUser()
        {
            return RedirectToAction("CreateUser", "Account");
        }  
        public IActionResult UpdateReports()
        {
            return View();
        } 
        public async Task<IActionResult> ReceivedJsonDailyBasis(int page, int pageSize)
        {
            if(page==0 && pageSize==0)
            {
                page=1;
                pageSize = 10;
            }
            
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

                    HttpResponseMessage response = await _httpClient.GetAsync($"{ApiEndPoints.JSONReceivedDatesEndPoint}?page={page}&pageSize={pageSize}");
                    if (response.IsSuccessStatusCode)
                    {
                        Console.WriteLine($"Response headers: {string.Join(", ", response.Headers)}");
                        foreach (var header in response.Headers)
                        {
                            // Print each header's name and its values
                            Console.WriteLine($"{header.Key}: {string.Join(", ", header.Value)}");
                        }
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

        [HttpGet]
        public IActionResult ReceivedJson()
        {
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> ConsolidateReports(int page, int pageSize)
        {
            if (page == 0 && pageSize == 0)
            {
                page = 1;
                pageSize = 10;
            } 
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

                    HttpResponseMessage response = await _httpClient.GetAsync($"{ApiEndPoints.ConsolidateReportEndPoint}?page={page}&pageSize={pageSize}");
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

        [HttpGet]
        public IActionResult ConsolidateReport()
        {
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> ConsolidatePendencyReport()
        {

            var jwtToken = Request.Cookies["jwtToken"]; 
            using (var httpClientHandler = new HttpClientHandler())
            {
                // Set TLS version 
                httpClientHandler.SslProtocols = SslProtocols.Tls12 | SslProtocols.Tls13;

                // Ignore SSL certificate validation (not recommended for production)
                httpClientHandler.ServerCertificateCustomValidationCallback =
                    (sender, certificate, chain, sslPolicyErrors) => true;

                using (var httpClient = new HttpClient(httpClientHandler))
                {

                    httpClient.DefaultRequestHeaders.Add("Authorization", "Bearer " + jwtToken);
                    HttpResponseMessage response = await _httpClient.GetAsync(ApiEndPoints.ConsolidatePendencyReportEndPoint);
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
       
        [HttpGet]
        public async Task< IActionResult> ConsolidatePendency()
        {
           
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> UpdateReport(string urlType)
        {
          

            using (var httpClientHandler = new HttpClientHandler())
            {
                // Set TLS version 
                httpClientHandler.SslProtocols = SslProtocols.Tls12 | SslProtocols.Tls13;

                // Ignore SSL certificate validation (not recommended for production)
                httpClientHandler.ServerCertificateCustomValidationCallback =
                    (sender, certificate, chain, sslPolicyErrors) => true;

                using (var httpClient = new HttpClient(httpClientHandler))
                {
                    // Send the POST request without any content
                    HttpResponseMessage response = await _httpClient.PostAsync(ApiEndPoints.UpdatePendencyReportEndPoint, null);

                    if (response.IsSuccessStatusCode)
                    {
                        var responseContent = await response.Content.ReadAsStringAsync();
                        return Content(responseContent, "application/json");
                    }
                    else
                    {
                        // Handle the error as needed
                        return StatusCode((int)response.StatusCode);
                    }
                }
            }
             
        }


    

       
    }
}
