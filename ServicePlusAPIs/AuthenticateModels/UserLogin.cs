using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace ServicePlusAPIs.UserModels
{
    public class UserLogin
    {
        [Required(ErrorMessage = "User Name is required")]
        public string? Username 
        { 
            get;
            set;
        }

        [Required(ErrorMessage = "Password is required")]
        public string? Password 
        { 
            get;
            set;
        }

    }
}
