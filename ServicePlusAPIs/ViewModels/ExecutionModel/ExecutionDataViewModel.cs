using ServicePlusAPIs.Models.CommonModel.ExecutionCommonModel;
using ServicePlusAPIs.Models.ServiceWiseModels.PSEB_Execution_OfficialFormDetails;

namespace ServicePlusAPIs.Models.ExecutionModel
{
    public class ExecutionDataViewModel
    {
        public TaskDetailViewModel? task_details
        {
            get;
            set; 
        }
        public Dictionary<string, string>? official_form_details
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
