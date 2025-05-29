#nullable disable

using ESMART.Domain.Entities.FrontDesk;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ESMART.Domain.Entities.StoreKeeping
{
    public class Order
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string OrderId { get; set; }
        public string Invoice {  get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public ICollection<OrderItem> OrderItems { get; set; }
        public decimal Amount { get; set; }

        public string BookingId { get; set; }
        public Booking Booking { get; set; }

        public string RoomBookingId { get; set; }
        public virtual RoomBooking RoomBooking { get; set; }

        public string GuestAccountId { get; set; }
        public virtual GuestAccount GuestAccount { get; set; }
    }
}
