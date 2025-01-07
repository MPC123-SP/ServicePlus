using ServicePlusAPIs.Models.ExecutionModel;
using ServicePlusAPIs.Models.InitiatedModel;

namespace ServicePlusAPIs.ViewModels
{
    public class ServiceViewModel
    {
        public List<InitiatedDataViewModel>? initiated_data
        {
            get; 
            set; 
        }
        public List<ExecutionDataViewModel>? execution_data 
        { 
            get;
            set;
        }
    }
}
