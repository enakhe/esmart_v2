#nullable disable

using ESMART.Domain.Entities.Data;
using ESMART.Domain.Entities.RoomSettings;
using ESMART.Domain.Entities.Transaction;
using ESMART.Domain.Entities.Verification;
using ESMART.Domain.Enum;

namespace ESMART.Domain.Entities.FrontDesk
{
    public class Booking
    {
        public Booking()
        {
            this.Codes = new HashSet<VerificationCode>();
            this.Transactions = new HashSet<Entities.Transaction.Transaction>();
            this.TransactionItems = new HashSet<Entities.Transaction.TransactionItem>();
            this.RoomBookings = new HashSet<RoomBooking>();
        }

        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string BookingId { get; set; }
        public DateTime CheckIn { get; set; }
        public DateTime CheckOut { get; set; }
        public string Duration
        {
            get
            {
                var duration = CheckOut.Date - CheckIn.Date;
                if (duration.TotalDays >= 1)
                {
                    int days = (int)duration.TotalDays;
                    return $"{days} {(days > 1 ? "days" : "day")}";
                }
                else if (duration.TotalHours >= 1)
                {
                    int hours = (int)duration.TotalHours;
                    return $"{hours} {(hours > 1 ? "hours" : "hour")}";
                }
                else
                {
                    int minutes = (int)duration.TotalMinutes;
                    return $"{minutes} {(minutes > 1 ? "minutes" : "minute")}";
                }
            }
        }

        public decimal Amount { get; set; }
        public BookingStatus Status { get; set; }
        public string BankAccountId { get; set; }
        public decimal Discount { get; set; }
        public decimal VAT { get; set; }
        public decimal ServiceCharge { get; set; }
        public bool IsOverStay { get; set; } = false;
        public bool IsSettled { get; set; }
        public PaymentMethod PaymentMethod { get; set; }

        public string GuestId { get; set; }
        public string RoomId { get; set; }
        public string GuestAccountId { get; set; }
        public string ApplicationUserId { get; set; }
        public string UpdatedBy { get; set; }

        public DateTime DateCreated { get; set; } = DateTime.Now;
        public DateTime DateModified { get; set; }
        public DateTime? SettledDate { get; set; }
        public bool IsClosed {  get; set; }
        public bool IsTrashed { get; set; } = false;

        public virtual Guest Guest { get; set; }
        public virtual Room Room { get; set; }
        public virtual ApplicationUser ApplicationUser { get; set; }
        public virtual GuestAccount GuestAccount { get; set; }
        public virtual BankAccount BankAccount { get; set; }

        public ICollection<VerificationCode> Codes { get; set; }
        public ICollection<Entities.Transaction.Transaction> Transactions { get; set; }
        public ICollection<Entities.Transaction.TransactionItem> TransactionItems { get; set; }
        public ICollection<RoomBooking> RoomBookings { get; set; }
    }
}
