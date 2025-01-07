using ServicePlusAPIs.HelperModels;

namespace ServicePlusAPIs.HelperViewModel
{
    public class CustomLGDDistrictViewModel
    {
        public string? CustomLGDDDistrictCode
        {
            get;
            set;

        }
        public string? CustomLGDDDistrictName
        {
            get;
            set;

        }

        public  List<CustomLGDTehsilSubTehsilViewModel>? CustomLGDTehsilSubTehsil
        {
            get;
            set;
        }
    }
}
