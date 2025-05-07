#nullable disable

using Microsoft.AspNetCore.Identity;

namespace ESMART.Domain.Entities.Data
{
    public class ApplicationRole : IdentityRole<string>
    {
        public ApplicationRole()
        {
            this.JoinEntities = new HashSet<ApplicationUserRole>();
        }

        public int NoOfUser { get; set; } = 0;
        public string Description { get; set; }
        public DateTime DateCreated { get; set; } = DateTime.Now;

        public virtual ICollection<ApplicationUserRole> JoinEntities { get; set; }
    }
}
