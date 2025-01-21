using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Newtonsoft.Json;
using ServicePlusDashBoard.AccountModels;
using ServicePlusDashBoard.Helper;
using ServicePlusDashBoard.ViewModel;
using System.IdentityModel.Tokens.Jwt;
using System.Net.Http;
using System.Security.Authentication;
using System.Security.Claims;
using System.Text;

namespace ServicePlusDashBoard.Controllers
{

    [Authorize]
    public class AccountController : Controller
    {
        private readonly HttpClient _httpClient;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public AccountController(IHttpClientFactory httpClientFactory, HttpClient httpClient, IHttpContextAccessor httpContextAccessor)
        {
            _httpClient = httpClientFactory.CreateClient("ServicePlusClient");

            _httpContextAccessor = httpContextAccessor;
        }
        [AllowAnonymous]
        [HttpGet]
        public async Task<IActionResult> Login()
        {
            // Fetch JSON response from the endpoint
            var response = await _httpClient.GetAsync(ApiEndPoints.GetServicesNameEndPoint);

            if (response.IsSuccessStatusCode)
            {
                // Read JSON as a string and deserialize it to a C# object
                var jsonString = await response.Content.ReadAsStringAsync();
                var serviceNames = JsonConvert.DeserializeObject<List<string>>(jsonString);
                ViewBag.ServiceNames = serviceNames;
            }
            else
            {
                // Handle error case
                ViewBag.ServiceNames = new List<string>();
            }

            return View();
        }


        [AllowAnonymous]
        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel loginViewModel)
        {
            if (ModelState.IsValid)
            {
                string Role = "";
                TokenResponse token = await GetTokenFromOtherAPI(loginViewModel.UserName, loginViewModel.Password);
                // Decode the JWT token to extract claims
                if (token != null)
                {
                    if (token.token != null)
                    {
                        var tokenHandler = new JwtSecurityTokenHandler();
                        var tokenClaim = tokenHandler.ReadJwtToken(token.token);

                        // Find the role claim
                        Claim roleClaim = tokenClaim.Claims.FirstOrDefault(claim => claim.Type == "Roles");

                        if (roleClaim != null)
                        {
                            Role = roleClaim.Value;

                        }


                        if (Role == "SuperAdmin")
                        { // Store the token in cookies
                            Response.Cookies.Append("jwtToken", token.token, new CookieOptions
                            {


                            });

                            return RedirectToAction("Index", "Admin");
                        }
                        else
                        { // Store the token in cookies
                            Response.Cookies.Append("jwtToken", token.token, new CookieOptions
                            {

                            });

                            return RedirectToAction("Index", "Home");
                        }
                    }
                    else
                    {
                        TempData["AuthenticationError"] = token.Message;

                    }
                }
                else
                {
                    TempData["AuthenticationError"] = "Invalid username or password";

                }

            }

            var response = await _httpClient.GetAsync(ApiEndPoints.GetServicesNameEndPoint);

            if (response.IsSuccessStatusCode)
            {
                // Read JSON as a string and deserialize it to a C# object
                var jsonString = await response.Content.ReadAsStringAsync();
                var serviceNames = JsonConvert.DeserializeObject<List<string>>(jsonString);
                ViewBag.ServiceNames = serviceNames;
            }
            else
            {
                // Handle error case
                ViewBag.ServiceNames = new List<string>();
            }


            // Handle authentication failure
            return View();
        }

        private async Task<TokenResponse> GetTokenFromOtherAPI(string userName, string password)
        {
            // Make an HTTP request to the other API to get the token
            var requestContent = new StringContent(JsonConvert.SerializeObject(new
            {
                Username = userName,
                Password = password
            }), Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync(ApiAccountEndPoints.LoginEndPoint, requestContent);

            if (response.IsSuccessStatusCode)
            {
                var jsonString = await response.Content.ReadAsStringAsync();
                var tokenResponse = JsonConvert.DeserializeObject<TokenResponse>(jsonString);
                return tokenResponse; // Assuming the token is returned in the response
            }
            else
            {
                var errorString = await response.Content.ReadAsStringAsync();
                throw new Exception($"Error fetching token: {errorString}");
            }
        }

        public IActionResult LogOut()
        {
            Response.Cookies.Delete("jwtToken");

            HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

            // Redirect to a page after logout (e.g., home page)
            return RedirectToAction("Login", "Account");

        }


        [HttpGet]
        public async Task<IActionResult> CreateUser()
        {
            var jwtToken = Request.Cookies["jwtToken"];
            List<RolesViewModel> dataList = await SendHttpGetRequestAsync<RolesViewModel>(ApiAccountEndPoints.GetRoleEndPoint);
            SelectList roleSelectList = new SelectList(dataList, "Name", "Name");
            SelectList checkRoleSelectList = new SelectList(dataList, "Id", "Name");
            ViewBag.RoleSelectList = roleSelectList;
            ViewBag.CheckSelectList = checkRoleSelectList;

            List<PermissionsViewModel> permissionsViewModels = await SendHttpGetRequestAsync<PermissionsViewModel>(ApiEndPoints.GetApiNamesEndPoint);
            SelectList permissionSelectList = new SelectList(permissionsViewModels, "ApiName", "ApiName");
            ViewBag.PermissionSelectList = permissionSelectList;


            List<DistrictViewModel> districtViewModel = await SendHttpGetRequestAsync<DistrictViewModel>(ApiEndPoints.GetDistrictsEndPoint);
            SelectList districtSelectList = new SelectList(districtViewModel, "CustomLGDDDistrictCode", "CustomLGDDDistrictName");
            ViewBag.DistrictSelectList = districtSelectList;


            var getDepartments = await SendHttpGetRequest<string>(ApiEndPoints.GetDepartmentsEndPoint);
            SelectList departmentsSelectList = new SelectList(getDepartments);
            ViewBag.DepartmentSelectList = departmentsSelectList;


            var getServices = await SendHttpGetRequest<string>(ApiEndPoints.GetServicesNameEndPoint);
            SelectList serviceNamesSelectList = new SelectList(getServices);

            ViewBag.ServicesSelectList = serviceNamesSelectList;


            // Retrieve the JSON string from TempData
            string rolePermissionJson = TempData["RolePermissionData"] as string;
            // Deserialize it back to List<APIDescription>
            if (rolePermissionJson != null)
            {
                List<ApiDescriptions> rolePermission = JsonConvert.DeserializeObject<List<ApiDescriptions>>(rolePermissionJson);

                ViewBag.RolePermission = rolePermission;
            }
            TempData.Remove("RolePermissionData");
            return View();
        }


        [HttpPost]
        public async Task<IActionResult> CreateUser(CreateUser createUser)
        {
            var jwtToken = Request.Cookies["jwtToken"];
            if (ModelState.IsValid)
            {
                if (jwtToken != null)
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
                            httpClient.DefaultRequestHeaders.Add("Authorization", "Bearer " + jwtToken);

                            // Convert the request object to JSON and send it in the request body
                            var jsonRequest = JsonConvert.SerializeObject(createUser);
                            var content = new StringContent(jsonRequest, Encoding.UTF8, "application/json");

                            HttpResponseMessage response = await httpClient.PostAsync(ApiAccountEndPoints.RegisterEndPoint, content);

                            if (response.IsSuccessStatusCode)
                            {
                                string responseContent = await response.Content.ReadAsStringAsync();
                                // Handle the response as needed
                                var apiResponse = JsonConvert.DeserializeObject<ApiResponse>(responseContent);

                                if (apiResponse.Status == "Success")
                                {
                                    ModelState.Clear();
                                    ViewBag.SuccessMessage = apiResponse.Message;
                                }
                                else
                                {
                                    ViewBag.ErrorMessage = apiResponse.Message;
                                }
                            }
                            else
                            {


                                // Attempt to deserialize the response content into an ErrorResponse object
                                try
                                {
                                    var errorResponse = JsonConvert.DeserializeObject<ApiResponse>(await response.Content.ReadAsStringAsync());

                                    if (errorResponse != null)
                                    {
                                        ViewBag.ErrorMessage = errorResponse.Message;
                                    }
                                }
                                catch (JsonException)
                                {
                                    // Handle any JSON deserialization errors here
                                    ViewBag.ErrorMessage = "An error occurred while processing the API response.";
                                }

                            }

                        }
                    }

                }
            }

            List<RolesViewModel> dataList = await SendHttpGetRequestAsync<RolesViewModel>(ApiAccountEndPoints.GetRoleEndPoint);
            SelectList roleSelectList = new SelectList(dataList, "Name", "Name");
            ViewBag.RoleSelectList = roleSelectList;
            SelectList checkRoleSelectList = new SelectList(dataList, "Id", "Name");
            ViewBag.CheckSelectList = checkRoleSelectList;

            List<PermissionsViewModel> permissionsViewModels = await SendHttpGetRequestAsync<PermissionsViewModel>(ApiEndPoints.GetServicesNameEndPoint);
            SelectList permissionSelectList = new SelectList(permissionsViewModels, "ApiName", "ApiName");
            ViewBag.PermissionSelectList = permissionSelectList;

            List<DistrictViewModel> districtViewModel = await SendHttpGetRequestAsync<DistrictViewModel>(ApiEndPoints.GetDistrictsEndPoint);
            SelectList districtSelectList = new SelectList(districtViewModel, "CustomLGDDDistrictName", "CustomLGDDDistrictName");
            ViewBag.DistrictSelectList = districtSelectList;

            var getDepartments = await SendHttpGetRequest<string>(ApiEndPoints.GetDepartmentsEndPoint);
            SelectList departmentsSelectList = new SelectList(getDepartments);
            ViewBag.DepartmentSelectList = departmentsSelectList;


            var getServices = await SendHttpGetRequest<string>(ApiEndPoints.GetServicesNameEndPoint);
            SelectList serviceNamesSelectList = new SelectList(getServices);

            ViewBag.ServicesSelectList = serviceNamesSelectList;
            // ModelState.Clear();
            return View();


        }

        private async Task<List<T>> SendHttpGetRequestAsync<T>(string url)
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
                    return JsonConvert.DeserializeObject<List<T>>(content);
                }
                else
                {
                    // Handle the error here if needed
                    return null;
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
        private async Task<ApiResponse> SendHttpPostRequest<T>(string url, object data)
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

                // Serialize the data object to JSON and create a StringContent
                var jsonContent = JsonConvert.SerializeObject(data);
                var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

                HttpResponseMessage response = await httpClient.PostAsync(url, content);

                if (response.IsSuccessStatusCode)
                {
                    string responseContent = await response.Content.ReadAsStringAsync();
                    // Handle the response as needed
                    var apiResponse = JsonConvert.DeserializeObject<ApiResponse>(responseContent);

                    return apiResponse;
                }
                else
                {
                    // Handle the error here if needed
                    string responseContent = await response.Content.ReadAsStringAsync();
                    // Handle the response as needed
                    var apiResponse = JsonConvert.DeserializeObject<ApiResponse>(responseContent);

                    return apiResponse;
                }
            }
        }

        public async Task<IActionResult> CreateRole()
        {
            var jwtToken = Request.Cookies["jwtToken"];

            if (jwtToken != null)
            {

                var viewModel = new CreateRole();

                // Populate Permissions and PermissionSelectList
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
                        HttpResponseMessage response = await httpClient.GetAsync(ApiEndPoints.GetApiNamesEndPoint);

                        if (response.IsSuccessStatusCode)
                        {
                            string content = await response.Content.ReadAsStringAsync();
                            List<PermissionsViewModel> permissionsViewModels = JsonConvert.DeserializeObject<List<PermissionsViewModel>>(content);
                            viewModel.PermissionSelectList = new SelectList(permissionsViewModels, "ApiName", "ApiName");
                        }
                    }
                }
                List<ApiDescriptions> apiNames = await SendHttpGetRequestAsync<ApiDescriptions>(ApiAccountEndPoints.GetAccountApiNamesEndPoint);
                ViewBag.apiDescription = apiNames;
                return View(viewModel);
            }
            else
            {
                return View();
            }

        }



        [HttpPost]
        public async Task<IActionResult> CreateRole(CreateRole createRole)
        {
            var jwtToken = Request.Cookies["jwtToken"];
            if (jwtToken != null)
            {

                // Populate Permissions and PermissionSelectList
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

                        // Convert the request object to JSON and send it in the request body
                        var jsonRequest = JsonConvert.SerializeObject(createRole);
                        var content = new StringContent(jsonRequest, Encoding.UTF8, "application/json");

                        HttpResponseMessage response = await httpClient.PostAsync(ApiAccountEndPoints.CreateDynamicRolesEndPoint, content);

                        if (response.IsSuccessStatusCode)
                        {
                            string responseContent = await response.Content.ReadAsStringAsync();
                            // Handle the response as needed
                            return RedirectToAction("CreateUser", "Account");

                        }
                        else
                        {
                            return View();

                        }
                    }
                }

            }
            else
            {
                return View();
            }
        }


        [HttpPost]
        public async Task<IActionResult> GetRolePermission(CreateUser createUser)
        {

            var rolePermission = await SendHttpGetRequestAsync<ApiDescriptions>($"{ApiAccountEndPoints.GetRolePermissionsByIdEndPoint}?roleId={createUser.Roles.FirstOrDefault()}");

            // Serialize the rolePermission object to a JSON string
            string rolePermissionJson = JsonConvert.SerializeObject(rolePermission);

            // Store the JSON string in TempData
            TempData["RolePermissionData"] = rolePermissionJson;


            // Redirect to the CreateUser action
            return RedirectToAction("CreateUser");

        }

        [HttpGet]
        public async Task<IActionResult> GetUser()
        {
            string rolePermissionJson = TempData["Response"] as string;
            ViewBag.response = rolePermissionJson;
            TempData.Remove("Response");

            return View();
        }
        [HttpPost]
        public async Task<IActionResult> GetUsers(int page, int pageSize)
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

                    HttpResponseMessage response = await httpClient.GetAsync($"{ApiAccountEndPoints.GetUserEndPoint}?page={page}&pageSize={pageSize}");
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
        public IActionResult ResetPassword()
        {

            return View();
        }

        [HttpGet]
        public async Task<IActionResult> AddApiDescription()
        { 
            List<ApiDescriptions> apiNames = await SendHttpGetRequestAsync<ApiDescriptions>(ApiAccountEndPoints.GetAccountApiNamesEndPoint);

            // Filter the list to include only items with empty descriptions
            var apiNamesWithEmptyDescriptions = apiNames.Where(api => string.IsNullOrEmpty(api.ApiDescription)).ToList();

            SelectList apiSelectList = new SelectList(apiNamesWithEmptyDescriptions, "ApiName", "ApiName");

            ViewBag.apiSelectList = apiSelectList;

            string rolePermissionJson = TempData["Response"] as string;
            ViewBag.response = rolePermissionJson;
            TempData.Remove("Response");

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> AddApiDescription(ApiDescriptions apiDescription)
        {
            if (ModelState.IsValid)
            {
                 
                ApiResponse response = await SendHttpPostRequest<string>(ApiAccountEndPoints.AddEditApiDescriptionEndPoint, apiDescription);

                TempData["Response"] = response.Message;
            }
            return RedirectToAction("AddApiDescription");
        }

        [HttpGet]
        public async Task<IActionResult> UpdateApiName()
        { 
            ApiResponse response = await SendHttpPostRequest<string>(ApiAccountEndPoints.UpdateApiNamesEndPoint, "");

            TempData["Response"] = response.Message;
            return RedirectToAction("AddApiDescription");
        }

        [HttpGet]
        public async Task<IActionResult> DisableUserAccountByUserName(string userName)
        {
            if (!string.IsNullOrWhiteSpace(userName))
            {
                
                ApiResponse response = await SendHttpPostRequest<string>($"{ApiAccountEndPoints.DisableUserAccountByUserNameEndPoint}?username={userName}", userName);

                TempData["Response"] = response.Message;
            }
            return RedirectToAction("GetUser");

        }
        [HttpGet]
        public async Task<IActionResult> EnableUserAccountByUserName(string userName)
        {
            if (!string.IsNullOrWhiteSpace(userName))
            {
                 
                ApiResponse response = await SendHttpPostRequest<string>($"{ApiAccountEndPoints.EnableUserAccountByUserNameEndPoint}?username={userName}", userName);

                TempData["Response"] = response.Message;
            }
            return RedirectToAction("GetUser");

        }
    }

}
