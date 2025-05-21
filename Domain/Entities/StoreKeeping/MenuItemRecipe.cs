#nullable disable

namespace ESMART.Domain.Entities.StoreKeeping
{
    public class MenuItemRecipe
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string MenuItemId { get; set; }
        public string InventoryItemId { get; set; }
        public decimal Quantity { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime UpdatedAt { get; set; }

        public virtual MenuItem MenuItem { get; set; }
        public virtual InventoryItem InventoryItem { get; set; }
    }
}
