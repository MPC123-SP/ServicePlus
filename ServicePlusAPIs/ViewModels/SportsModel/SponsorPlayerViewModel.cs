namespace ServicePlusAPIs.ViewModels.SportsModel
{
    public class SponsorPlayerViewModel
    {
        public int InitiatedDataId{get;set; }

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
    }
}
