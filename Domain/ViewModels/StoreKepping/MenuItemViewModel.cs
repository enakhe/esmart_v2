#nullable disable

using ESMART.Domain.Entities.StoreKeeping;
using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel; 
using System.Windows.Input;

namespace ESMART.Domain.ViewModels.StoreKepping
{
    public class MenuItemViewModel : ObservableObject
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public decimal Price { get; set; }
        public string IsAvailable { get; set; }
        public string CategoryId { get; set; }
        public byte[] Image { get; set; }
        public bool IsLow { get; set; }
        public string ServiceArea { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        public ObservableCollection<MenuCategory> Categories { get; set; }
        public ObservableCollection<MenuItemRecipe> Recipes { get; set; }

        public ICommand SaveCommand { get; }
        public ICommand ToggleAvailabilityCommand { get; }
        public ICommand AddRecipeCommand { get; }
    }
}
