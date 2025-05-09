using System.Text;

namespace ESMART.Application.Common.Utils
{
    public class Helper
    {
        public static bool AreAnyNullOrEmpty(params string[] args)
        {
            foreach (var arg in args)
            {
                if (string.IsNullOrEmpty(arg))
                {
                    return true;
                }
            }
            return false;
        }

        public static double GetDateInterval(DateTime startDate, DateTime endDate)
        {
            return (endDate.Date - startDate.Date).Days;
        }

        public static decimal GetPriceByRateAndTime(DateTime startDate, DateTime endDate, decimal rate)
        {
            decimal timeSpan = (decimal)GetDateInterval(startDate, endDate);
            return timeSpan * rate;
        }

        public static decimal CalculateTotal(decimal basePrice, decimal discountPercent, decimal vatPercent, decimal serviceCharge)
        {
            decimal vatAmount = (vatPercent / 100) * basePrice;

            decimal discount = (discountPercent / 100) * basePrice;

            // Step 4: Total
            decimal total = (basePrice + vatAmount + serviceCharge) - discount;

            return Math.Round(total, 2);
        }

        public static string ByteArrayToString(byte[] byteArray)
        {
            if (byteArray == null)
                return string.Empty;

            return Encoding.ASCII.GetString(byteArray).TrimEnd('\0');
        }

    }
}
