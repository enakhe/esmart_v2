#nullable disable

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ESMART.Domain.Entities.Laundry
{
    public class LaundaryOrderItem
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();

        public string LaundryOrderItemId { get; set; }

        public string LaundaryId { get; set; }
        public Laundry Laundary { get; set; }

        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }

    }

    public class LaundaryOrderItemViewModel
    {
        public string LaundryOrderId { get; set; }
        public string Item { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal TotalPrice => Quantity * UnitPrice;
    }
}
