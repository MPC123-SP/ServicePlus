using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using ServicePlusDashBoard.AccountModels;
using ServicePlusDashBoard.Helper;
using ServicePlusDashBoard.Models;
using System.Security.Authentication;
using System.Text;

namespace ServicePlusDashBoard.Controllers
{ 
    [CustomAuthorizeAttribute(role: "SuperAdmin")]
    public class AdminController : Controller
    {
        private readonly HttpClient _httpClient;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public AdminController(IHttpClientFactory httpClientFactory, HttpClient httpClient, IHttpContextAccessor httpContextAccessor)
        {
            _httpClient = httpClientFactory.CreateClient("ServicePlusClient");
            _httpContextAccessor = httpContextAccessor;
        }
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
            string url = $"http://10.147.24.36:8083/api/ServicePlus/JSONReceivedDates?page={page}&pageSize={pageSize}";
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
            string url = $"http://10.147.24.36:8083/api/ServicePlus/ConsolidateReport?page={page}&pageSize={pageSize}";
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

        [HttpGet]
        public IActionResult ConsolidateReport()
        {
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> ConsolidatePendencyReport()
        {

            var jwtToken = Request.Cookies["jwtToken"];
            string url = "http://10.147.24.36:8083/api/ServicePlus/ConsolidatePendencyReport"; // Replace with your desired URL
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
       
        [HttpGet]
        public async Task< IActionResult> ConsolidatePendency()
        {
           
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> UpdateReport(string urlType)
        {
            string url = "http://10.147.24.36:8082/api/ServicePlus/UpdatePendencyReport";

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
                    HttpResponseMessage response = await httpClient.PostAsync(url, null);

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
