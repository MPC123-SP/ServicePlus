using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ServicePlusEXT.Entities
{
    [Index(nameof(ApplRefNo), nameof(ApplId), IsUnique = true)]
    [Table("ServiceApplications")]
    public class ServiceApplication
    {
        public int Id { get; set; }

        [Required]
        public string ApplRefNo { get; set; } = string.Empty;

        [Required]
        public string ApplId { get; set; } = string.Empty;

        public string AppliedBy { get; set; } = string.Empty;

        public List<ApplicationAttribute> Attributes { get; set; } = new();
    }

    [Table("ApplicationAttribute")]
    public class ApplicationAttribute
    {
        public int Id { get; set; }

        public int ServiceApplicationId { get; set; }

        public string Key { get; set; } = string.Empty;

        public string Value { get; set; } = string.Empty;

        public ServiceApplication ServiceApplication { get; set; } = null!;
    }
}
