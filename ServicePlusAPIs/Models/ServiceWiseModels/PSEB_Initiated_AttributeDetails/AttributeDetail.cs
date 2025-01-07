using ServicePlusAPIs.Models.InitiatedModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ServicePlusAPIs.Models.ServiceWiseModels.PSEB_Initiated_AttributeDetails
{
    public class AttributeDetail
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int? AttributeDetailID 
        { 
            get;
            set;
        }
        public int InitiatedDataId
        {
            get;
            set;
        }
        [ForeignKey("InitiatedDataId")]
        public virtual InitiatedData? InitiatedData
        {
            get;
            set;
        }
        public string? ApplicationFormFieldID
        {
            get; 
            set; 
        }
        public string? ApplicationFormFieldValue
        {
            get; 
            set;
        }
       


    }

}
