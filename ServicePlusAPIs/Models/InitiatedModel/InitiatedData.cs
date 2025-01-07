using ServicePlusAPIs.Models.CommonModel.ExecutionCommonModel;
using ServicePlusAPIs.Models.EnclouserDetails;
using ServicePlusAPIs.Models.ExecutionModel;
using ServicePlusAPIs.Models.ServiceWiseModels.PSEB_Execution_OfficialFormDetails;
using ServicePlusAPIs.Models.ServiceWiseModels.PSEB_Initiated_AttributeDetails;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ServicePlusAPIs.Models.InitiatedModel
{
    public class InitiatedData
    {
        [Key]
        public int? InitiatedDataId
        {
            get;
            set;
        }

        public string? DepartmentId
        {
            get;
            set;
        }
        public string? DepartmentName
        {
            get;
            set;
        }
        public string? ServiceId
        {
            get;
            set;
        }
        public string? ServiceName
        {
            get;
            set;
        }
        public int? ApplId
        {
            get;
            set;
        }
        public string? ApplRefNo
        {
            get;
            set;
        }
        public string? NoOfAttachment
        {
            get;
            set;
        }
        public string? SubmissionMode
        {
            get;
            set;
        }
        public DateTime? SubmissionDate
        {
            get;
            set;
        }
        public string? AppliedBy
        {
            get;
            set;
        }
        public string? SubmissionLocation
        {
            get;
            set;
        }
        public string? SubmissionLocationId
        {
            get;
            set;
        }
        public string? SubmissionLocationTypeId
        {
            get;
            set;
        }
        public string? PaymentMode
        {
            get;
            set;
        }
        public string? ReferenceNo
        {
            get;
            set;
        }
        public DateTime? PaymentDate
        {
            get;
            set;
        }
        public string? Amount
        {
            get;
            set;
        }
        public string? RegistrationId
        {
            get;
            set;
        }
        public string? BaseServiceId
        {
            get;
            set;
        }
        public string? VersionNo
        {
            get;
            set;
        }
        public string? SubVersion
        {
            get;
            set;
        }
        [Column(name: "RecordInsertionTime")]
        public DateTime? InitiatedRecordInsertionTime
        {
            get;
            set;
        }
        [Column(name: "RecordInsertionFlag")]
        public int? InitiatedRecordInsertionFlag
        {
            get;
            set;
        }
        public virtual List<EnclosureDetail>? EnclosureDetails
        {
            get;
            set;
        }


        public virtual List<AttributeDetail>? AttributeDetail
        {
            get;
            set;
        }





    }
}
