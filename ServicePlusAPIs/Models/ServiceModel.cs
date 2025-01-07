using ServicePlusAPIs.Models.ExecutionModel;
using ServicePlusAPIs.Models.InitiatedModel;

namespace ServicePlusAPIs.Models
{
    public class ServiceModel
    {
        public List<InitiatedData>? InitiatedData
        {
            get;
            set;
        }
        public List<ExecutionData>? ExecutionData
        {
            get;
            set;
        }
    }
}
