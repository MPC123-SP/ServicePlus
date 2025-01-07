using ServicePlusAPIs.Models.CommonModel.ExecutionCommonModel;
using ServicePlusAPIs.Models.InitiatedModel;
using ServicePlusAPIs.Models.ServiceWiseModels.PSEB_Execution_OfficialFormDetails;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ServicePlusAPIs.Models.ExecutionModel
{
    public class ExecutionData
    {

        [Key]
        public int? ExecutionDataId
        {
            get;
            set;
        }

        public virtual TaskDetail? TaskDetails
        {
            get;
            set;
        }
        public virtual List<OfficialFormDetail>? OfficialFormDetail
        {
            get;
            set;
        }
        public string? ApplicantTaskDetails
        {
            get;
            set;
        }
        [Column(name: "RecordInsertionTime")]

        public DateTime? ExecutionDataRecordInsertionTime
        {
            get;
            set;
        }
        [Column(name: "RecordInsertionFlag")]
        public int? ExecutionDataRecordInsertionFlag
        {
            get;
            set;
        }


    }

}
