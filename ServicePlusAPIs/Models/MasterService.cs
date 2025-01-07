using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ServicePlusAPIs.Models
{
    public class MasterService
    {
        [Key]
        public int MasterServiceId
        {
            get;
            set;
        }

        public int? MasterServiceInitiatedId
        {
            get;
            set;
        }

        public int? MasterServiceExecutionId
        {
            get;
            set;
        }
        public string? AppId
        {
            get;
            set;
        }
        public string AppRefNo
        {
            get;
            set;
        }
        [Column(name: "InitiatedRecordInsertionTime")]
        public DateTime MasterServiceInitiatedRecordInsertionTime
        {
            get;
            set;
        }
        [Column(name: "InitiatedRecordInsertionFlag")]
        public int MasterServiceInitiatedRecordInsertionFlag
        {
            get;
            set;
        }
        [Column(name: "ExecutionRecordInsertionTime")]
        public DateTime MasterServiceExecutionRecordInsertionTime
        {
            get;
            set;
        }
        [Column(name: "ExecutionRecordInsertionFlag")]
        public int MasterServiceExecutionRecordInsertionFlag
        {
            get;
            set;
        }
    }
}