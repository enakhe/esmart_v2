#nullable disable

using ESMART.Domain.Entities.Data;
using ESMART.Domain.Entities.Transaction;
using ESMART.Domain.Enum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ESMART.Domain.Entities.FrontDesk
{
    public class GuestTransaction
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string GuestTraId { get; set; } = $"TRP-{Guid.NewGuid().ToString().Split('-')[0].ToUpper().AsSpan(0, 5)}";
        public string GuestId { get; set; }
        public string ApplicationUserId { get; set; }
        public string Invoice { get; set; }
        public decimal Discount { get; set; }
        public decimal BillPosts { get; set; }
        public decimal Amount { get; set; }
        public decimal Tax { get; set; }
        public decimal Payment { get; set; }
        public string Description { get; set; }
        public string BankAccountId { get; set; }
        public string Consumer { get; set; }
        public string RoomId { get; set; }
        public PaymentMethod PaymentMethod { get; set; }
        public TransactionType TransactionType { get; set; }
        public DateTime Date { get; set; }
        public string GuestAccountId { get; set; }

        public Guest Guest { get; set; }
        public Domain.Entities.RoomSettings.Room Room {get; set;}
        public ApplicationUser ApplicationUser { get; set; }
        public virtual BankAccount BankAccount { get; set; }
        public virtual GuestAccount GuestAccount { get; set; }
    }
}
