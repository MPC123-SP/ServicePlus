namespace ServicePlusDashBoard.ViewModel
{
    public class User
    {
        public string? FirstName 
        { 
            get;
            set;
        }
        public string? LastName
        {
            get;
            set;
        }
        public string? MobileNumber 
        { 
            get;
            set;
        }
        public string? Email 
        { 
            get;
            set;
        }
        public List<string>? District 
        { 
            get; 
            set;
        }
        public List<string>? Department 
        {
            get;
            set;
        }
        public List<string>? ServiceName 
        {
            get;
            set;
        }
        public string? UserName 
        {
            get;
            set;
        }
        public List<string>? RoleName 
        {
            get;
            set;
        }
        public List<string>? RolePermissions 
        {
            get;
            set;
        }
    }

}
