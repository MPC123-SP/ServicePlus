using System.ComponentModel.DataAnnotations;

namespace ServicePlusAPIs.UserViewModels
{
    public class RegisterUserViewModel
    {
        [Required(ErrorMessage ="FirstName is Required")]
        public string? FirstName
        {
            get;
            set;
        } 
        public string? Lastname
        {
            get;
            set;
        }
        [Required(ErrorMessage = "Username is Required")]
        public string? Username
        {
            get;
            set;
        }
        [Required(ErrorMessage = "Email is Required")]
        public string? Email
        {
            get;
            set;
        }
        [Required(ErrorMessage = "District is Required")]
        public List<string>? District
        {
            get;
            set;
        }
        [Required(ErrorMessage = "DepartmentName is Required")]
        public List<string>? DepartmentName
        {
            get;
            set;
        }
        [Required(ErrorMessage = "ServiceName is Required")]
        public List<string>? ServiceName
        {
            get;
            set;
        }
        [Required(ErrorMessage = "Password is Required")]
        public string? Password
        {
            get;
            set;
        }
        [Required(ErrorMessage = "Role is Required")]
        public List<string>? Roles
        {
            get;
            set;
        }
    }
}
