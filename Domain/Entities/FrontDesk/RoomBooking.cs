#nullable disable

using ESMART.Domain.Entities.RoomSettings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ESMART.Domain.Entities.FrontDesk
{
    public class RoomBooking
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string BookingId { get; set; }
        public string RoomId { get; set; }

        public string OccupantName { get; set; }
        public string OccupantPhoneNumber { get; set; }
        public DateTime CheckIn { get; set; }
        public DateTime CheckOut {  get; set; }
        public DateTime Date {  get; set; }
        public decimal Rate { get; set; }
        public decimal Discount { get; set; }
        public decimal ServiceCharge { get; set; }
        public decimal Tax { get; set; }

        public virtual Booking Booking { get; set; }
        public virtual Room Room { get; set; }
    }

}
