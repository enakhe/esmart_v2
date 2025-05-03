#nullable disable

using ESMART.Domain.Entities.Data;

namespace ESMART.Domain.Entities.FrontDesk
{
    public class Guest
    {
        public Guest()
        {
            this.Transactions = new HashSet<Entities.Transaction.Transaction>();
            this.Bookings = new HashSet<Booking>();
        }

        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string GuestId { get; set; }
        public byte[] GuestImage { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string MiddleName { get; set; }
        public string FullName => $"{FirstName} {LastName} {MiddleName}";
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public string Gender { get; set; }

        public string Street { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string Country { get; set; }

        public string Status { get; set; }
        public string ApplicationUserId { get; set; }
        public string UpdatedBy { get; set; }

        public DateTime DateCreated { get; set; } = DateTime.Now;
        public DateTime DateModified { get; set; } = DateTime.Now;
        public bool IsTrashed { get; set; } = false;

        public virtual ApplicationUser ApplicationUser { get; set; }
        public GuestIdentity GuestIdentity { get; set; }
        public ICollection<Entities.Transaction.Transaction> Transactions { get; set; }
        public ICollection<Booking> Bookings { get; set; }
    }
}
