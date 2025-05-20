#nullable disable

namespace ESMART.Domain.Entities.StoreKeeping
{
    public class InventoryItem
    {
        public InventoryItem()
        {
            MenuItemRecipes = new HashSet<MenuItemRecipe>();
            MenuItems = new HashSet<MenuItem>();
        }

        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string Name { get; set; }
        public string Description { get; set; }
        public decimal Quantity { get; set; }
        public InventoryUnit UnitOfMeasure { get; set; }
        public decimal ReorderLevel { get; set; }
        public decimal ReorderQuantity { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime UpdatedAt { get; set; } = DateTime.Now;

        public virtual ICollection<MenuItemRecipe> MenuItemRecipes { get; set; }
        public virtual ICollection<MenuItem> MenuItems { get; set; }
    }

    public enum InventoryUnit
    {
        Gram,           
        Kilogram,       
        Milligram,     

        Milliliter,     
        Liter,          
        Centiliter,     

        Piece,          
        Bottle,       
        Can,           
        Pack,         
        Carton,        
        Crate,       
        Sachet,         

        Slice,
        Scoop,
        Shot,           
        Cup,
        Bag,
        Drop           
    }

}
