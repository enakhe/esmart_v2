#nullable disable

using ESMART.Domain.Entities.StoreKeeping;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ESMART.Domain.ViewModels.StoreKepping
{
    public class MenuOrderViewModel
    {
        public string Id { get; set; }
        public string BookingId { get; set; }
        public string Guest { get; set; }
        public string Room { get; set; }
        public string Invoice { get; set; }
        public string TotalAmount { get; set; }
        public string Quantity { get; set; }
        public ICollection<OrderItemViewModel> OrderItems { get; set; } = new List<OrderItemViewModel>();
        public DateTime CreatedAt { get; set; }
    }
}
