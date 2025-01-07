using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace ServicePlusAPIs.HelperModels
{
    public class CustomAttributeLabel
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int CustomAttributLabelId 
        { 
            get;
            set; 
        }

        public int? ApplicationFormId
        {
            get;
            set;
        }
        public string? ApplicationFormLabel
        {
            get; 
            set;
        }    
        public string? ServiceName
        {
            get; 
            set;
        }
    }
}
