#nullable disable

using ESMART.Application.Common.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace ESMART.Presentation.Forms.StockKeeping.MenuItemRecipe
{
    /// <summary>
    /// Interaction logic for AddMenuItemRecipeDialog.xaml
    /// </summary>
    public partial class AddMenuItemRecipeDialog : Window
    {
        private readonly IStockKeepingRepository _stockKeepingRepository;
        public AddMenuItemRecipeDialog(IStockKeepingRepository stockKeepingRepository)
        {
            _stockKeepingRepository = stockKeepingRepository;
            InitializeComponent();
        }

        private async Task LoadInventoryItem()
        {
            LoaderOverlay.Visibility = Visibility.Visible;
            try
            {
                var inventoryItems = await _stockKeepingRepository.GetAllInventoryItemsAsync();
                if (inventoryItems != null)
                {
                    cmbInventory.ItemsSource = inventoryItems;
                    cmbInventory.DisplayMemberPath = "Name";
                    cmbInventory.SelectedValuePath = "Id";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                LoaderOverlay.Visibility = Visibility.Collapsed;
            }
        }

        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            await LoadInventoryItem();
        }
    }

    public class InventoryItemOption
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public decimal Quantity { get; set; }
        public bool IsSelected { get; set; } = false;
    }
}
