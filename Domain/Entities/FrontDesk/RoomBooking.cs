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

        public virtual Booking Booking { get; set; }
        public virtual Room Room { get; set; }
    }

}
