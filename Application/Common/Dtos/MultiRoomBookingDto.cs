#nullable disable

using ESMART.Domain.Entities.FrontDesk;
using ESMART.Domain.Enum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ESMART.Application.Common.Dtos
{
    public class MultiRoomBookingDto
    {
        public string GuestId { get; set; }
        public string GuestAccountId { get; set; }
        public List<RoomBooking> RoomBookings { get; set; }
        public DateTime CheckIn { get; set; }
        public DateTime CheckOut { get; set; }
        public decimal Discount { get; set; }
        public decimal VAT { get; set; }
        public decimal ServiceCharge { get; set; }
        public string AccountNumber { get; set; }
        public PaymentMethod PaymentMethod { get; set; }
        public string ApplicationUserId { get; set; }
    }

}
