using System.ComponentModel.DataAnnotations.Schema;

namespace ServicePlusAPIs.Models.SportsModel
{
    public class SponsorPlayer
    {
        public int Id { get; set; }
        public int InitiatedDataId { get; set; }
        public string ApplRefNo { get; set; }
        public string Game { get; set; }
        /// <summary>
        /// Kind-> equipments,kit
        /// Cash->Cash/Bank
        /// </summary>
        public string SponsorType { get; set; }
        public string? CashAward { get; set; }
        public string? KindAward { get; set; }
        public bool IsApproved { get; set; }
        public bool IsStatus { get; set; }
         
        public DateTime CreatedAt { get; set; }=DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
        public DateTime? DeletedAt { get; set; }
        public int SportSponsorDetailId {  get; set; }
        [ForeignKey("SportSponsorDetailId")]
        public virtual SportSponsorDetail SportSponsorDetail { get; set; }
    }
}
