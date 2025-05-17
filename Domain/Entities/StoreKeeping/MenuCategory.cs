#nullable disable

namespace ESMART.Domain.Entities.StoreKeeping
{
    public class MenuCategory
    {
        public MenuCategory()
        {
            MenuItems = new HashSet<MenuItem>();
        }

        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string Name { get; set; }
        public string Description { get; set; }
        public bool IsActive { get; set; }
        public ServiceArea ServiceArea { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public ICollection<MenuItem> MenuItems { get; set; }
    }
}
