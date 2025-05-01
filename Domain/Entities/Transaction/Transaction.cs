#nullable disable

using ESMART.Domain.Entities.Data;
using ESMART.Domain.Entities.FrontDesk;
using ESMART.Domain.Enum;

namespace ESMART.Domain.Entities.Transaction
{
    public class Transaction
    {
        public Transaction()
        {
            TransactionItems = new HashSet<TransactionItem>();
        }

        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string TransactionId { get; set; }
        public string GuestId { get; set; }
        public string BookingId { get; set; }
        public DateTime Date { get; set; }
        public decimal TotalAmount { get; set; }
        public string InvoiceNumber { get; set; }
        public string Description { get; set; }
        public string CreatedBy { get; set; }
        public bool IsTrashed { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime UpdatedAt { get; set; } = DateTime.Now;

        public required virtual Guest Guest { get; set; }
        public required virtual ApplicationUser ApplicationUser { get; set; }
        public required virtual Booking Booking { get; set; }
        public ICollection<TransactionItem> TransactionItems { get; set; }
    }
}
