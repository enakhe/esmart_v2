#nullable disable

using ESMART.Domain.Entities.FrontDesk;
using ESMART.Domain.Entities.RoomSettings;
using ESMART.Domain.Enum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ESMART.Application.Common.Dtos
{
    public class GuestBookingSummaryDto
    {
        public string BookingId { get; set; }
        public DateTime CheckIn { get; set; }
        public DateTime CheckOut { get; set; }
        public string Duration { get; set; }
        public decimal Amount { get; set; }
        public BookingStatus Status { get; set; }
        public ICollection<RoomBooking> Rooms { get; set; }
        public DateTime DateCreated { get; set; }

        // Guest Details
        public Guest Guest { get; set; }
    }
}
