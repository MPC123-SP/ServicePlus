using ServicePlusAPIs.Models.EnclouserDetails;
using ServicePlusAPIs.Models.ServiceWiseModels.PSEB_Initiated_AttributeDetails;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ServicePlusAPIs.Models.InitiatedModel
{
    public class InitiatedDataViewModel
    {
 
        public string? department_id
        {
            get; 
            set;
        }
        public string? department_name
        {
            get;
            set;
        }
        public string? service_id
        { 
            get; 
            set;
        }
        public string? service_name
        {
            get; 
            set;
        }
        public string? appl_id
        { 
            get;
            set;
        }
        public string? appl_ref_no
        { 
            get;
            set;
        }
        public string? no_of_attachment
        {
            get;
            set;
        }
        public string? submission_mode
        { 
            get; 
            set;
        }
        public string? submission_date
        { 
            get; 
            set;
        }
        public string? applied_by
        {
            get;
            set;
        }
        public string? submission_location
        {
            get;
            set;
        }
        public string? submission_location_id
        { 
            get; 
            set;
        }
        public string? submission_location_type_id
        { 
            get;
            set;
        }
        public string? payment_mode
        {
            get;
            set; 
        }
        public string? reference_no
        {
            get;
            set; 
        }
        public string? payment_date
        { 
            get; 
            set;
        }
        public string? amount
        { 
            get; 
            set;
        }
        public string? registration_id
        { 
            get;
            set; 
        }
        public string? base_service_id
        {
            get; 
            set;
        }
        public string? version_no
        { 
            get; 
            set;
        }
        public string? sub_version
        { 
            get; 
            set; 
        }

        //public  List<EnclosureDetailViewModel>? enclosure_details
        //{
        //    get;
        //    set;
        //}
        public List<Dictionary<string, int>>? enclosure_details 
        { 
            get; 
            set;
        }  
        public Dictionary<string, string>? attribute_details 
        { 
            get; 
            set;
        }

        //public AttributeDetailViewModel? attribute_details
        //{
        //    get; 
        //    set; 
        //}
    }
}
