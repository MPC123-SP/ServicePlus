namespace ServicePlusEXT.ResponseDtos
{
    public class ServiceResponseDto
    {
        public string applicationId { get; set; } = string.Empty;
        public int serviceId { get; set; }
        public string applicantName { get; set; } = string.Empty;
        public string status { get; set; } = string.Empty;
        public string date { get; set; } = string.Empty;
    }
}
