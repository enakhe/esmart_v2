#nullable disable

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ESMART.Domain.Entities.FrontDesk
{
    public class RoomNightCharge
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string RoomBookingId { get; set; }
        public DateTime Night { get; set; }

        public RoomBooking RoomBooking { get; set; }
    }
}
