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
    }
}
