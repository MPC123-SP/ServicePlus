using System.ComponentModel.DataAnnotations;

namespace ServicePlusAPIs.Models.SportsModel
{
    public class SportsCertificateExcelRecord
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
        public string? Score { get; set; }
        public string? ConveyorName { get; set; }
        //public DateTime? CertificateGeneratedTime { get; set; }
        public string? CertificatePath { get; set; }
        public string? CertificateSerialNo { get; set; }
        public string? ApplicantAgeGroupPB { get; set; }
        public string? ApplicantEventPB { get; set; }
        public string? ApplicantFatherNamePB { get; set; }
        public string? ApplicantFullNamePB { get; set; }
        public string? ApplicantGamePB { get; set; }
        public string? ConveyorNamePB { get; set; }
        public string? GameHeldDistrictPB { get; set; }
        public string? GameRepresentingDistrictPB { get; set; }
        public string? ScorePB { get; set; }
        public DateTime? UpdatedAt { get; set; } = DateTime.Now;
        public bool IsProcessed { get; set; } = false;
    }
}
