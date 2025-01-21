namespace ServicePlusDashBoard.Helper
{
    public static class ApiEndPoints
    {
        public const string GetServicesNameEndPoint = "/api/ServicePlus/GetServicesName";
        public const string GetApiNamesEndPoint = "/api/ServicePlus/GetApiNames";
        public const string GetDistrictsEndPoint = "/api/ServicePlus/GetDistricts";
        public const string GetDepartmentsEndPoint = "/api/ServicePlus/GetDepartments";
       
    }
    public static class ApiAccountEndPoints
    {
        public const string LoginEndPoint = "/api/Authenticate/login";
        public const string GetRoleEndPoint = "/api/Authenticate/GetRole";
        public const string RegisterEndPoint = "/api/Authenticate/register";
        public const string CreateDynamicRolesEndPoint = "/api/Authenticate/CreateDynamicRoles";
        public const string GetRolePermissionsByIdEndPoint = "/api/Authenticate/GetRolePermissionsById";
        public const string GetUserEndPoint = "/api/Authenticate/GetUser";
        public const string AddEditApiDescriptionEndPoint = "/api/Authenticate/AddEditApiDescription";
        public const string UpdateApiNamesEndPoint = "/api/Authenticate/UpdateApiNames";
        public const string DisableUserAccountByUserNameEndPoint = "/api/Authenticate/DisableUserAccountByUserName";
        public const string EnableUserAccountByUserNameEndPoint = "/api/Authenticate/EnableUserAccountByUserName";
        public const string GetAccountApiNamesEndPoint = "/api/Authenticate/GetApiNames";
    }
}
