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
                    UpdatedAt = m.UpdatedAt,
                    IsLow = m.MenuItemRecipes.Any(r => r.InventoryItem.Quantity < r.InventoryItem.ReorderQuantity), // Example condition for low stock

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

        public async Task<MenuItemViewModel> GetMenuItemByIdAsync(string id)
        {
            try
            {
                await using var context = await _contextFactory.CreateDbContextAsync();
                var menuItem = await context.MenuItems
                    .Include(m => m.MenuItemRecipes)
                    .FirstOrDefaultAsync(m => m.Id == id);

                return menuItem == null
                    ? throw new Exception("Menu item not found.")
                    : new MenuItemViewModel
                    {
                        Id = menuItem.Id,
                        Name = menuItem.Name,
                        Description = menuItem.Description,
                        Price = menuItem.Price,
                        IsAvailable = menuItem.IsAvailable ? "Yes" : "No",
                        CategoryId = menuItem.MenuCategoryId,
                        ServiceArea = menuItem.ServiceArea.ToString(),
                        CreatedAt = menuItem.CreatedAt,
                        UpdatedAt = menuItem.UpdatedAt,
                        Recipes = [.. menuItem.MenuItemRecipes.Select(r => new MenuItemRecipe
                        {
                            Id = r.Id,
                            MenuItemId = r.MenuItemId,
                            InventoryItemId = r.InventoryItemId,
                            CreatedAt = r.CreatedAt,
                            UpdatedAt = r.UpdatedAt,
                            Quantity = r.Quantity,
                        })]
                    };
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

        // Get menu irem by menu category
        public async Task<List<MenuItemViewModel>> GetMenuItemsByMenuCategoryAsync(string menuCategoryId)
        {
            try
            {
                await using var context = await _contextFactory.CreateDbContextAsync();
                var menuItems = await context.MenuItems
                    .Where(m => m.MenuCategoryId == menuCategoryId)
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
                throw new Exception("An error occurred while retrieving menu items by menu category.", ex);
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
                    await context.MenuCategories.AddRangeAsync(defaultCategories);
                    await context.SaveChangesAsync();
                }
            }
            catch (Exception ex)
            {
                throw new Exception("An error occurred while seeding default menu item categories.", ex);
            }
        }
    }
}