using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ESMART.Domain.Entities.Configuration
{
    public class CurrencyOption
    {
        public string Symbol { get; set; }
        public string Code { get; set; }
        public string Display => $"{Symbol} - {Code}";
    }

}
