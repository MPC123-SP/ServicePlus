using System.ComponentModel.DataAnnotations;

namespace ServicePlusAPIs.HelperModels
{
    public class ApiNames
    {
        [Key]
        public int ApiNamesId 
        {
            get;
            set;
        }
        public string ApiName
        {
            get;
            set;
        }  
        public string? ApiDescription
        {
            get;
            set;
        }

    }
}
