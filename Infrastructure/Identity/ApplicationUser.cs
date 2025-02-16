using ESMART.Domain.Entities.FrontDesk;
using Microsoft.AspNetCore.Identity;

namespace ESMART.Infrastructure.Identity
{
    public class ApplicationUser : IdentityUser
    {
        public ApplicationUser()
        {
            Guests = new HashSet<Guest>();
        }

        public ICollection<Guest> Guests { get; set; }
    }
}
