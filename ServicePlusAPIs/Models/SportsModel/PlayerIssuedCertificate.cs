using System.ComponentModel.DataAnnotations;

namespace ServicePlusAPIs.Models.SportsModel
{
    public class PlayerIssuedCertificate
    {
        [Key]
        public int Id { get; set; }
        public string? ApplicantFullName { get; set; }
        public string? ApplicantFatherName { get; set; }
        public string? ApplicantDOB { get; set; }
        public string? ApplicantMobileNo { get; set; }
        public string? GameHeldDistrict { get; set; }
        public string? GameRepresentingDistrict { get; set; }
        public string? ApplicantGame { get; set; }
        public string? TournamentFrom { get; set; }
        public string? TournamentTo { get; set; }
        public string? ApplicantEvent { get; set; }
        public string? ApplicantAgeGroup { get; set; }
        public string? Position { get; set; }
        public string? Score { get; set; }//Time/Distance /Height/Weight/Score
        public string? ConveyorName { get; set; }
        public DateTime? CertificateGeneratedTime { get; set; }
        public string? CertificatePath { get; set; }
        public string? CertificateSerialNo { get; set; }
    }
}
