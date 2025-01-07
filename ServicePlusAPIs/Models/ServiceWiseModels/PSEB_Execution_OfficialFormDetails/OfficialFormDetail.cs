using ServicePlusAPIs.Models.ExecutionModel;
using ServicePlusAPIs.Models.InitiatedModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ServicePlusAPIs.Models.ServiceWiseModels.PSEB_Execution_OfficialFormDetails
{
    public class OfficialFormDetail
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int? OfficialFormDetailID 
        {
            get; 
            set;
        }
        public int? ExecutionDataId
        {
            get;
            set;
        }
        [ForeignKey("ExecutionDataId")]
        public virtual ExecutionData? ExecutionData
        {
            get;
            set;
        }
        public string? OfficalFormID 
        { 
            get;
            set;
        }
        public string? OfficalFormValue
        {
            get; 
            set;
        }

    }
}
