using Microsoft.AspNetCore.Identity;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace ServicePlusAPIs.UserModels
{
    public class RegisterUser : IdentityUser
    {
        [Required(ErrorMessage = "FirstName is required")]
        public string? FirstName { get; set; }

        [Required(ErrorMessage = "LastName is required")]
        public string? LastName { get; set; }

        // Navigation properties for one-to-many relationships
        public List<RegisterUserDepartment> RegisterUserDepartments { get; set; } = new List<RegisterUserDepartment>();
        public List<RegisterUserDistrict> RegisterUserDistricts { get; set; } = new List<RegisterUserDistrict>();
        public List<RegisterUserService> RegisterUserServices { get; set; } = new List<RegisterUserService>();
    }

    public class RegisterUserDistrict
    {

        public int DistrictId { get; set; }
        public string DistrictName { get; set; }

        // Foreign key to RegisterUser
        public string UserId { get; set; }

        // Navigation property to the parent user
        public RegisterUser User { get; set; }
    }

    public class RegisterUserDepartment
    {
        public int DepartmentId { get; set; }
        public string DepartmentName { get; set; }

        // Foreign key to RegisterUser
        public string UserId { get; set; }

        // Navigation property to the parent user
        public RegisterUser User { get; set; }
    }

    public class RegisterUserService
    {
        public int ServiceId { get; set; }
        public string ServiceName { get; set; }

        // Foreign key to RegisterUser
        public string UserId { get; set; }

        // Navigation property to the parent user
        public RegisterUser User { get; set; }
    }
}
