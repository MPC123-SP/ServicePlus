using AutoMapper;
using DocumentFormat.OpenXml.Drawing.Spreadsheet;
using DocumentFormat.OpenXml.EMMA;
using DocumentFormat.OpenXml.Math;
using DocumentFormat.OpenXml.Office2010.PowerPoint;
using DocumentFormat.OpenXml.Spreadsheet;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using ServicePlusAPIs.AuthenticateModels;
using ServicePlusAPIs.AuthenticateViewModels;
using ServicePlusAPIs.Context;
using ServicePlusAPIs.HelperModels;
using ServicePlusAPIs.UserModels;
using ServicePlusAPIs.UserViewModels;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Reflection;
using System.Security.Claims;
using System.Text;

namespace ServicePlusAPIs.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthenticateController : ControllerBase
    {
        private readonly UserManager<RegisterUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IConfiguration _configuration;
        private readonly IMapper _mapper;
        private readonly ServicePlusContext _servicePlusContext;

        private readonly ILogger<AuthenticateController> _logger;

        public AuthenticateController(
            UserManager<RegisterUser> userManager,
            RoleManager<IdentityRole> roleManager,
            IConfiguration configuration, IMapper mapper
            , ServicePlusContext servicePlusContext, ILogger<AuthenticateController> logger)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _configuration = configuration;
            _mapper = mapper;
            _servicePlusContext = servicePlusContext;
            _logger = logger;
        }

        #region Login & Generate Token
        [HttpPost]
        [Route("login")]
        public async Task<IActionResult> Login([FromBody] UserLoginViewModel model)
        {
            var userLogin = _mapper.Map<UserLoginViewModel, UserLogin>(model);
            var user = await _userManager.FindByNameAsync(userLogin.Username);
            var userRecord= _servicePlusContext.Users
                         .Include(u => u.RegisterUserDistricts)
                         .Include(u => u.RegisterUserServices)
                         .Include(u => u.RegisterUserDepartments).Where(d => d.UserName == user.UserName).ToList();


            if (user != null && await _userManager.CheckPasswordAsync(user, userLogin.Password))
            {
                var isLockoutEnabled = await _userManager.GetLockoutEnabledAsync(user);

                if (isLockoutEnabled==false)
                {
                    // Check if the user is currently locked out
                    var isLockedOut = await _userManager.IsLockedOutAsync(user);

                    if (isLockedOut==false)
                    {
                        // Return a response indicating that the account is locked
                        return Unauthorized(new Response { Status = "Error", Message = "User account is  locked." });
                    }
                }
                var userRoles = await _userManager.GetRolesAsync(user);
                var registerUserServices = await _servicePlusContext.RegisterUserServices.Where(d => d.UserId == user.Id).ToListAsync();
                var rolePermissions = new List<RolePermission>();
                
                foreach (var role in userRoles)
                {
                    var rolesId = _roleManager.Roles.Where(d => d.Name == role).Select(d => d.Id).FirstOrDefault();
                    var rolePermissionsForUser = await GetRoleWisePemission(rolesId);
                    rolePermissions.AddRange(rolePermissionsForUser);
                }

                var authClaims = new List<Claim>
                                {
                                    new Claim(ClaimTypes.Name, user.UserName),
                                    new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                                };

                foreach (var userRole in userRoles)
                {
                    authClaims.Add(new Claim("Roles", userRole));
                }

                foreach (var permission in rolePermissions)
                {
                    authClaims.Add(new Claim("Permission", permission.Permission));
                }

                foreach (var userServices in registerUserServices)
                {
                    authClaims.Add(new Claim("UserServices", userServices.ServiceName));
                }


                var token = GetToken(authClaims);

                return Ok(new
                {
                    token = new JwtSecurityTokenHandler().WriteToken(token),
                    expiration = token.ValidTo
                });
            }
            return Unauthorized();
        }

        private JwtSecurityToken GetToken(List<Claim> authClaims)
        {
            var authSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JWT:Secret"]));

            var token = new JwtSecurityToken(
                issuer: _configuration["JWT:ValidIssuer"],
                audience: _configuration["JWT:ValidAudience"],
                expires: DateTime.Now.AddHours(3),
                claims: authClaims,
                signingCredentials: new SigningCredentials(authSigningKey, SecurityAlgorithms.HmacSha256)
                );

            return token;
        }

        #endregion

        #region Register
        [CustomAuthorizeAttribute]
        [HttpPost]
        [Route("register")]
        public async Task<IActionResult> Register(RegisterUserViewModel registerUserViewModel)
        {
            // Check if roles are specified
            if (registerUserViewModel.Roles == null || registerUserViewModel.Roles.Count == 0)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new Response { Status = "Error", Message = "No roles specified" });
            }

            foreach (var roleName in registerUserViewModel.Roles)
            {
                // Check if the role exists
                var roleExists = await _roleManager.RoleExistsAsync(roleName);

                if (!roleExists)
                {
                    return StatusCode(StatusCodes.Status500InternalServerError, new Response { Status = "Error", Message = $"Role '{roleName}' does not exist" });
                }
            }

            // Check if the user already exists
            var userExists = await _userManager.FindByNameAsync(registerUserViewModel.Username);

            if (userExists != null)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new Response { Status = "Error", Message = "User already exists!" });
            }

            // Create a new user
            var user = new RegisterUser
            {
                FirstName = registerUserViewModel.FirstName,
                LastName = registerUserViewModel.Lastname,
                Email = registerUserViewModel.Email,
                SecurityStamp = Guid.NewGuid().ToString(),
                UserName = registerUserViewModel.Username,
            };


            // Add districts to the user
            if (registerUserViewModel.District != null)
            {
                foreach (var districtName in registerUserViewModel.District)
                {
                    user.RegisterUserDistricts.Add(new RegisterUserDistrict { DistrictName = districtName });
                }
            }

            // Add department names to the user
            if (registerUserViewModel.DepartmentName != null)
            {
                foreach (var departmentName in registerUserViewModel.DepartmentName)
                {
                    user.RegisterUserDepartments.Add(new RegisterUserDepartment { DepartmentName = departmentName });
                }
            }

            // Add service names to the user
            if (registerUserViewModel.ServiceName != null)
            {
                foreach (var serviceName in registerUserViewModel.ServiceName)
                {
                    user.RegisterUserServices.Add(new RegisterUserService { ServiceName = serviceName });
                }
            }
            var result = await _userManager.CreateAsync(user, registerUserViewModel.Password);

            //// Add roles to the user
            var rolesToAdd = new List<string>(registerUserViewModel.Roles);
            await _userManager.AddToRolesAsync(user, rolesToAdd);

            if (!result.Succeeded)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new Response { Status = "Error", Message = "User creation failed! Please check user details and try again." });
            }

            return StatusCode(StatusCodes.Status201Created, new Response { Status = "Success", Message = "User created successfully" });
        }
        #endregion

        #region Permanent Delete User By Name
        [CustomAuthorizeAttribute]
        [HttpPost]
        [Route("PermanentDeleteUserByUserName")]
        public async Task<IActionResult> DeleteUserByUserName(string username)
        {
            var user = await _servicePlusContext.Users
                            .Include(u => u.RegisterUserDistricts)
                            .Include(u => u.RegisterUserServices)
                            .Include(u => u.RegisterUserDepartments)
                            .FirstOrDefaultAsync(u => u.UserName == username);

            if (user == null)
            {
                return StatusCode(StatusCodes.Status404NotFound, new Response { Status = "Error", Message = "User not found" });
            }

            // Delete related data in foreign key tables (assuming they are properly configured with Cascade Delete)
            // You can use the DbContext to remove related data
            _servicePlusContext.RegisterUserDistricts.RemoveRange(user.RegisterUserDistricts);
            _servicePlusContext.RegisterUserDepartments.RemoveRange(user.RegisterUserDepartments);
            _servicePlusContext.RegisterUserServices.RemoveRange(user.RegisterUserServices);

            // Delete the user
            var result = await _userManager.DeleteAsync(user);

            if (!result.Succeeded)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new Response { Status = "Error", Message = "User deletion failed" });
            }

            return StatusCode(StatusCodes.Status200OK, new Response { Status = "Success", Message = "User deleted successfully" });
        }

        #endregion

        #region Disable UserAccount User By Name
        [CustomAuthorizeAttribute]
        [HttpPost]
        [Route("DisableUserAccountByUserName")]
        public async Task<IActionResult> DisableUserAccountByUserName(string username)
        {
            if (string.IsNullOrWhiteSpace(username))
            {
                return StatusCode(StatusCodes.Status400BadRequest, new Response { Status = "Error", Message = "Invalid username" });
            }

            var user = await _userManager.FindByNameAsync(username);

            if (user == null)
            {
                return StatusCode(StatusCodes.Status404NotFound, new Response { Status = "Error", Message = "User not found" });
            }

            // Disable lockout for the user
            var result = await _userManager.SetLockoutEnabledAsync(user, false);

            if (result.Succeeded)
            {
                return StatusCode(StatusCodes.Status200OK, new Response { Status = "Success", Message = "User account disabled successfully" });
            }
            else
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new Response { Status = "Error", Message = "User account disabling failed" });
            }
        }
        #endregion

        #region Enable UserAccount User By Name
        [CustomAuthorizeAttribute]
        [HttpPost]
        [Route("EnableUserAccountByUserName")]
        public async Task<IActionResult> EnableUserAccountByUserName(string username)
        {
            if (string.IsNullOrWhiteSpace(username))
            {
                return StatusCode(StatusCodes.Status400BadRequest, new Response { Status = "Error", Message = "Invalid username" });
            }

            var user = await _userManager.FindByNameAsync(username);

            if (user == null)
            {
                return StatusCode(StatusCodes.Status404NotFound, new Response { Status = "Error", Message = "User not found" });
            }

            // Disable lockout for the user
            var result = await _userManager.SetLockoutEnabledAsync(user, true);

            if (result.Succeeded)
            {
                return StatusCode(StatusCodes.Status200OK, new Response { Status = "Success", Message = "User account Enable successfully" });
            }
            else
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new Response { Status = "Error", Message = "User account Enable failed" });
            }
        }

        #endregion

        #region ForgetPassword
        [HttpPost]
        [Route("ResetPassword")]
        public async Task<IActionResult> ResetPassword(ResetPasswordViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = await _userManager.FindByEmailAsync(model.Email);

                if (user == null)
                {
                    return BadRequest("User not found.");
                }

                var result = await _userManager.ResetPasswordAsync(user, model.Token, model.Password);

                if (result.Succeeded)
                {
                    return Ok("Password reset successful.");
                }

                return BadRequest("Password reset failed.");
            }

            return BadRequest(ModelState);
        }
        #endregion

        #region Get User
        [CustomAuthorizeAttribute]
        [HttpGet]
        [Route("GetUser")]
        public async Task<IActionResult> GetUser(int page, int pageSize)
        {
            var totalCount = await _servicePlusContext.Users.CountAsync();

            var userList =await _servicePlusContext.Users
                       .Include(u => u.RegisterUserDistricts)
                       .Include(u => u.RegisterUserServices)
                       .Include(u => u.RegisterUserDepartments).OrderByDescending(d => d.UserName)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();


            var result = new List<object>();

            foreach (var user in userList)
            {
                var roles = await _userManager.GetRolesAsync(user);
                var rolesId = _roleManager.Roles.Where(d => d.Name == roles.First()).Select(d => d.Id).FirstOrDefault();

                if (roles != null)
                {
                    var rolePermissions = _servicePlusContext.RolePermissions
                        .Where(d => d.RoleId == rolesId.ToString()).Select(d => d.Permission)
                        .ToList();

                    result.Add(new
                    {
                        FirstName = user.FirstName,
                        MobileNumber = user.PhoneNumber,
                        Email = user.Email,
                        District = user.RegisterUserDistricts.Select(d => d.DistrictName),
                        Department = user.RegisterUserDepartments.Select(d => d.DepartmentName),
                        ServiceName = user.RegisterUserServices.Select(d => d.ServiceName),
                        UserName = user.UserName,
                        RoleName = roles, // Role name 
                        RolePermissions = rolePermissions,
                        LockoutEnabled=user.LockoutEnabled
                    });
                }
            }
            var response = new
            {
                recordsTotal = totalCount,
                recordsFiltered = totalCount,
                data = result
            };
            return Ok(response);
        }



        #endregion

        #region Create Dynamic Roles 
        [CustomAuthorizeAttribute]
        [HttpPost]
        [Route("CreateDynamicRoles")]
        public async Task<IActionResult> CreateDynamicRole(RolesViewModel roleViewModel)
        {
            var existingRole = await _roleManager.FindByNameAsync(roleViewModel.Role);
            if (existingRole != null)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new Response { Status = "Error", Message = "Role already exists!" });
            }

            // Create a new role
            var newRole = new IdentityRole(roleViewModel.Role);

            // Add the role to the database
            var result = await _roleManager.CreateAsync(newRole);

            if (!result.Succeeded)
            {
                // Handle the error if role creation fails
                return StatusCode(StatusCodes.Status500InternalServerError, new Response { Status = "Error", Message = "Role creation failed! Please check role details and try again." });
            }

            // Add report permissions to the custom table
            if (roleViewModel.ReportPermissions != null)
            {
                foreach (var permission in roleViewModel.ReportPermissions)
                {
                    _servicePlusContext.RolePermissions.Add(new RolePermission
                    {
                        RoleId = newRole.Id,
                        Permission = permission
                    });
                }
            }

            await _servicePlusContext.SaveChangesAsync();

            return Ok(new Response { Status = "Success", Message = "Role created successfully!" });
        }
        #endregion

        #region Get Role 
        [CustomAuthorizeAttribute]
        [HttpGet]
        [Route("GetRole")]
        public async Task<IActionResult> GetRole()
        {
            var roles = _roleManager.Roles.ToList();
            return Ok(roles);
        }
        #endregion

        #region Delete Role
        [CustomAuthorizeAttribute]
        [HttpDelete]
        [Route("DeleteRoleById")]
        public async Task<IActionResult> DeleteRoleById(string id)
        {
            try
            {
                // Find the role by Id
                var role = await _roleManager.FindByIdAsync(id);

                if (role == null)
                {
                    // Role not found, return an error response
                    return NotFound(new Response { Status = "Error", Message = "Role not found" });
                }

                // Check if the role has any users assigned to it
                var usersInRole = await _userManager.GetUsersInRoleAsync(role.Name);

                if (usersInRole.Any())
                {
                    // You may choose to handle this scenario differently, e.g., by reassigning users or preventing deletion
                    return BadRequest(new Response { Status = "Error", Message = "Cannot delete role with assigned users" });
                }

                // Delete the role
                var result = await _roleManager.DeleteAsync(role);

                if (result.Succeeded)
                {
                    return Ok(new Response { Status = "Success", Message = "Role deleted successfully" });
                }
                else
                {
                    // Handle role deletion errors
                    var errors = result.Errors.Select(e => e.Description);
                    return BadRequest(new Response { Status = "Error", Message = "Role deletion failed" });
                }
            }
            catch (Exception ex)
            {
                // Log any unexpected exceptions
                _logger.LogError(ex, "An error occurred while deleting a role.");
                return StatusCode(StatusCodes.Status500InternalServerError, new Response { Status = "Error", Message = "An error occurred" });
            }
        }

        #endregion

        #region EditRole 
        [CustomAuthorizeAttribute]
        [HttpPut]
        [Route("EditRole")]
        public async Task<IActionResult> EditRole(string id, RolesViewModel roleViewModel)
        {
            try
            {
                // Find the role by Id
                var role = await _roleManager.FindByIdAsync(id);

                if (role == null)
                {
                    // Role not found, return an error response
                    return NotFound(new Response { Status = "Error", Message = "Role not found" });
                }

                // Modify the role properties
                role.Name = roleViewModel.Role; // Update the role name or any other properties as needed

                // Update the role using RoleManager
                var result = await _roleManager.UpdateAsync(role);

                if (result.Succeeded)
                {
                    return Ok(new Response { Status = "Success", Message = "Role updated successfully" });
                }
                else
                {
                    // Handle role update errors
                    var errors = result.Errors.Select(e => e.Description);
                    return BadRequest(new Response { Status = "Error", Message = "Role update failed" });
                }
            }
            catch (Exception ex)
            {
                // Log any unexpected exceptions
                _logger.LogError(ex, "An error occurred while editing a role.");
                return StatusCode(StatusCodes.Status500InternalServerError, new Response { Status = "Error", Message = "An error occurred" });
            }
        }


        #endregion

        #region Get Role Permissions By Id
        [CustomAuthorizeAttribute]
        [Route("GetRolePermissionsById")]
        [HttpGet]
        public async Task<IActionResult> GetRolePermissionsById(string roleId)
        {
            if (roleId != null)
            {
                var apiNamesWithDescription = _servicePlusContext.RolePermissions
                    .Where(d => d.RoleId == roleId)
                    .Select(d => new
                    {
                        apiName = d.Permission,
                        apiDescription = _servicePlusContext.ApiNames
                            .Where(api => api.ApiName == d.Permission)
                            .Select(api => api.ApiDescription)
                            .FirstOrDefault()
                    })
                    .ToList();

                return Ok(apiNamesWithDescription);
            }
            else
            {
                return StatusCode(StatusCodes.Status400BadRequest, new Response { Status = "Error", Message = "RoleId not found" });
            }
        }

        private async Task<List<RolePermission>> GetRoleWisePemission(string roleId)
        {
            if (roleId != null)
            {
                var apiNamesWithDescription = _servicePlusContext.RolePermissions
                    .Where(d => d.RoleId == roleId).ToList();

                return apiNamesWithDescription;
            }
            else
            {
                // You may want to handle this case differently, such as returning an empty list.
                return new List<RolePermission>();
            }
        }

        #endregion

        #region Update API Names & Get API Names && Add API Description
        [CustomAuthorizeAttribute]
        [Route("UpdateApiNames")]
        [HttpPost]
        public async Task<IActionResult> UpdateApiNames()
        {
            // Step 1: Delete all existing records related to API names

            var assembly = Assembly.GetExecutingAssembly();
            var controllerTypes = assembly.GetTypes().Where(t => typeof(ControllerBase).IsAssignableFrom(t));

            List<ApiNames> apiNames = new List<ApiNames>();

            foreach (var controllerType in controllerTypes)
            {
                var methods = controllerType.GetMethods(BindingFlags.Public | BindingFlags.Instance);

                foreach (var method in methods)
                {
                    // Check if the method has an actual implementation by examining its IL code
                    if (method.DeclaringType == controllerType)
                    {
                        var controllerName = controllerType.Name;
                        if (controllerName == "ServicePlusController")
                        {
                            ApiNames apiName = new ApiNames()
                            {
                                ApiName = method.Name
                            };

                            // Check if the apiName already exists in the database
                            var existingApiName = _servicePlusContext.ApiNames.FirstOrDefault(an => an.ApiName == apiName.ApiName);
                            if (existingApiName == null)
                            {
                                apiNames.Add(apiName);
                            }
                        }
                    }
                }
            }
            if (apiNames.Count > 0)
            {
                return StatusCode(StatusCodes.Status200OK, new Response { Status = "Message", Message = "No New API Found" });

            }
            else
            {
                // Step 3: Add only the new apiNames to the database
                _servicePlusContext.ApiNames.AddRange(apiNames);
                _servicePlusContext.SaveChanges();
            }
            return Ok();
        }



        [CustomAuthorizeAttribute]
        [Route("GetApiNames")]
        [HttpGet]
        public async Task<IActionResult> GetApiNames()
        {
            var getApiNames = await _servicePlusContext.ApiNames.ToListAsync();
            return Ok(getApiNames);
        }

        [CustomAuthorizeAttribute]
        [Route("AddEditApiDescription")]
        [HttpPost]
        public async Task<IActionResult> AddApiDescription(ApiNameViewModel apiViewModel)
        {
            try
            {
                // Check if the model state is valid
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                // Map the view model to the entity model
                var mappedData = _mapper.Map<ApiNameViewModel, ApiNames>(apiViewModel);

                // Check if an entry with the same ApiName exists in the database
                var existingApiName = _servicePlusContext.ApiNames.FirstOrDefault(d => d.ApiName == mappedData.ApiName);

                if (existingApiName != null)
                {
                    // Update the existing entry with the new data
                    existingApiName.ApiDescription = apiViewModel.ApiDescription; // Assuming ApiDescription is a property in ApiNames
                    _servicePlusContext.ApiNames.Update(existingApiName);
                    await _servicePlusContext.SaveChangesAsync();
                    return StatusCode(StatusCodes.Status200OK, new Response { Status = "Updated", Message = "API Description Updated" });

                }
                else
                {
                    // Entry with the given ApiName doesn't exist, return an error response
                    return StatusCode(StatusCodes.Status500InternalServerError, new Response { Status = "Error", Message = "ApiName Not Found" });
                }
            }
            catch (Exception ex)
            {
                // Handle any exceptions that occur during the update process
                return StatusCode(StatusCodes.Status500InternalServerError, new Response { Status = "Error", Message = ex.Message });
            }
        }

        #endregion
       
      


    }


}
