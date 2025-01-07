using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace ServicePlusDashBoard.ViewModel
{
    public class CreateUser
    {
        [Required(ErrorMessage = "FirstName is Required")]
        public string FirstName
        {
            get;
            set;
        } 
        public string? Lastname
        {
            get;
            set;
        }
        [Required(ErrorMessage = "Username is required")]
        public string UserName 
        {
            get;
            set;
        }
        [Required(ErrorMessage = "Email is required")]
        [EmailAddress]
        public string Email 
        { 
            get;
            set;
        }
        [Required(ErrorMessage = "District is Required")]
        public List<string> District
        {
            get;
            set;
        }
        [Required(ErrorMessage = "DepartmentName is Required")]
        public List<string> DepartmentName
        {
            get;
            set;
        }
        [Required(ErrorMessage = "ServiceName is Required")]
        public List<string> ServiceName
        {
            get;
            set;
        }
        [Required(ErrorMessage = "Password is required")]
        [PasswordPropertyText]
        public string Password
        {
            get;
            set;
        }
        [Required(ErrorMessage = "Roles is required")]
        public List<string> Roles
        { 
            get;
            set;
        }

        
    }
}
