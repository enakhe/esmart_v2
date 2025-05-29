#nullable disable

using ESMART.Domain.Entities.Data;
using ESMART.Domain.Entities.FrontDesk;
using ESMART.Domain.Entities.StoreKeeping;
using ESMART.Domain.Entities.Transaction;
using ESMART.Domain.Enum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ESMART.Application.Common.Dtos
{
    public class CreateOrderDto
    {
        public string GuestId { get; set; }
        public string OrderId { get; set; }
        public string BookingId { get; set; }
        public List<OrderItem> OrderItems { get; set; }
        public string RoomBookingId { get; set; }
        public virtual RoomBooking RoomBooking { get; set; }
        public string RoomId { get; set; }
        public string Consumer { get; set; }
        public string ApplicationUserId { get; set; }
        public ApplicationUser ApplicationUser { get; set; }
        public string GuestAccountId { get; set; }
        public GuestAccount GuestAccount { get; set; }
        public string Invoice { get; set; }
        public decimal Amount { get; set; }

        public string BankAccountId { get; set; }
        public BankAccount BankAccount { get; set; }

        public PaymentMethod PaymentMethod { get; set; }
        public TransactionType TransactionType  { get; set; }
    }
}
