#nullable disable

using ESMART.Domain.Entities.Data;
using ESMART.Domain.Entities.FrontDesk;

namespace ESMART.Domain.Entities.Verification
{
    public class VerificationCode
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string Code { get; set; }

        public string BookingId { get; set; }
        public string IssuedBy { get; set; }
        public DateTime IssuedAt { get; set; } = DateTime.Now;
        public DateTime ExpiresAt { get; set; } = DateTime.Now.AddMinutes(20);

        public virtual Booking Booking { get; set; }
        public virtual ApplicationUser ApplicationUser { get; set; }
    }
}