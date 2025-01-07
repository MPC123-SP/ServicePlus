using ServicePlusAPIs.Models.ExecutionModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ServicePlusAPIs.HelperModels
{
    public class CustomLGDTehsilSubTehsil
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int CustomLGDTehsilSubTehsilId
        {
            get;
            set;
        }
        public int CustomLGDDistrictId
        {
            get;
            set;
        }
        [ForeignKey("CustomLGDDistrictId")]
        public virtual CustomLGDDistrict? CustomLGDDistrict
        {
            get;
            set;
        }
        public string? CustomLGDTehsilSubTehsilCode
        {
            get;
            set;
        }
        public string? CustomLGDTehsilSubTehsilName
        {
            get;
            set;
        }
    }
}
