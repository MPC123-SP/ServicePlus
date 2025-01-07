using ServicePlusAPIs.Models.InitiatedModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ServicePlusAPIs.Models.ServiceWiseModels.PSEB_Execution_OfficialFormDetails
{
    public class OfficialFormDetailViewModel
    {

        public string? OfficalFormID
        {
            get;
            set;
        }
        public string? OfficalFormValue
        {
            get;
            set;
        }

    }
}
