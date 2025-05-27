#nullable disable

using ESMART.Domain.Entities.RoomSettings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ESMART.Domain.Entities.FrontDesk
{
    public class BookingDetail
    {
        public string GuestAccountId { get; set; }
        public string RoomId { get; set; }
        public string InvoiceId { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public decimal Rate { get; set; }
        public Room Room { get; set; }

        public string Description => Room != null && Room.RoomType != null
            ? $"{Room.RoomType.Name} ({Room.Number}) from {StartDate:dd/MM/yy} to {EndDate:dd/MM/yy}"
            : "Room details are not available";
    }

}
