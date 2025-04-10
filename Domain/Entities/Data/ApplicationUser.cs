using ESMART.Domain.Entities.FrontDesk;
using Microsoft.AspNetCore.Identity;

namespace ESMART.Domain.Entities.Data
{
    public class ApplicationUser : IdentityUser
    {
        public ApplicationUser()
        {
            Guests = new HashSet<Guest>();
            Transactions = new HashSet<Domain.Entities.Transaction.Transaction>();
        }

        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? MiddleName { get; set; }
        public string? FullName => $"{FirstName} {LastName} {MiddleName}";

        public ICollection<Guest> Guests { get; set; }
        public ICollection<Domain.Entities.Transaction.Transaction> Transactions { get; set; }
    }
}
