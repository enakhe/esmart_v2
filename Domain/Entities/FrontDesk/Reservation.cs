#nullable disable

using ESMART.Domain.Entities.Data;
using ESMART.Domain.Entities.RoomSettings;
using ESMART.Domain.Enum;

namespace ESMART.Domain.Entities.FrontDesk
{
    public class Reservation
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string ReservationId { get; set; }
        public string GuestId { get; set; }
        public string RoomId { get; set; }
        public string ApplicationUserId { get; set; }
        public DateTime ArrivateDate { get; set; }
        public DateTime DepartureDate { get; set; }
        public string Duration
        {
            get
            {
                var duration = DepartureDate.Date - ArrivateDate.Date;
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
        public string AccountNumber { get; set; }
        public decimal Discount { get; set; }
        public decimal VAT { get; set; }
        public decimal ServiceCharge { get; set; }
        public decimal TotalAmount { get; set; }
        public PaymentMethod PaymentMethod { get; set; }
        public ReservationStatus Status { get; set; }
        public TransactionStatus TransactionStatus { get; set; }
        public DateTime DateAdded { get; set; } = DateTime.Now;
        public DateTime DateModified { get; set; } = DateTime.Now;
        public bool IsTrashed { get; set; } = false;

        public ApplicationUser ApplicationUser { get; set; }
        public virtual Guest Guest { get; set; }
        public virtual Room Room { get; set; }
    }

    public enum ReservationStatus
    {
        Reserved, Cancelled, Converted
    }
}
