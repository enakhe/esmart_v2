#nullable disable

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ESMART.Domain.ViewModels.FrontDesk
{
    public class RoomTypeReservationViewModel
    {
        public string Id { get; set; }
        public string Guest { get; set; }
        public string GuestId { get; set; }
        public string PhoneNumber { get; set; }
        public string RoomType { get; set; }
        public DateTime ArivalDate { get; set; }
        public DateTime DepartureDate { get; set; }
        public decimal Amount { get; set; }
    }
}
