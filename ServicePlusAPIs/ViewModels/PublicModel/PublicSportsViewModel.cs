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
        public string? ApplicantGender { get; set; }
        public string? ApplicantGame { get; set; }
        public string? ApplicantAgeGroup { get; set; }
        public string? ApplicantEvent { get; set; }
        public string? ApplicantGameCategory { get; set; }
        public string? ApplicantMedal { get; set; }

    }
}
