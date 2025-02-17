using ESMART.Domain.Entities.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ESMART.Domain.Entities.FrontDesk
{
    public class Guest
    {
        public Guest()
        {
            Transactions = new HashSet<Entities.Transaction.Transaction>();
        }

        public string? Id { get; set; } = Guid.NewGuid().ToString();
        public string? GuestId { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? MiddleName { get; set; }
        public string? FullName => $"{FirstName} {LastName} {MiddleName}";
        public string? Email { get; set; }
        public string? PhoneNumber { get; set; }
        public string? Gender { get; set; }

        public string? Street { get; set; }
        public string? City { get; set; }
        public string? State { get; set; }
        public string? Country { get; set; }

        public string? Status { get; set; }

        public string? IdNumber { get; set; }
        public string? IdType { get; set; }
        public byte[]? IdentificationDocumentFront { get; set; }
        public byte[]? IdentificationDocumentBack { get; set; }
        public byte[]? GuestImage { get; set; }
        public string? CreatedBy { get; set; }
        public DateTime DateCreated { get; set; }
        public DateTime DateModified { get; set; }
        public bool IsTrashed { get; set; }

        public virtual ApplicationUser? ApplicationUser { get; set; }
        public ICollection<Entities.Transaction.Transaction> Transactions { get; set; }
    }
}
