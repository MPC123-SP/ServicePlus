namespace ServicePlusEXT.Shared.Services
{
    public sealed record ServiceApplicationReportColumn(string Header, string Field, string? AttributeKey = null);

    public static class ServiceApplicationReportDefinition
    {
        public const string ApplicantProfileAttributeKey = "177033";
        public const string SignatureAttributeKey = "177034";
        public const string Base64ImagePrefix = "data:image/png;base64,";

        public static readonly IReadOnlyList<ServiceApplicationReportColumn> Columns =
        [
            new("S.No", "id"),
            new("Application Reference No", "applRefNo"),
            new("Post Details", "177021", "177021"),
            new("Advt. No.", "179348", "179348"),
            new("Applicant's Name", "177022", "177022"),
            new("Father's Name", "177023", "177023"),
            new("Mother's Name", "177024", "177024"),
            new("Marital Status", "177025", "177025"),
            new("Date of Birth", "177026", "177026"),
            new("Age", "178553", "178553"),
            new("Mobile Number", "177036", "177036"),
            new("E-Mail", "177029", "177029"),
            new("Category", "177039", "177039"),
            new("Gender", "177027", "177027"),
            new("Applicant Profile", ApplicantProfileAttributeKey, ApplicantProfileAttributeKey),
            new("Signature", SignatureAttributeKey, SignatureAttributeKey),
            new("Identity Proof", "177030", "177030"),
            new("Identity Proof No", "178531", "178531"),
            new("Domicile of Punjab", "177038", "177038"),
            new("Aadhar last 8 digits", "179265", "179265"),
            new("Amount", "amount"),
            new("Criminal Case Registered", "179218", "179218"),
            new("Criminal Case Details", "179231", "179231"),
            new("Govt. Employee", "179202", "179202"),
            new("Detail of Govt. Job", "179349", "179349"),
            new("Punjabi Passed (Yes/No)", "177052", "177052"),
            new("Is Degree from a University in ", "177053", "177053"),
            new("Address", "177068", "177068"),
            new("STATE", "177069", "177069"),
            new("DISTRICT", "177070", "177070"),
            new("PINCODE", "177074", "177074"),
            new("10th Qualification", "179278", "179278"),
            new("Year", "179279", "179279"),
            new("Board/University", "179280", "179280"),
            new("Total Marks / CGPA Multiplier", "179281", "179281"),
            new("Obtained Marks/CGPA", "179282", "179282"),
            new("Percentage", "179283", "179283"),
            new("Mode of Degree", "179284", "179284"),
            new("State / International", "179355", "179355"),
            new("12th Qualification", "179285", "179285"),
            new("Year", "179286", "179286"),
            new("Board/University", "179287", "179287"),
            new("Total Marks / CGPA Multiplier", "179288", "179288"),
            new("Obtained Marks/CGPA", "179289", "179289"),
            new("Percentage", "179290", "179290"),
            new("Mode of Degree", "179291", "179291"),
            new("State / International", "179356", "179356"),
            new("Degree Qualification", "179294", "179294"),
            new("Year", "179295", "179295"),
            new("Board/University", "179296", "179296"),
            new("Total Marks / CGPA Multiplier", "179297", "179297"),
            new("Obtained Marks/CGPA", "179298", "179298"),
            new("Percentage", "179299", "179299"),
            new("Mode of Degree", "179300", "179300"),
            new("State / International", "179357", "179357")
        ];

        public static readonly HashSet<string> AttributeKeys = Columns
            .Where(column => column.AttributeKey is not null)
            .Select(column => column.AttributeKey!)
            .ToHashSet(StringComparer.Ordinal);

        public static string FormatAttributeValue(string attributeKey, string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return value;
            }

            return attributeKey is ApplicantProfileAttributeKey or SignatureAttributeKey
                && !value.StartsWith(Base64ImagePrefix, StringComparison.OrdinalIgnoreCase)
                    ? $"{Base64ImagePrefix}{value}"
                    : value;
        }
    }
}
