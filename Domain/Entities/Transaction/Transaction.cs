#nullable disable

using ESMART.Domain.Entities.Data;
using ESMART.Domain.Entities.FrontDesk;
using System.ComponentModel.DataAnnotations.Schema;

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

        [ForeignKey("Booking")]
        public string BookingId { get; set; }
        public DateTime Date { get; set; }
        public decimal TotalRevenue { get; set; }
        public decimal TotalReceivables { get; set; }
        public string InvoiceNumber { get; set; }
        public string Description { get; set; }
        public string ApplicationUserId { get; set; }
        public bool IsTrashed { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime UpdatedAt { get; set; } = DateTime.Now;

        public virtual Guest Guest { get; set; }
        public virtual ApplicationUser ApplicationUser { get; set; }
        public virtual Booking Booking { get; set; }
        public ICollection<TransactionItem> TransactionItems { get; set; }
    }
}
