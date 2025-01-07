using ServicePlusAPIs.Models.ExecutionModel;
using ServicePlusAPIs.Models.InitiatedModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ServicePlusAPIs.Models.ServiceWiseModels.PSEB_Execution_OfficialFormDetails
{
    public class ApplicantTaskDetail
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ApplicantTaskDetailID
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
        public string? Value 
        {
            get; 
            set;
        }  
    }
}
