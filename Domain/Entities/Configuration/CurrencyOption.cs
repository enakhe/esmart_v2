#nullable disable

namespace ESMART.Domain.Entities.Configuration
{
    public class CurrencyOption
    {
        public string Symbol { get; set; }
        public string Code { get; set; }
        public string Display => $"{Symbol} - {Code}";
    }

}
