using ServicePlusAPIs.Models.InitiatedModel;
using ServicePlusAPIs.ViewModels;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ServicePlusAPIs.Models.CommonModel.ExecutionCommonModel
{
    public class TaskDetailViewModel
    {
    
        public string? amount
        { 
            get;
            set;
        }
        public int? appl_id
        { 
            get;
            set;
        }
        public string? remarks
        { 
            get;
            set;
        }
        public int? task_id
        { 
            get;
            set;
        }
        public int? action_no
        { 
            get;
            set;
        }
        public string? task_name
        {
            get;
            set;
        }
        public int? task_type
        {
            get;
            set;
        }
        public string? user_name
        {
            get;
            set;
        }
        public int? service_id
        { 
            get; 
            set; 
        }

        public   UserDetailViewModel? user_detail
        {
            get;
            set; 
        }
        public string? action_taken
        {
            get;
            set; 
        }
        public string? payment_date
        {
            get;
            set;
        }
        public string? payment_mode
        { 
            get; 
            set;
        }
        public int? pull_user_id
        { 
            get;
            set;
        }
        public string? executed_time
        { 
            get;
            set;
        }
        public string? received_time
        { 
            get; 
            set;
        }
        public string? payment_ref_no
        {
            get; 
            set;
        }
        public int? current_process_id
        { 
            get; 
            set;
        }
        public string? callback_curr_proc_id
        {
            get;
            set;
        }
       
    }

    public class UserDetailViewModel
    {
  
        public string? user_name
        { 
            get;
            set; 
        }
        public string? designation
        { 
            get;
            set;
        }
        public string? location_id
        {
            get; 
            set;
        }
        public int? pull_user_id
        {
            get; 
            set; 
        }
        public string? location_name
        { 
            get;
            set;
        }
        public string? department_level
        {
            get;
            set; 
        }
        public string? location_type_id
        { 
            get;
            set;
        }
        public int? current_process_id
        { 
            get;
            set; 
        }
    }

}
