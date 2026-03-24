namespace ServicePlusEXT.ResponseDtos
{
    public record ReportColumnDto(string Header, string Field);

    public sealed class ServiceApplicationReportRowDto
    {
        public int Id { get; init; }
        public string ApplRefNo { get; init; } = string.Empty;
        public Dictionary<string, string> Values { get; init; } = new();
    }

    public sealed class ServiceApplicationReportResponseDto
    {
        public IReadOnlyList<ReportColumnDto> Columns { get; init; } = [];
        public IReadOnlyList<ServiceApplicationReportRowDto> Items { get; init; } = [];
        public string? NextCursor { get; init; }
        public bool HasMore { get; init; }
        public int PageSize { get; init; }
    }
}
