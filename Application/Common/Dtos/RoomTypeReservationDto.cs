#nullable disable

using ESMART.Domain.Entities.Data;
using ESMART.Domain.Entities.Transaction;
using ESMART.Domain.Enum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ESMART.Application.Common.Dtos
{
    public class RoomTypeReservationDto
    {
        public string GuestId { get; set; }
        public string RoomTypeId { get; set; }

        public DateTime CheckInDate { get; set; }
        public DateTime CheckOutDate { get; set; }

        public decimal AdvancePayment { get; set; }
        public decimal Discount { get; set; }

        public string ApplicationUserId { get; set; }
        public string GuestAccountId { get; set; }
        public BankAccount BankAccount { get; set; }
        public ApplicationUser ApplicationUser { get; set; }
        public string BankAccountId { get; set; }
        public PaymentType PaymentType { get; set; }
        public PaymentMethod PaymentMethod { get; set; }
    }
}
