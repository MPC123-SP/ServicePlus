using System.ComponentModel.DataAnnotations;

namespace ServicePlusEXT.Entities
{
    public class ServiceApplication
    {
        [Key]
        public int Id { get; set; } // PK

        public string ApplicationId { get; set; } = string.Empty;

        public int ServiceId { get; set; }

        public string ApplicantName { get; set; } = string.Empty;

        public string Status { get; set; } = string.Empty;

        public DateTime Date { get; set; }
    }
}
