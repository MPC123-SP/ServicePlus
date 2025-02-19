using ServicePlusAPIs.Models.SportsModel;

namespace ServicePlusAPIs.ViewModels.SportsModel
{
    public class SportSponsorDetailViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Address { get; set; }
        public string PhoneNumber { get; set; }
        public string Email { get; set; } 
        public bool IsStatus { get; set; }
        public List<SponsorPlayerViewModel> SponsorPlayers{ get; set; }
    }
}
