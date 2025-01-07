using Microsoft.EntityFrameworkCore.Metadata.Internal;
using ServicePlusAPIs.Models.InitiatedModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ServicePlusAPIs.Models.EnclouserDetails
{
    public class EnclosureDetail
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int? EnclouserID
        { 
            get;
            set;
        }
        public int? InitiatedDataId 
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
        public string? EnclousersId 
        { 
            get;
            set; 
        }
        public int? EnclousersValue
        { 
            get;
            set;
        }

    }
}
