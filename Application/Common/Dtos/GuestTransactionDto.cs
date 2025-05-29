#nullable disable

using ESMART.Domain.Enum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ESMART.Application.Common.Dtos
{
    public class GuestTransactionDto
    {
        public string GuestId { get; set; }
        public PaymentMethod PaymentMethod { get; set; }
        public decimal Amount { get; set; }
        public string GuestAccountId { get; set; }
        public string RoomId { get; set; }
        public decimal Discount { get; set; }
        public decimal Tax { get; set; }
        public string Consumer { get; set; }
        public string BankAccountId { get; set; }
        public TransactionType TransactionType { get; set; }
        public string ApplicationUserId { get; set; }
        public string Description { get; set; }
    }
}
