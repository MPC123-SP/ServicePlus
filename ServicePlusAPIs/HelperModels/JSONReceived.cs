using System.ComponentModel.DataAnnotations;

namespace ServicePlusAPIs.HelperModels
{
    public class JSONReceived
    {
        [Key]
        public int JSONReceivedID 
        {
            get; 
            set;
        }
        public DateTime JsonReceivedDate
        {
            get;
            set;
        }
        public int? ReceivedInititatedRecord
        {
            get;
            set;
        }
        public int? ReceivedExecutionRecord
        {
            get;
            set;
        }
    }
}
