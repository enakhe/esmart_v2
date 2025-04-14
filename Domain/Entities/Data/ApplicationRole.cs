#nullable disable

using Microsoft.AspNetCore.Identity;

namespace ESMART.Domain.Entities.Data
{
    public class ApplicationRole : IdentityRole<string>
    {
        public ApplicationRole()
        {
            this.JoinEntities = new HashSet<ApplicationUserRole>();
            this.JoinEntitiesCategory = new HashSet<ApplicationRoleCategory>();
        }

        public int NoOfUser { get; set; }
        public byte[] Icon { get; set; }
        public string Description { get; set; }
        public string Tag { get; set; }
        public bool Status { get; set; }
        public string ManagerId { get; set; }
        public DateTime DateCreated { get; set; } = DateTime.Now;
        public DateTime ExpirationDate { get; set; }

        public virtual ICollection<ApplicationUserRole> JoinEntities { get; set; }
        public virtual ICollection<ApplicationRoleCategory> JoinEntitiesCategory { get; set; }

        public virtual ApplicationUser Manager { get; set; }


    }
}
