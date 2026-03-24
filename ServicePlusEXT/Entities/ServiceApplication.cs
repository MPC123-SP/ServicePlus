using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace ServicePlusEXT.Entities
{
    [Index(nameof(ApplRefNo), IsUnique = true)]
    [Index(nameof(ApplId), IsUnique = true)]
    public class ServiceApplication
    {
        public int Id { get; set; }


        [Required]
        public string ApplRefNo { get; set; } = string.Empty;

        [Required]
        public string ApplId { get; set; } = string.Empty;

        public string AppliedBy { get; set; } = string.Empty;

        // Navigation
        public List<ApplicationAttribute> Attributes { get; set; } = new();
    }
    public class ApplicationAttribute
    {
        public int Id { get; set; }

        public int ServiceApplicationId { get; set; } // FK

        public string Key { get; set; } = string.Empty;

        public string Value { get; set; } = string.Empty;

        // Navigation
        public ServiceApplication ServiceApplication { get; set; } = null!;
    }
}
