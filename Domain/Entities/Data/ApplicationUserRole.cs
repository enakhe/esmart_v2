#nullable disable

using Microsoft.AspNetCore.Identity;

namespace ESMART.Domain.Entities.Data
{
    public class ApplicationUserRole : IdentityUserRole<string>
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public DateTime DateAssigned { get; set; } = DateTime.Now;
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public bool isActive { get; set; } = false;

        public virtual ApplicationUser ApplicationUser { get; set; }
        public virtual ApplicationRole ApplicationRole { get; set; }
    }
}
