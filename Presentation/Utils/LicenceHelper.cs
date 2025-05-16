using System.Security.Cryptography;
using System.Text;

namespace ESMART.Presentation.Utils
{
    public class LicenceHelper
    {
        private static string CreateHash(string input)
        {
            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(input));
                StringBuilder result = new StringBuilder();
                foreach (byte b in bytes)
                {
                    result.Append(b.ToString("X2"));
                }
                return result.ToString();
            }
        }

        public static bool ValidateProductKey(string hotelName, string productKey)
        {
            string secretKey = "ABCd1234!@#$EFGh5678!@#$IJKl890MNOPESMARTHMSAfrica";
            var parts = productKey.Split('-');
            if (parts.Length != 4) return false;

            string part1 = parts[0].Substring(4, 2);
            string part2 = parts[1].Substring(4, 2);
            string part3 = parts[2].Substring(4, 2);
            string part4 = parts[3].Substring(4, 2);

            string expirationDateStr = $"{part1}{part2}{part3}{part4}";
            DateTime expirationDate;
            if (!DateTime.TryParseExact(expirationDateStr, "yyyyMMdd", null, System.Globalization.DateTimeStyles.None, out expirationDate))
            {
                return false;
            }

            if (DateTime.Now > expirationDate)
            {
                return false;
            }

            string dataToHash = $"{hotelName}-{expirationDateStr}-{secretKey}";
            string expectedHash = CreateHash(dataToHash);

            bool isValid =
                parts[0].Substring(0, 4) == expectedHash.Substring(0, 4) &&
                parts[1].Substring(0, 4) == expectedHash.Substring(4, 4) &&
                parts[2].Substring(0, 4) == expectedHash.Substring(8, 4) &&
                parts[3].Substring(0, 4) == expectedHash.Substring(12, 4);

            return isValid;
        }

        public static string? GetExpirationDate(string productKey)
        {
            var parts = productKey.Split('-');
            if (parts.Length != 4) return null;

            string part1 = parts[0].Substring(4, 2);
            string part2 = parts[1].Substring(4, 2);
            string part3 = parts[2].Substring(4, 2);
            string part4 = parts[3].Substring(4, 2);

            string expirationDateStr = $"{part1}{part2}{part3}{part4}";
            return expirationDateStr;
        }

        public static string GenerateProductKey(string hotelName, DateTime expirationDate)
        {
            string secretKey = "ABCd1234!@#$EFGh5678!@#$IJKl890MNOPESMARTHMSAfrica";

            string expirationDateStr = expirationDate.ToString("yyyyMMdd");

            string part1 = expirationDateStr.Substring(0, 2);
            string part2 = expirationDateStr.Substring(2, 2);
            string part3 = expirationDateStr.Substring(4, 2);
            string part4 = expirationDateStr.Substring(6, 2);

            string dataToHash = $"{hotelName}-{expirationDateStr}-{secretKey}";
            string hash = CreateHash(dataToHash);

            string productKey = $"{hash.Substring(0, 4)}{part1}-{hash.Substring(4, 4)}{part2}-{hash.Substring(8, 4)}{part3}-{hash.Substring(12, 4)}{part4}";

            return productKey;
        }
    }
}
