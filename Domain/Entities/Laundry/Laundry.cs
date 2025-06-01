#nullable disable

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ESMART.Domain.Entities.Laundry
{
    public class Laundry
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string ItemId { get; set; }
        public LaundaryCategory Category { get; set; } 
        public string Description { get; set; }
        public decimal LaundryPrice { get; set; }
        public decimal PressingPrice { get; set; }
    }

    public enum LaundaryCategory
    {
        Male,
        Ladies,
        Others
    }
}
