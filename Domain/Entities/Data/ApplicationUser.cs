using ESMART.Domain.Entities;
using Microsoft.AspNetCore.Identity;

namespace ESMART.Domain.Entities
{
    public class ApplicationUser : IdentityUser
    {
        public ApplicationUser()
        {
            Guests = new HashSet<Guest>();
        }

        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string MiddleName { get; set; }
        public string FullName => $"{FirstName} {LastName} {MiddleName}";

        public ICollection<Guest> Guests { get; set; }
    }
}
