using ServicePlusAPIs.Models.ExecutionModel;
using ServicePlusAPIs.Models.InitiatedModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ServicePlusAPIs.Models.CommonModel.ExecutionCommonModel
{

    public class TaskDetail
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int TaskDetailID
        {
            get;
            set;
        }
        public int ExecutionDataId
        {
            get;
            set;
        }
        [ForeignKey("ExecutionDataId")]
        public virtual ExecutionData ExecutionData
        {
            get;
            set;
        }
        public string? Amount
        {
            get;
            set;
        }
        public int? ApplId
        {
            get;
            set;
        }
        public string? Remarks
        {
            get;
            set;
        }
        public int? TaskId
        {
            get;
            set;
        }
        public int? ActionNo
        {
            get;
            set;
        }
        public string? TaskName
        {
            get;
            set;
        }
        public int? TaskType
        {
            get;
            set;
        }
        public string? UserName
        {
            get;
            set;
        }
        public int? ServiceId
        {
            get;
            set;
        }

        public UserDetail? UserDetail
        {
            get;
            set;
        }
        public string? ActionTaken
        {
            get;
            set;
        }
        public DateTime? PaymentDate
        {
            get;
            set;
        }
        public string? PaymentMode
        {
            get;
            set;
        }
        public int? PullUserId
        {
            get;
            set;
        }
        public DateTime? ExecutedTime
        {
            get;
            set;
        }
        public DateTime? ReceivedTime
        {
            get;
            set;
        }
        public string? PaymentRefNo
        {
            get;
            set;
        }
        public int? CurrentProcessId
        {
            get;
            set;
        }
        public string? CallbackCurrProcId
        {
            get;
            set;
        }

    }

    public class UserDetail
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int UserDetailID
        {
            get;
            set;
        }
        public int TaskDetailID
        {
            get;
            set;
        }
        [ForeignKey("TaskDetailID")]
        public virtual TaskDetail TaskDetail
        {
            get;
            set;
        }

        public string? UserName
        {
            get;
            set;
        }
        public string? Designation
        {
            get;
            set;
        }
        public string? LocationId
        {
            get;
            set;
        }
        public int? PullUserId
        {
            get;
            set;
        }
        public string? LocationName
        {
            get;
            set;
        }
        public string? DepartmentLevel
        {
            get;
            set;
        }
        public string? LocationTypeId
        {
            get;
            set;
        }
        public int? CurrentProcessId
        {
            get;
            set;
        }
    }

}
