#nullable disable

using ESMART.Domain.Entities.FrontDesk;
using ESMART.Domain.Entities.RoomSettings;
using ESMART.Domain.Entities.Transaction;
using ESMART.Domain.Entities.Verification;
using Microsoft.AspNetCore.Identity;

namespace ESMART.Domain.Entities.Data
{
    public class ApplicationUser : IdentityUser
    {
        public ApplicationUser()
        {
            this.Guests = new HashSet<Guest>();
            this.Transactions = new HashSet<Domain.Entities.Transaction.Transaction>();
            this.TransactionItems = new HashSet<TransactionItem>();
            this.Rooms = new HashSet<Room>();
            this.Bookings = new HashSet<Booking>();
            this.VerificationCodes = new HashSet<VerificationCode>();
        }

        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string MiddleName { get; set; }
        public string FullName => $"{FirstName} {LastName} {MiddleName}";
        public DateTime DateCreated { get; set; } = DateTime.Now;
        public string RoleId { get; set; }

        public ICollection<Guest> Guests { get; set; }
        public ICollection<Room> Rooms { get; set; }
        public ICollection<Domain.Entities.Transaction.Transaction> Transactions { get; set; }
        public ICollection<TransactionItem> TransactionItems { get; set; }
        public ICollection<Booking> Bookings { get; set; }
        public ICollection<VerificationCode> VerificationCodes { get; set; }

        public virtual ApplicationRole Role { get; set; }
    }
}
