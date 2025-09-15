#nullable disable

using ESMART.Domain.Entities.FrontDesk;
using ESMART.Domain.Entities.StoreKeeping;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ESMART.Domain.Entities.Laundry
{
    public class LaundryOrder
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string OrderId { get; set; }
        public string Invoice { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public ICollection<LaundaryOrderItem> OrderItems { get; set; }
        public decimal Amount { get; set; }

        public string BookingId { get; set; }
        public Booking Booking { get; set; }

        public bool IsCancelled { get; set; }

        public string RoomBookingId { get; set; }
        public virtual RoomBooking RoomBooking { get; set; }

        public string GuestAccountId { get; set; }
        public virtual GuestAccount GuestAccount { get; set; }
    }
}
