using ESMART.Domain.Entities.FrontDesk;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ESMART.Domain.Entities.Account
{
    public class Transaction
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string? TransactionId { get; set; }
        public string? ServiceId { get; set; }
        public System.DateTime Date { get; set; }
        public decimal Amount { get; set; }
        public string? Status { get; set; }
        public string? Description { get; set; }
        public string? Type { get; set; }
        public bool IsTrashed { get; set; }
        public string? BankAccount { get; set; }
        public string? GuestId { get; set; }
        public required virtual Guest Guest { get; set; }
    }
}
