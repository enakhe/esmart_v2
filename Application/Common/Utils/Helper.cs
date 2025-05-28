using ESMART.Domain.Enum;
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

        public static decimal CalculateTotal(decimal basePrice, decimal discountPercent, decimal vatPercent, decimal serviceChargePercent)
        {
            decimal vatAmount = (vatPercent / 100) * basePrice;

            decimal discount = (discountPercent / 100) * basePrice;

            decimal serviceCharge = (serviceChargePercent / 100) * basePrice;

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

        public static MakeCardType GetCardType(int cardTypeValue)
        {
            if (System.Enum.IsDefined(typeof(MakeCardType), cardTypeValue))
            {
                return (MakeCardType)cardTypeValue;
            }
            else
            {
                throw new ArgumentOutOfRangeException(nameof(cardTypeValue), "Invalid card type value.");
            }
        }

        public static (decimal RackRate, decimal DiscountAmount, decimal TaxAmount, decimal FinalTotal) CalculateRackAndDiscountedTotal(
            decimal roomRate, decimal vat, decimal serviceCharge, decimal discount)
        {
            var vatRate = vat / 100m;
            var serviceChargeRate = serviceCharge / 100m;
            var discountRate = discount / 100m;

            var markupMultiplier = 1 + vatRate + serviceChargeRate;

            // Step 1: Get rack rate before VAT and service charge
            var rackRate = roomRate / markupMultiplier;

            // Step 2: Calculate the discount amount off the rack rate
            var discountAmount = rackRate * discountRate;

            // Step 3: Get the discounted rack rate
            var discountedRackRate = rackRate - discountAmount;

            // Step 4: Calculate tax based on discounted rack rate
            var taxAmount = discountedRackRate * (vatRate + serviceChargeRate);

            // Step 5: Final total (discounted rack + tax)
            var finalTotal = discountedRackRate + taxAmount;

            return (rackRate, discountAmount, taxAmount, finalTotal);
        }

    }
}
