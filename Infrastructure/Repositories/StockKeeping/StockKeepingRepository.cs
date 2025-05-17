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
    public class StockKeepingRepository(IDbContextFactory<ApplicationDbContext> contextFactory)
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
                    .ToListAsync();

                return [.. menuItems.Select(m => new MenuItemViewModel
                {
                    Id = m.Id,
                    Name = m.Name,
                    Description = m.Description,
                    Price = m.Price,
                    IsAvailable = m.IsAvailable,
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
                            UnitOfMeasure = r.UnitOfMeasure
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
                        IsAvailable = menuItem.IsAvailable,
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
                            UnitOfMeasure = r.UnitOfMeasure
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

    }
}