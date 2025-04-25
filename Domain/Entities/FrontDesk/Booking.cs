#nullable disable

using ESMART.Domain.Entities.Data;
using ESMART.Domain.Entities.RoomSettings;
using ESMART.Domain.Entities.Verification;
using ESMART.Domain.Enum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ESMART.Domain.Entities.FrontDesk
{
    public class Booking
    {
        public Booking() 
        {
            this.Codes = new HashSet<VerificationCode>();
        }
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string BookingId { get; set; }
        public DateTime CheckIn { get; set; }
        public DateTime CheckOut { get; set; }
        public string Duration => $"{(CheckOut - CheckIn).Days + 1} {((CheckOut - CheckIn).Days > 1 ? "days" : "day")}";
        public decimal Amount { get; set; }
        public PaymentStatus Status { get; set; }
        public string AccountNumber { get; set; }
        public decimal Discount { get; set; }
        public decimal VAT { get; set; }
        public decimal ServiceCharge { get; set; }
        public decimal TotalAmount { get; set; }
        public PaymentMethod PaymentMethod { get; set; }

        public string GuestId { get; set; }
        public string RoomId { get; set; }
        public string CreatedBy { get; set; }
        public string UpdatedBy { get; set; }

        public DateTime DateCreated { get; set; } = DateTime.Now;
        public DateTime DateModified { get; set; }
        public bool IsTrashed { get; set; } = false;

        public virtual Guest Guest { get; set; }
        public virtual Room Room { get; set; }
        public virtual ApplicationUser  ApplicationUser { get; set; }

        public ICollection<VerificationCode> Codes { get; set; }
    }
}
