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
    }
}
