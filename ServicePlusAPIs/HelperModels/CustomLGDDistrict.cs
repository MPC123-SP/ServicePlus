using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ServicePlusAPIs.HelperModels
{
    public class CustomLGDDistrict
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int CustomLGDDistrictId
        {
            get;
            set;
        }
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

        public virtual List<CustomLGDTehsilSubTehsil>? CustomLGDTehsilSubTehsil
        {
            get;
            set;
        }
    }
}
