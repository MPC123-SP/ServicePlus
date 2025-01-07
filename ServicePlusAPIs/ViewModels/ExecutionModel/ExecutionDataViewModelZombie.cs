using ServicePlusAPIs.Models.CommonModel.ExecutionCommonModel;
using ServicePlusAPIs.ViewModels.CommonModel.ExecutionCommonModel;
using ServicePlusAPIs.ViewModels.ServiceWiseModels.PSEB_Execution_OfficialFormDetails;

namespace ServicePlusAPIs.ViewModels.ExecutionModel
{
    public class ExecutionDataViewModelZombie
    {
        public TaskDetailViewModelZombie? task_details
        {
            get;
            set;
        }       
        public List<OfficialFormDetailViewModelZombie>? official_form_details
        {
            get;
            set;
        }
        public string? applicant_task_details
        {
            get;
            set;
        }
    }
}
