using ServicePlusAPIs.Models.ExecutionModel;
using ServicePlusAPIs.Models.InitiatedModel;
using ServicePlusAPIs.ViewModels.ExecutionModel;
using ServicePlusAPIs.ViewModels.InitiatedModel;

namespace ServicePlusAPIs.ViewModels
{
    public class ServiceViewModelZombie
    {
        public List<InitiatedDataViewModelZombie>? initiated_data
        {
            get;
            set;
        }
        public List<ExecutionDataViewModelZombie>? execution_data
        {
            get;
            set;
        }
    }
}
