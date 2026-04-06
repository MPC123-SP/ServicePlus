using System.ComponentModel.DataAnnotations.Schema;

namespace ServicePlusAPIs.Models.SportsModel
{
    public class SportSponsorDetail
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Address { get; set; }
        public string PhoneNumber { get; set; }
        public string Email { get; set; }      
        public bool IsStatus { get; set; }

        public DateTime CreatedAt { get; set; }=DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
        public DateTime? DeletedAt { get; set; }
        public virtual ICollection<SponsorPlayer>SponsorPlayers { get; set; }
    }
}
