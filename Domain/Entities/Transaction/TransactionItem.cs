#nullable disable

using ESMART.Domain.Entities.Data;
using ESMART.Domain.Enum;

namespace ESMART.Domain.Entities.Transaction
{
    public class TransactionItem
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string Description { get; set; }
        public string ServiceId { get; set; }
        public decimal Amount { get; set; }
        public decimal TaxAmount { get; set; }
        public decimal ServiceCharge { get; set; }
        public decimal Discount { get; set; }
        public Category Category { get; set; }
        public TransactionType Type { get; set; }
        public TransactionStatus Status { get; set; }
        public bool IsTrashed { get; set; }
        public string BankAccount { get; set; }
        public DateTime DateAdded { get; set; }
        public string IssuedBy { get; set; }


        public string TransactionId { get; set; }
        public virtual Transaction Transaction { get; set; }
        public virtual ApplicationUser ApplicationUser { get; set; }
    }
}
