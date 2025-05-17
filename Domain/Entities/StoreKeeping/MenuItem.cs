#nullable disable

namespace ESMART.Domain.Entities.StoreKeeping
{
    public class MenuItem
    {
        public MenuItem()
        {
            MenuItemRecipes = new HashSet<MenuItemRecipe>();
        }

        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string Name { get; set; }
        public string Description { get; set; }
        public decimal Price { get; set; }
        public bool IsAvailable { get; set; }
        public ServiceArea ServiceArea { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime UpdatedAt { get; set; }

        public string MenuCategoryId { get; set; }

        public virtual MenuCategory MenuCategory { get; set; }
        public virtual ICollection<MenuItemRecipe> MenuItemRecipes { get; set; }
    }

    public enum ServiceArea
    {
        Bar,
        Restaurant,
        Shared
    }
}
