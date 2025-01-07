using Microsoft.AspNetCore.Identity;

namespace ServicePlusAPIs.AuthenticateModels
{
    public class Roles:IdentityRole
    {
        public string? Role
        {
            get;
            set;
        }
    }
}
