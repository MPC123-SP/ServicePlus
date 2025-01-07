using System.ComponentModel.DataAnnotations;

namespace ServicePlusAPIs.UserViewModels
{
    public class UserLoginViewModel
    {
        public string? Username
        {
            get;
            set;
        }
         
        public string? Password
        {
            get;
            set;
        }
    }
}
