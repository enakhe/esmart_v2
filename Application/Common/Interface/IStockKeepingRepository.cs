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
        Task<MenuItemViewModel> GetMenuItemByIdAsync(string id);
        Task UpdateMenuItemAsync(MenuItem menuItem);
        Task DeleteMenuItemAsync(string id);
        Task ToggleMenuItemAvailabilityAsync(string id);
        Task AddRecipeToMenuItemAsync(string menuItemId, MenuItemRecipe recipe);
        Task RemoveRecipeFromMenuItemAsync(string recipeId);
        Task<List<MenuItemViewModel>> GetMenuItemsByCategoryIdAsync(string categoryId);
        Task<List<MenuItemViewModel>> GetMenuItemsByMenuCategoryAsync(string menuCategoryId);
        Task AddMenuItemCategoryAsync(MenuCategory menuCategory);
        Task<List<MenuCategory>> GetMenuItemCategoriesAsync();
        Task<MenuCategory> GetMenuItemCategoryByIdAsync(string id);
        Task UpdateMenuItemCategoryAsync(MenuCategory menuCategory);
        Task DeleteMenuItemCategoryAsync(string id);
        Task<MenuCategory> GetMenuItemCategoryByNameAsync(string name);
        Task SeedDefaultMenuItemCategoriesAsync();
    }
}
