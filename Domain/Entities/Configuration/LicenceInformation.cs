#nullable disable

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ESMART.Domain.Entities.Configuration
{
    public class LicenceInformation
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string HotelName { get; set; }
        public string LicenceKey { get; set; }
        public DateTime ExpiryDate { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime LastUpdatedDate { get; set; }
    }
}
