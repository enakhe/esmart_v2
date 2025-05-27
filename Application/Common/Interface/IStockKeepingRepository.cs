using ESMART.Domain.Entities.StoreKeeping;
using ESMART.Domain.ViewModels.StoreKepping;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ESMART.Application.Common.Interface
{
    public interface IStockKeepingRepository
    {
        Task AddMenuItemAsync(MenuItem menuItem);
        Task<List<MenuItemViewModel>> GetMenuItemsAsync();
        Task<MenuItem?> GetMenuItemByIdAsync(string id);
        Task<List<MenuCategoryGroup>> GetGroupedMenuItemsAsync(string category);
        Task UpdateMenuItemAsync(MenuItem menuItem);
        Task DeleteMenuItemAsync(string id);
        Task<List<MenuCategoryViewModel>> GetAllMenuItemCategoriesViewModelAsync();
        Task ToggleMenuItemAvailabilityAsync(string id);
        Task AddRecipeToMenuItemAsync(string menuItemId, MenuItemRecipe recipe);
        Task RemoveRecipeFromMenuItemAsync(string recipeId);
        Task<List<MenuItemViewModel>> GetMenuItemsByCategoryIdAsync(string categoryId);
        Task<List<MenuCategory>> GetMenuCategoriesByServiceAreaAsync(ServiceArea serviceArea);
        //Task<List<MenuItemViewModel>> GetMenuItemsByMenuCategoryAsync(string menuCategoryId);
        Task AddMenuItemCategoryAsync(MenuCategory menuCategory);
        Task<List<MenuItemViewModel>> SearchMenuItemsAsync(string searchTerm);
        Task<List<InventoryViewModel>> SearchInventoryItemsAsync(string searchTerm);
        Task<List<MenuCategory>> GetMenuItemCategoriesAsync();
        Task<MenuCategory> GetMenuItemCategoryByIdAsync(string id);
        Task UpdateMenuItemCategoryAsync(MenuCategory menuCategory);
        Task DeleteMenuItemCategoryAsync(string id);
        Task<MenuCategory> GetMenuItemCategoryByNameAsync(string name);
        Task SeedDefaultMenuItemCategoriesAsync();

        Task AddInventoryItemAsync(InventoryItem inventoryItem);
        Task<InventoryItem> GetInventoryItemByIdAsync(string id);
        Task<InventoryItem> GetInventoryItemByNameAsync(string name);
        Task<List<InventoryViewModel>> GetAllInventoryItemsAsync();
        Task UpdateInventoryItemAsync(InventoryItem inventoryItem);
        Task DeleteInventoryItemAsync(string id);
        Task ToggleInventoryItemAvailabilityAsync(string id);

        Task AddMenuItemRecipeAsync(MenuItemRecipe menuItemRecipe);
        Task<MenuItemRecipe> GetMenuItemRecipeByIdAsync(string id);
        Task<List<MenuItemRecipe>> GetAllMenuItemRecipesAsync();
        Task UpdateMenuItemRecipeAsync(MenuItemRecipe menuItemRecipe);
        Task DeleteMenuItemRecipeAsync(string id);
        Task<List<MenuItemRecipe>> GetMenuItemRecipesByMenuItemIdAsync(string menuItemId);
        Task<List<MenuItemRecipe>> GetMenuItemRecipesByInventoryItemIdAsync(string inventoryItemId);
        Task<MenuItemRecipe> GetMenuItemRecipeByMenuItemAndInventoryItemIdAsync(string menuItemId, string inventoryItemId);
        Task<List<MenuCategory>> GetAllMenuItemCategoriesAsync();

        Task AddOrderAsync(Order order);
        Task DeleteOrderAsync(string id);
        Task UpdateOrderAsync(Order order);
        Task<Order?> GetOrderByIdAsync(string id);
        Task<List<MenuOrderViewModel>> GetAllOrdersAsync();
        Task<List<Order>> GetOrdersByBookingIdAsync(string bookingId);
        Task<List<MenuOrderViewModel>> GetOrdersBySearchAsync(string searchTerm);
        Task<List<MenuOrderViewModel>> GetOrdersByDateRangeAsync(DateTime fromDate, DateTime toDate);

    }
}
