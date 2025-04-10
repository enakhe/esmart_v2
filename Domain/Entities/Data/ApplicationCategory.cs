#nullable disable

namespace ESMART.Domain.Entities.Data
{
    public class ApplicationCategory
    {
        public ApplicationCategory()
        {
            this.JoinEntities = new HashSet<ApplicationRoleCategory>();
        }
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string Name { get; set; }

        public virtual ICollection<ApplicationRoleCategory> JoinEntities { get; set; }
    }
}
