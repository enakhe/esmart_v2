#nullable disable

using ESMART.Domain.Entities.Data;
using ESMART.Domain.Entities.RoomSettings;
using ESMART.Domain.Entities.Transaction;
using ESMART.Domain.Enum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ESMART.Domain.Entities.FrontDesk
{
    public class RoomTypeReservation
    {
        public string Id { get; set; }
        public string GuestId { get; set; }
        public string RoomTypeId { get; set; }

        public DateTime ReservationDate { get; set; }
        public DateTime CheckInDate { get; set; }
        public DateTime CheckOutDate { get; set; }

        public decimal AdvancePayment { get; set; }
        public PaymentMethod PaymentMethod { get; set; }
        public ReservationStatus Status { get; set; }
        public decimal Discount { get; set; }

        public string BankAccountId { get; set; }
        public string BookingId { get; set; }
        public string GuestAccountId { get; set; }


        public virtual Guest Guest { get; set; }
        public virtual RoomType RoomType { get; set; }
        public virtual BankAccount BankAccount { get; set; } 
        public virtual GuestAccount GuestAccount { get; set; }
    }
}
