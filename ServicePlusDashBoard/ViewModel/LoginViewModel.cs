using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace ServicePlusDashBoard.ViewModel
{
    public class LoginViewModel
    {
        [Required]        
        public string UserName 
        { 
            get; 
            set;
        }

        [Required]
        [MinLength(8)]
        [PasswordPropertyText]
        public string Password 
        { 
            get;
            set;
        }
        public bool ShowHidePassword
        {
            get;
            set;
        }
    }
}
