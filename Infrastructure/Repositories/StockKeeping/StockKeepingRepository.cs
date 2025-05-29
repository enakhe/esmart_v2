using ESMART.Application.Common.Interface;
using ESMART.Domain.Entities.StoreKeeping;
using ESMART.Domain.ViewModels.StoreKepping;
using ESMART.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ESMART.Infrastructure.Repositories.StockKeeping
{
    public class StockKeepingRepository(IDbContextFactory<ApplicationDbContext> contextFactory) : IStockKeepingRepository
    {
        private readonly IDbContextFactory<ApplicationDbContext> _contextFactory = contextFactory;

        public async Task AddMenuItemAsync(MenuItem menuItem)
        {
            try
            {
                await using var context = await _contextFactory.CreateDbContextAsync();
                await context.MenuItems.AddAsync(menuItem);
                await context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                throw new Exception("An error occurred while adding the menu item.", ex);
            }
        }

        public async Task<List<MenuItemViewModel>> GetMenuItemsAsync()
        {
            try
            {
                await using var context = await _contextFactory.CreateDbContextAsync();

                var categories = await context.MenuCategories.ToListAsync();

                var menuItems = await context.MenuItems
                    .Include(m => m.MenuItemRecipes)
                    .OrderBy(m => m.Name)
                    .ToListAsync();

                return [.. menuItems.Select(m => new MenuItemViewModel
                {
                    Id = m.Id,
                    Name = m.Name,
                    Description = m.Description,
                    Price = m.Price,
                    IsAvailable = m.IsAvailable ? "Yes" : "No",
                    CategoryId = m.MenuCategoryId,
                    ServiceArea = m.ServiceArea.ToString(),
                    CreatedAt = m.CreatedAt,
                    UpdatedAt = m.UpdatedAt, // Example condition for low stock

                    Categories = [.. categories],
                    Recipes = [.. m.MenuItemRecipes.Select(r => new MenuItemRecipe
                        {
                            Id = r.Id,
                            MenuItemId = r.MenuItemId,
                            InventoryItemId = r.InventoryItemId,
                            CreatedAt = r.CreatedAt,
                            UpdatedAt = r.UpdatedAt,
                            Quantity = r.Quantity,
                        })]
                })];
            }
            catch (Exception ex)
            {
                throw new Exception("An error occurred while retrieving menu items.", ex);
            }
        }

        public async Task<MenuItem?> GetMenuItemByIdAsync(string id)
        {
            try
            {
                await using var context = await _contextFactory.CreateDbContextAsync();
                var menuItem = await context.MenuItems
                    .Include(m => m.MenuItemRecipes)
                    .Include(m => m.MenuCategory)
                    .FirstOrDefaultAsync(m => m.Id == id);

                return menuItem;
            }
            catch (Exception ex)
            {
                throw new Exception("An error occurred while retrieving the menu item.", ex);
            }
        }

        // Update the menu item
        public async Task UpdateMenuItemAsync(MenuItem menuItem)
        {
            try
            {
                await using var context = await _contextFactory.CreateDbContextAsync();

                context.MenuItems.Update(menuItem);
                await context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                throw new Exception("An error occurred while updating the menu item.", ex);
            }
        }

        // Delete the menu item
        public async Task DeleteMenuItemAsync(string id)
        {
            try
            {
                await using var context = await _contextFactory.CreateDbContextAsync();
                var menuItem = await context.MenuItems.FindAsync(id) ?? throw new Exception("Menu item not found.");
                context.MenuItems.Remove(menuItem);
                await context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                throw new Exception("An error occurred while deleting the menu item.", ex);
            }
        }

        // Toggle the availability of the menu item
        public async Task ToggleMenuItemAvailabilityAsync(string id)
        {
            try
            {
                await using var context = await _contextFactory.CreateDbContextAsync();

                var menuItem = await context.MenuItems.FindAsync(id) ?? throw new Exception("Menu item not found.");

                menuItem.IsAvailable = !menuItem.IsAvailable;
                menuItem.UpdatedAt = DateTime.UtcNow;

                context.MenuItems.Update(menuItem);
                await context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                throw new Exception("An error occurred while toggling the menu item's availability.", ex);
            }
        }

        // Add a recipe to the menu item
        public async Task AddRecipeToMenuItemAsync(string menuItemId, MenuItemRecipe recipe)
        {
            try
            {
                await using var context = await _contextFactory.CreateDbContextAsync();
                var menuItem = await context.MenuItems.FindAsync(menuItemId) ?? throw new Exception("Menu item not found.");

                recipe.MenuItemId = menuItem.Id;
                recipe.CreatedAt = DateTime.UtcNow;

                await context.MenuItemRecipes.AddAsync(recipe);
                await context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                throw new Exception("An error occurred while adding the recipe to the menu item.", ex);
            }
        }

        // Remove a recipe from the menu item
        public async Task RemoveRecipeFromMenuItemAsync(string recipeId)
        {

            try
            {
                await using var context = await _contextFactory.CreateDbContextAsync();
                var recipe = await context.MenuItemRecipes.FindAsync(recipeId) ?? throw new Exception("Recipe not found.");

                context.MenuItemRecipes.Remove(recipe);
                await context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                throw new Exception("An error occurred while removing the recipe from the menu item.", ex);
            }
        }

        // Search menu item
        public async Task<List<MenuItemViewModel>> SearchMenuItemsAsync(string searchTerm)
        {
            try
            {
                await using var context = await _contextFactory.CreateDbContextAsync();
                var menuItems = await context.MenuItems
                    .Where(m => m.Name.Contains(searchTerm) || m.Description.Contains(searchTerm))
                    .Include(m => m.MenuItemRecipes)
                    .ToListAsync();
                return [.. menuItems.Select(m => new MenuItemViewModel
                {
                    Id = m.Id,
                    Name = m.Name,
                    Description = m.Description,
                    Price = m.Price,
                    IsAvailable = m.IsAvailable ? "Yes" : "No",
                    CategoryId = m.MenuCategoryId,
                    ServiceArea = m.ServiceArea.ToString(),
                    CreatedAt = m.CreatedAt,
                    UpdatedAt = m.UpdatedAt,
                    Recipes = [.. m.MenuItemRecipes.Select(r => new MenuItemRecipe
                        {
                            Id = r.Id,
                            MenuItemId = r.MenuItemId,
                            InventoryItemId = r.InventoryItemId,
                            CreatedAt = r.CreatedAt,
                            UpdatedAt = r.UpdatedAt,
                            Quantity = r.Quantity,
                        })]
                })];
            }
            catch (Exception ex)
            {
                throw new Exception("An error occurred while searching for menu items.", ex);
            }
        }

        // Get menu item byt category id
        public async Task<List<MenuItemViewModel>> GetMenuItemsByCategoryIdAsync(string categoryId)
        {
            try
            {
                await using var context = await _contextFactory.CreateDbContextAsync();
                var menuItems = await context.MenuItems
                    .Where(m => m.MenuCategoryId == categoryId)
                    .Include(m => m.MenuItemRecipes)
                    .ToListAsync();
                return [.. menuItems.Select(m => new MenuItemViewModel
                {
                    Id = m.Id,
                    Name = m.Name,
                    Description = m.Description,
                    Price = m.Price,
                    IsAvailable = m.IsAvailable ? "Yes" : "No",
                    CategoryId = m.MenuCategoryId,
                    ServiceArea = m.ServiceArea.ToString(),
                    CreatedAt = m.CreatedAt,
                    UpdatedAt = m.UpdatedAt,
                    Recipes = [.. m.MenuItemRecipes.Select(r => new MenuItemRecipe
                        {
                            Id = r.Id,
                            MenuItemId = r.MenuItemId,
                            InventoryItemId = r.InventoryItemId,
                            CreatedAt = r.CreatedAt,
                            UpdatedAt = r.UpdatedAt,
                            Quantity = r.Quantity,
                        })]
                })];
            }
            catch (Exception ex)
            {
                throw new Exception("An error occurred while retrieving menu items by category ID.", ex);
            }
        }

        // Fix for the method GetGroupedMenuItemsAsync to resolve CS0029 error
        public async Task<List<MenuCategoryGroup>> GetGroupedMenuItemsAsync(string category)
        {
            try
            {
                await using var context = await _contextFactory.CreateDbContextAsync();

                var grouped = await context.MenuItems
                    .Include(m => m.MenuCategory)
                    .Include(m => m.MenuItemRecipes)
                    .Where(i => i.MenuCategory.Name == category && i.IsAvailable )
                    .GroupBy(m => m.MenuCategory)
                    .Select(g => new MenuCategoryGroup
                    {
                        CategoryName = g.Key.Name,
                        Image = g.Key.Image,
                        Items = g.Select(m => new MenuItemViewModel
                        {
                            Id = m.Id,
                            Name = m.Name,
                            Description = m.Description,
                            Price = m.Price,
                            Image = m.Image,
                            IsAvailable = m.IsAvailable ? "Yes" : "No",
                            CategoryId = m.MenuCategoryId,
                            ServiceArea = m.ServiceArea.ToString(),
                            CreatedAt = m.CreatedAt,
                            UpdatedAt = m.UpdatedAt,
                        }).ToList()
                    })

                    .ToListAsync();

                return grouped;
            }
            catch (Exception ex)
            {
                throw new Exception("An error occurred while retrieving grouped menu items.", ex);
            }
        }

        // get menu category by service area
        public async Task<List<MenuCategory>> GetMenuCategoriesByServiceAreaAsync(ServiceArea serviceArea)
        {
            try
            {
                await using var context = await _contextFactory.CreateDbContextAsync();
                return await context.MenuCategories
                    .Where(c => c.ServiceArea == serviceArea || c.ServiceArea == ServiceArea.Shared)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                throw new Exception("An error occurred while retrieving menu categories by service area.", ex);
            }
        }

        //Add menu item category
        public async Task AddMenuItemCategoryAsync(MenuCategory menuCategory)
        {
            try
            {
                await using var context = await _contextFactory.CreateDbContextAsync();
                await context.MenuCategories.AddAsync(menuCategory);
                await context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                throw new Exception("An error occurred while adding the menu item category.", ex);
            }
        }

        //Get menu item category
        public async Task<List<MenuCategory>> GetMenuItemCategoriesAsync()
        {
            try
            {
                await using var context = await _contextFactory.CreateDbContextAsync();
                return await context.MenuCategories.ToListAsync();
            }
            catch (Exception ex)
            {
                throw new Exception("An error occurred while retrieving menu item categories.", ex);
            }
        }

        //Get menu item category by id
        public async Task<MenuCategory> GetMenuItemCategoryByIdAsync(string id)
        {
            try
            {
                await using var context = await _contextFactory.CreateDbContextAsync();
                return await context.MenuCategories.FindAsync(id) ?? throw new Exception("Menu item category not found.");
            }
            catch (Exception ex)
            {
                throw new Exception("An error occurred while retrieving the menu item category.", ex);
            }
        }

        //Update menu item category
        public async Task UpdateMenuItemCategoryAsync(MenuCategory menuCategory)
        {
            try
            {
                await using var context = await _contextFactory.CreateDbContextAsync();
                context.MenuCategories.Update(menuCategory);
                await context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                throw new Exception("An error occurred while updating the menu item category.", ex);
            }
        }

        //Delete menu item category
        public async Task DeleteMenuItemCategoryAsync(string id)
        {
            try
            {
                await using var context = await _contextFactory.CreateDbContextAsync();
                var menuCategory = await context.MenuCategories.FindAsync(id) ?? throw new Exception("Menu item category not found.");
                context.MenuCategories.Remove(menuCategory);
                await context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                throw new Exception("An error occurred while deleting the menu item category.", ex);
            }
        }

        // search inventory items
        public async Task<List<InventoryViewModel>> SearchInventoryItemsAsync(string searchTerm)
        {
            try
            {
                await using var context = await _contextFactory.CreateDbContextAsync();
                return await context.InventoryItems
                    .Where(i => i.Name.Contains(searchTerm) || i.Description.Contains(searchTerm))
                    .Select(i => new InventoryViewModel
                    {
                        Id = i.Id,
                        Name = i.Name,
                        Description = i.Description,
                        Quantity = i.Quantity,
                        UnitOfMeasure = i.UnitOfMeasure,
                        ReorderLevel = i.ReorderLevel,
                        ReorderQuantity = i.ReorderQuantity,
                        IsLow = i.Quantity < i.ReorderQuantity,
                        IsActive = i.IsActive ? "Yes" : "No",
                        CreatedAt = i.CreatedAt,
                        UpdatedAt = i.UpdatedAt
                    })
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                throw new Exception("An error occurred while searching for inventory items.", ex);
            }
        }

        //Get menu item category by name
        public async Task<MenuCategory> GetMenuItemCategoryByNameAsync(string name)
        {
            try
            {
                await using var context = await _contextFactory.CreateDbContextAsync();
                return await context.MenuCategories.FirstOrDefaultAsync(m => m.Name == name) ?? throw new Exception("Menu item category not found.");
            }
            catch (Exception ex)
            {
                throw new Exception("An error occurred while retrieving the menu item category by name.", ex);
            }
        }

        // Add Inventory item to database
        public async Task AddInventoryItemAsync(InventoryItem inventoryItem)
        {
            try
            {
                await using var context = await _contextFactory.CreateDbContextAsync();
                await context.InventoryItems.AddAsync(inventoryItem);
                await context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                throw new Exception("An error occurred while adding the inventory item.", ex);
            }
        }

        // Get Inventory item by id
        public async Task<InventoryItem> GetInventoryItemByIdAsync(string id)
        {
            try
            {
                await using var context = await _contextFactory.CreateDbContextAsync();
                return await context.InventoryItems.FindAsync(id) ?? throw new Exception("Inventory item not found.");
            }
            catch (Exception ex)
            {
                throw new Exception("An error occurred while retrieving the inventory item.", ex);
            }
        }

        // Get Inventory item by name
        public async Task<InventoryItem> GetInventoryItemByNameAsync(string name)
        {
            try
            {
                await using var context = await _contextFactory.CreateDbContextAsync();
                return await context.InventoryItems.FirstOrDefaultAsync(i => i.Name == name) ?? throw new Exception("Inventory item not found.");
            }
            catch (Exception ex)
            {
                throw new Exception("An error occurred while retrieving the inventory item by name.", ex);
            }
        }

        // Get all Inventory items
        public async Task<List<InventoryViewModel>> GetAllInventoryItemsAsync()
        {
            try
            {
                await using var context = await _contextFactory.CreateDbContextAsync();
                return await context.InventoryItems
                    .Select(i => new InventoryViewModel
                    {
                        Id = i.Id,
                        Name = i.Name,
                        Description = i.Description,
                        Quantity = i.Quantity,
                        UnitOfMeasure = i.UnitOfMeasure,
                        ReorderLevel = i.ReorderLevel,
                        ReorderQuantity = i.ReorderQuantity,
                        IsLow = i.Quantity < i.ReorderQuantity,
                        IsActive = i.IsActive ? "Yes" : "No",
                        CreatedAt = i.CreatedAt,
                        UpdatedAt = i.UpdatedAt
                    })
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                throw new Exception("An error occurred while retrieving all inventory items.", ex);
            }
        }

        // Update Inventory item
        public async Task UpdateInventoryItemAsync(InventoryItem inventoryItem)
        {
            try
            {
                await using var context = await _contextFactory.CreateDbContextAsync();
                context.InventoryItems.Update(inventoryItem);
                await context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                throw new Exception("An error occurred while updating the inventory item.", ex);
            }
        }

        // Delete Inventory item
        public async Task DeleteInventoryItemAsync(string id)
        {
            try
            {
                await using var context = await _contextFactory.CreateDbContextAsync();
                var inventoryItem = await context.InventoryItems.FindAsync(id) ?? throw new Exception("Inventory item not found.");
                context.InventoryItems.Remove(inventoryItem);
                await context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                throw new Exception("An error occurred while deleting the inventory item.", ex);
            }
        }

        // Toggle Inventory item availability
        public async Task ToggleInventoryItemAvailabilityAsync(string id)
        {
            try
            {
                await using var context = await _contextFactory.CreateDbContextAsync();
                var inventoryItem = await context.InventoryItems.FindAsync(id) ?? throw new Exception("Inventory item not found.");
                inventoryItem.IsActive = !inventoryItem.IsActive;
                inventoryItem.UpdatedAt = DateTime.UtcNow;
                context.InventoryItems.Update(inventoryItem);
                await context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                throw new Exception("An error occurred while toggling the inventory item's availability.", ex);
            }
        }

        // Add menu item recipe to database
        public async Task AddMenuItemRecipeAsync(MenuItemRecipe menuItemRecipe)
        {
            try
            {
                await using var context = await _contextFactory.CreateDbContextAsync();
                await context.MenuItemRecipes.AddAsync(menuItemRecipe);
                await context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                throw new Exception("An error occurred while adding the menu item recipe.", ex);
            }
        }

        // Get menu item recipe by id
        public async Task<MenuItemRecipe> GetMenuItemRecipeByIdAsync(string id)
        {
            try
            {
                await using var context = await _contextFactory.CreateDbContextAsync();
                return await context.MenuItemRecipes.FindAsync(id) ?? throw new Exception("Menu item recipe not found.");
            }
            catch (Exception ex)
            {
                throw new Exception("An error occurred while retrieving the menu item recipe.", ex);
            }
        }

        // Get all menu item recipes
        public async Task<List<MenuItemRecipe>> GetAllMenuItemRecipesAsync()
        {
            try
            {
                await using var context = await _contextFactory.CreateDbContextAsync();
                return await context.MenuItemRecipes.ToListAsync();
            }
            catch (Exception ex)
            {
                throw new Exception("An error occurred while retrieving all menu item recipes.", ex);
            }
        }

        // Update menu item recipe
        public async Task UpdateMenuItemRecipeAsync(MenuItemRecipe menuItemRecipe)
        {
            try
            {
                await using var context = await _contextFactory.CreateDbContextAsync();
                context.MenuItemRecipes.Update(menuItemRecipe);
                await context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                throw new Exception("An error occurred while updating the menu item recipe.", ex);
            }
        }

        // Delete menu item recipe
        public async Task DeleteMenuItemRecipeAsync(string id)
        {
            try
            {
                await using var context = await _contextFactory.CreateDbContextAsync();
                var menuItemRecipe = await context.MenuItemRecipes.FindAsync(id) ?? throw new Exception("Menu item recipe not found.");
                context.MenuItemRecipes.Remove(menuItemRecipe);
                await context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                throw new Exception("An error occurred while deleting the menu item recipe.", ex);
            }
        }

        // Get menu item recipe by menu item id
        public async Task<List<MenuItemRecipe>> GetMenuItemRecipesByMenuItemIdAsync(string menuItemId)
        {
            try
            {
                await using var context = await _contextFactory.CreateDbContextAsync();
                return await context.MenuItemRecipes
                    .Where(m => m.MenuItemId == menuItemId)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                throw new Exception("An error occurred while retrieving menu item recipes by menu item ID.", ex);
            }
        }

        // Get menu item recipe by inventory item id
        public async Task<List<MenuItemRecipe>> GetMenuItemRecipesByInventoryItemIdAsync(string inventoryItemId)
        {
            try
            {
                await using var context = await _contextFactory.CreateDbContextAsync();
                return await context.MenuItemRecipes
                    .Where(m => m.InventoryItemId == inventoryItemId)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                throw new Exception("An error occurred while retrieving menu item recipes by inventory item ID.", ex);
            }
        }

        // Get menu item recipe by menu item and inventory item id
        public async Task<MenuItemRecipe> GetMenuItemRecipeByMenuItemAndInventoryItemIdAsync(string menuItemId, string inventoryItemId)
        {
            try
            {
                await using var context = await _contextFactory.CreateDbContextAsync();
                return await context.MenuItemRecipes
                    .FirstOrDefaultAsync(m => m.MenuItemId == menuItemId && m.InventoryItemId == inventoryItemId) ?? throw new Exception("Menu item recipe not found.");
            }
            catch (Exception ex)
            {
                throw new Exception("An error occurred while retrieving the menu item recipe by menu item and inventory item ID.", ex);
            }
        }

        // Get all menu item categories
        public async Task<List<MenuCategory>> GetAllMenuItemCategoriesAsync()
        {
            try
            {
                await using var context = await _contextFactory.CreateDbContextAsync();
                return await context.MenuCategories.ToListAsync();
            }
            catch (Exception ex)
            {
                throw new Exception("An error occurred while retrieving all menu item categories.", ex);
            }
        }

        // get all meny item categories view model
        public async Task<List<MenuCategoryViewModel>> GetAllMenuItemCategoriesViewModelAsync()
        {
            try
            {
                await using var context = await _contextFactory.CreateDbContextAsync();
                return await context.MenuCategories
                    .Select(c => new MenuCategoryViewModel
                    {
                        Id = c.Id,
                        Name = c.Name,
                        Description = c.Description,
                        IsAvailable = c.IsActive ? "Yes" : "No",
                        ServiceArea = c.ServiceArea.ToString(),
                        CreatedAt = c.CreatedAt,
                    })
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                throw new Exception("An error occurred while retrieving all menu item categories view model.", ex);
            }
        }

        // Add a order to database
        public async Task AddOrderAsync(Order order)
        {
            try
            {
                await using var context = await _contextFactory.CreateDbContextAsync();
                await context.Orders.AddAsync(order);
                await context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                throw new Exception("An error occurred while adding the menu order.", ex);
            }
        }

        // Delete order
        public async Task DeleteOrderAsync(string id)
        {
            try
            {
                await using var context = await _contextFactory.CreateDbContextAsync();
                var order = await context.Orders.FindAsync(id) ?? throw new Exception("Order not found.");
                context.Orders.Remove(order);
                await context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                throw new Exception("An error occurred while deleting the order.", ex);
            }
        }

        // Update order
        public async Task UpdateOrderAsync(Order order)
        {
            try
            {
                await using var context = await _contextFactory.CreateDbContextAsync();
                context.Orders.Update(order);
                await context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                throw new Exception("An error occurred while updating the order.", ex);
            }
        }

        // Get order by id
        public async Task<Order?> GetOrderByIdAsync(string id)
        {
            try
            {
                await using var context = await _contextFactory.CreateDbContextAsync();
                return await context.Orders
                    .Include(o => o.OrderItems)
                    .FirstOrDefaultAsync(o => o.Id == id);
            }
            catch (Exception ex)
            {
                throw new Exception("An error occurred while retrieving the order.", ex);
            }
        }

        // Get all orders
        public async Task<List<MenuOrderViewModel>> GetAllOrdersAsync()
        {
            try
            {
                await using var context = await _contextFactory.CreateDbContextAsync();
                return await context.Orders
                    .Include(o => o.OrderItems)
                    .Include(o => o.Booking)
                    .Include(o => o.Booking.Guest)
                    .Include(o => o.Booking.Room)
                    .Select(o => new MenuOrderViewModel
                    {
                        Id = o.Id,
                        BookingId = o.BookingId,
                        Guest = o.Booking.Guest.FullName,
                        Room = o.Booking.Room.Number,
                        TotalAmount = o.OrderItems.Sum(oi => oi.UnitPrice * oi.Quantity).ToString("N2"),
                        Quantity = o.OrderItems.Sum(oi => oi.Quantity).ToString(),
                        OrderItems = o.OrderItems.Select(oi => new OrderItemViewModel
                        {
                            OrderId = oi.OrderId,
                            Item = oi.MenuItem.Name,
                            Quantity = oi.Quantity,
                            UnitPrice = oi.UnitPrice,
                        }).ToList(),
                        CreatedAt = o.CreatedAt
                    })
                    .OrderByDescending(o => o.CreatedAt)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                throw new Exception("An error occurred while retrieving all orders.", ex);
            }
        }

        // Get orders by booking id
        public async Task<List<Order>> GetOrdersByBookingIdAsync(string bookingId)
        {
            try
            {
                await using var context = await _contextFactory.CreateDbContextAsync();
                return await context.Orders
                    .Where(o => o.BookingId == bookingId)
                    .Include(o => o.OrderItems)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                throw new Exception("An error occurred while retrieving orders by booking ID.", ex);
            }
        }

        // get orders by search
        public async Task<List<MenuOrderViewModel>> GetOrdersBySearchAsync(string searchTerm)
        {
            try
            {
                await using var context = await _contextFactory.CreateDbContextAsync();
                return await context.Orders
                    .Include(o => o.OrderItems)
                    .Include(o => o.Booking)
                    .Include(o => o.Booking.Guest)
                    .Include(o => o.Booking.Room)
                    .Where(o => (o.Booking.Guest.FirstName+o.Booking.Guest.LastName+o.Booking.Guest.MiddleName).Contains(searchTerm) ||
                                o.Booking.Room.Number.Contains(searchTerm) ||
                                o.OrderItems.Any(oi => oi.MenuItem.Name.Contains(searchTerm)))
                    .Select(o => new MenuOrderViewModel
                    {
                        Id = o.Id,
                        BookingId = o.BookingId,
                        Guest = o.Booking.Guest.FullName,
                        Room = o.Booking.Room.Number,
                        TotalAmount = o.OrderItems.Sum(oi => oi.UnitPrice * oi.Quantity).ToString("N2"),
                        Quantity = o.OrderItems.Sum(oi => oi.Quantity).ToString(),
                        OrderItems = o.OrderItems.Select(oi => new OrderItemViewModel
                        {
                            OrderId = oi.OrderId,
                            Item = oi.MenuItem.Name,
                            Quantity = oi.Quantity,
                            UnitPrice = oi.UnitPrice,
                        }).ToList(),
                        CreatedAt = o.CreatedAt
                    })
                    .OrderByDescending(o => o.CreatedAt)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                throw new Exception("An error occurred while searching for orders.", ex);
            }
        }

        // filter our order bt from and to date
        public async Task<List<MenuOrderViewModel>> GetOrdersByDateRangeAsync(DateTime fromDate, DateTime toDate)
        {
            try
            {
                await using var context = await _contextFactory.CreateDbContextAsync();
                return await context.Orders
                    .Include(o => o.OrderItems)
                    .Include(o => o.RoomBooking.Booking)
                    .Include(o => o.RoomBooking.Booking.Guest)
                    .Include(o => o.RoomBooking.Room)
                    .Where(o => o.CreatedAt >= fromDate && o.CreatedAt <= toDate)
                    .Select(o => new MenuOrderViewModel
                    {
                        Id = o.Id,
                        BookingId = o.BookingId,
                        Guest = o.RoomBooking.Booking.Guest.FullName,
                        Room = o.RoomBooking.Room.Number,
                        Invoice = o.Invoice,
                        TotalAmount = o.OrderItems.Sum(oi => oi.UnitPrice * oi.Quantity).ToString("N2"),
                        Quantity = o.OrderItems.Sum(oi => oi.Quantity).ToString(),
                        OrderItems = o.OrderItems.Select(oi => new OrderItemViewModel
                        {
                            OrderId = oi.OrderItemId,
                            Item = oi.MenuItem.Name,
                            Quantity = oi.Quantity,
                            UnitPrice = oi.UnitPrice,
                        }).ToList(),
                        CreatedAt = o.CreatedAt
                    })
                    .OrderByDescending(o => o.CreatedAt)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                throw new Exception("An error occurred while retrieving orders by date range.", ex);
            }
        }


        // Seed default menuitem categories to the database
        public async Task SeedDefaultMenuItemCategoriesAsync()
        {
            try
            {
                await using var context = await _contextFactory.CreateDbContextAsync();
                if (!await context.MenuCategories.AnyAsync())
                {
                    var defaultCategories = new List<MenuCategory>
                    {
                        new() { 
                            Name = "Appetizers",
                            Description = "Starters to whet your appetite.",
                            IsActive = true,
                            ServiceArea = ServiceArea.Restaurant,
                        },

                        new() {
                            Name = "Main Courses",
                            Description = "Hearty meals to satisfy your hunger.",
                            IsActive = true,
                            ServiceArea = ServiceArea.Restaurant,
                        },

                        new() {
                            Name = "Breakfast",
                            Description = "Hearty meals to satisfy your morning.",
                            IsActive = true,
                            ServiceArea = ServiceArea.Restaurant,
                        },

                        new() {
                            Name = "Main Dish",
                            Description = "",
                            IsActive = true,
                            ServiceArea = ServiceArea.Restaurant,
                        },

                        new() {
                            Name = "Salads",
                            Description = "Fresh and healthy salads.",
                            IsActive = true,
                            ServiceArea = ServiceArea.Restaurant,
                        },

                        new() {
                            Name = "Desserts",
                            Description = "Sweet treats to end your meal.",
                            IsActive = true,
                            ServiceArea = ServiceArea.Restaurant,
                        },

                        new() {
                            Name = "Drinks",
                            Description = "Refreshing beverages to quench your thirst.",
                            IsActive = true,
                            ServiceArea = ServiceArea.Shared,
                        },

                        new() {
                            Name = "Beverages",
                            Description = "A variety of drinks to enjoy.",
                            IsActive = true,
                            ServiceArea = ServiceArea.Shared,
                        },

                        new() {
                            Name = "Soups",
                            Description = "Warm and comforting soups.",
                            IsActive = true,
                            ServiceArea = ServiceArea.Restaurant,
                        },  
                    };
                    //await context.MenuCategories.AddRangeAsync(defaultCategories);
                    //await context.SaveChangesAsync();
                }
            }
            catch (Exception ex)
            {
                throw new Exception("An error occurred while seeding default menu item categories.", ex);
            }
        }
    }
}