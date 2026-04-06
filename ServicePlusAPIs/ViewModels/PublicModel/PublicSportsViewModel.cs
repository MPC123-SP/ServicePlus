using System.ComponentModel.DataAnnotations;
using System.Reflection;

namespace ServicePlusAPIs.ViewModels.PublicModel
{
    public class PublicSportsViewModel
    {
        public int? InitiatedDataId { get; set; }
        public int? AttributeDetailID { get; set; }

        public int? TaskDetailID { get; set; }
        public int? ExecutionDataId { get; set; }
        public int? OfficialFormDetailID { get; set; }
        public int? ApplId { get; set; } //this field is available in InitiatedData and TaskDetail Table
        public string? ApplRefNo { get; set; }// this field is only available in InitiatedData Table

        public string? TaskName { get; set; }
        public int? TaskId { get; set; }
        public string? ServiceId { get; set; }
        public string? ServiceName { get; set; }// this field is only available in InitiatedData Table
        public DateTime? SubmissionDate { get; set; }
        public string? ApplicantFirstName { get; set; }
        public string? ApplicantFatherName { get; set; }
        public string? ApplicantBloodGroup { get; set; }
        public string? ApplicantMobileNo { get; set; }
        public string? ApplicantGender { get; set; }
        public string? ApplicantGame { get; set; }
        public string? Level { get; set; }
        public string? ApplicationType { get; set; }// 1 Means Sports Player Directory // 2 Registration For Participation 170608
        public string? ApplicantAgeGroup { get; set; }
        public string? ApplicantEvent { get; set; }
        public string? ApplicantGameCategory { get; set; }
        public string? ApplicantMedal { get; set; }
        public string? IsMedalist { get; set; }
        public string? District { get; set; }
        public string? Block { get; set; }

    }
    public class PlayerDetail
    {
        public string? PlayerName { get; set; }
        public string? DateOfBirth { get; set; }
        public string? MobileNumber { get; set; }
        public string? Email { get; set; }
        public string? ApplicationRefNo { get; set; }
        public string? Position { get; set; }
    }
    public class PlayerDetailsViewModel
    {
        public int? InitiatedDataId { get; set; }
        public int? AttributeDetailID { get; set; }

        public int? ExecutionDataId { get; set; }
        public int? OfficialFormDetailID { get; set; }
        public int? ApplId { get; set; } //this field is available in InitiatedData and TaskDetail Table
        public string? ApplRefNo { get; set; }// this field is only available in InitiatedData Table
        public int? TaskId { get; set; }
        public string? ServiceId { get; set; }
        public string? ServiceName { get; set; }// this field is only available in InitiatedData Table
        public DateTime? SubmissionDate { get; set; }
        public string? ApplicantFirstName { get; set; }
        public string? ApplicantFatherName { get; set; }
        public string? ApplicantMotherName { get; set; }
        public string? ApplicantDOB { get; set; }
        public string? ApplicantAge { get; set; }
        public string? StateOfBirth { get; set; }
        public string? DistrictOfBirth { get; set; }
        public string? PANCardNumber { get; set; }
        public string? ApplicantBloodGroup { get; set; }
        public string? ApplicantMobileNo { get; set; }
        public string? ApplicantAlternateMobileNumber { get; set; }
        public string? ApplicantEmail { get; set; }
        public string? ApplicantGender { get; set; }
        public string? ApplicantGame { get; set; }
        public string? Level { get; set; }
        public string? ApplicationType { get; set; }// 1 Means Sports Player Directory // 2 Registration For Participation 170608
        public string? ApplicantAgeGroup { get; set; }
        public string? ApplicantEvent { get; set; }
        public string? ApplicantGameCategory { get; set; }
        public string? ApplicantMedal { get; set; }
        public string? IsMedalist { get; set; }
        public string? District { get; set; }
        public string? Block { get; set; }
        public string? PhysicalDisability { get; set; }
        public string? MaritialStatus { get; set; }
        public string? SpouseName { get; set; }
        public string? IsEmployed { get; set; }
        public string? EmploymentStatus { get; set; }
        public string? JobDescription { get; set; }
        public string? CompleteAddress { get; set; }
        public string? Region { get; set; }
        public string? AddState { get; set; }
        public string? AddDistrict { get; set; }
        public string? AddPincode { get; set; }
        public string? AccountNumber { get; set; }
        public string? AccountHolder { get; set; }
        public string? IFSCCode { get; set; }
        public string? NameOnPassbook { get; set; }
        public string? BankAddress { get; set; }
        public string? BankName { get; set; }
        public string? ApplicationToBeSubmitted { get; set; }
        public List<PlayerEducation> PlayerEducations { get; set; }

    }



    public class PlayerEducation
    {

        public string? Qualification { get; set; }
        public string? InstituteName { get; set; }
        public string? PassingYear { get; set; }
    }
    public class PlayerTrainingDetail
    {

        public string? CoachName { get; set; }
        public string? PhoneNumber { get; set; }
        public string? Email { get; set; }
        public string? CoachType { get; set; }//whether it is Private or GOVT
        public string? TrainingCenterName { get; set; }
        public string? Game { get; set; }
        public string? TrainingFrom { get; set; }
        public string? TrainingTo { get; set; }

        public string? District { get; set; }
    }
    public class PlayerAchievements
    {
        public string? CompetitionType { get; set; }
        public List<InterNationalAchievements>? InterNationalAchievements { get; set; }
        public List<NationalAchievements>? NationalAchievements { get; set; }
        public List<StateAchievements>? StateAchievements { get; set; }
        public List<DistrictAchievements>? DistrictAchievements { get; set; }
        public List<BlockAchievements>? BlockAchievements { get; set; }

    }

    public class InterNationalAchievements
    {
        public string? Game { get; set; }
        public string? GameCategory { get; set; }
        public string? GameType { get; set; }
        public string? AgeGroup { get; set; }
        public string? GameEvent { get; set; }
        public string? TournamentName { get; set; }
        public string? TournamentFrom { get; set; }
        public string? TournamentTo { get; set; }
        public string? Position { get; set; }

    }
    public class NationalAchievements
    {
        public string? Game { get; set; }
        public string? GameCategory { get; set; }
        public string? GameType { get; set; }
        public string? AgeGroup { get; set; }
        public string? GameEvent { get; set; }
        public string? TournamentName { get; set; }
        public string? TournamentFrom { get; set; }
        public string? TournamentTo { get; set; }
        public string? Position { get; set; }

    }
    public class StateAchievements
    {
        public string? State { get; set; }
        public string? Game { get; set; }
        public string? GameCategory { get; set; }
        public string? GameType { get; set; }
        public string? AgeGroup { get; set; }
        public string? GameEvent { get; set; }
        public string? TournamentName { get; set; }
        public string? TournamentFrom { get; set; }
        public string? TournamentTo { get; set; }
        public string? Position { get; set; }

    }
    public class DistrictAchievements
    {
        public string? District { get; set; }
        public string? Game { get; set; }
        public string? GameCategory { get; set; }
        public string? GameType { get; set; }
        public string? AgeGroup { get; set; }
        public string? GameEvent { get; set; }
        public string? TournamentName { get; set; }
        public string? TournamentFrom { get; set; }
        public string? TournamentTo { get; set; }
        public string? Position { get; set; }

    }
    public class BlockAchievements
    {
        public string? District { get; set; }
        public string? Block { get; set; }
        public string? Game { get; set; }
        public string? GameCategory { get; set; }
        public string? GameType { get; set; }
        public string? AgeGroup { get; set; }
        public string? GameEvent { get; set; }
        public string? TournamentName { get; set; }
        public string? TournamentFrom { get; set; }
        public string? TournamentTo { get; set; }
        public string? Position { get; set; }

    }

    public class FilterParameter
    {
        public int page { get; set; }
        public int pageSize { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string? SearchValue { get; set; }
        public string? Tournament { get; set; }
        public string? Gender { get; set; }
        public string? Level { get; set; }
        public string? ApplicantGame { get; set; }
        public string? ApplicantAgeGroup { get; set; }
        public string? ApplicantGameCategory { get; set; }
        public string? ApplicationType { get; set; }
        public string? IsMedalist { get; set; }
        public string? District { get; set; }
        public string? Block { get; set; }
        public string? ApplicantEvent { get; set; }
    }

    public class PlayerCertificateDetail
    {
        public string? ApplRefNo { get; set; }
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
    }
    public class Conveyor
    {
        public int Id { get; set; }
        public string? ConveyorName { get; set; }
        public string? SignPath { get; set; }//Signature
    }
    public class FilterParameterForPlayerCertificate
    {
        public string? District { get; set; }//State,District,Block,National,International
        public string? CertificateSerialNoFrom { get; set; } 
        public string? CertificateSerialNoTo { get; set; } 
        public string? ApplicantGame { get; set; }
        public string? ApplicantEvent { get; set; }
        public string? ApplicantAgeGroup { get; set; }

    }
    public class CertificateIssuedViewModel
    { 
        public string? ApplRefNo { get; set; }
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
        public string? TaskId { get; set; }
        public string? ServiceId { get; set; }
        public string? TaskName { get; set; }
        public string? OfficialUserName { get; set; } 

    }
}
