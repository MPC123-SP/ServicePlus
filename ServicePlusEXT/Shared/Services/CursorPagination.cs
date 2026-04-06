using System.Text;

namespace ServicePlusEXT.Shared.Services
{
    public static class CursorPagination
    {
        public static string EncodeInt32(int value)
        {
            return Convert.ToBase64String(Encoding.UTF8.GetBytes(value.ToString()));
        }

        public static bool TryDecodeInt32(string? cursor, out int value)
        {
            value = 0;

            if (string.IsNullOrWhiteSpace(cursor))
            {
                return true;
            }

            try
            {
                var decoded = Encoding.UTF8.GetString(Convert.FromBase64String(cursor));
                return int.TryParse(decoded, out value);
            }
            catch (FormatException)
            {
                return false;
            }
        }
    }
}
