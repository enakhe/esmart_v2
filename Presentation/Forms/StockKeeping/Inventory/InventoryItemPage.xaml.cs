using ESMART.Application.Common.Interface;
using ESMART.Infrastructure.Repositories.Configuration;
using ESMART.Presentation.Forms.Export;
using ESMART.Presentation.Utils;
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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace ESMART.Presentation.Forms.StockKeeping.Inventory
{
    /// <summary>
    /// Interaction logic for InventoryItemPage.xaml
    /// </summary>
    public partial class InventoryItemPage : Page
    {
        private readonly IStockKeepingRepository _stockKeepingRepository;
        private readonly IHotelSettingsService _hotelSettingsService;
        public InventoryItemPage(IStockKeepingRepository stockKeepingRepository, IHotelSettingsService hotelSettingsService)
        {
            _stockKeepingRepository = stockKeepingRepository;
            _hotelSettingsService = hotelSettingsService;
            InitializeComponent();
        }

        private async Task LoadInventoryItem()
        {
            LoaderOverlay.Visibility = Visibility.Visible;
            try
            {
                var inventoryItems = await _stockKeepingRepository.GetAllInventoryItemsAsync();
                InventoryItemDataGrid.ItemsSource = inventoryItems;

                txtMenuItemCount.Text = inventoryItems.Count.ToString();
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

        private async void ExportButton_Click(object sender, RoutedEventArgs e)
        {
            LoaderOverlay.Visibility = Visibility.Visible;
            try
            {
                var columnNames = InventoryItemDataGrid.Columns
                    .Where(c => c.Header != null)
                    .Select(c => c.Header.ToString())
                .Where(name => !string.IsNullOrWhiteSpace(name) && name != "Operation")
                    .ToList();

                var optionsWindow = new ExportDialog(columnNames, InventoryItemDataGrid, _hotelSettingsService, "All Inventory Item");
                var result = optionsWindow.ShowDialog();

                if (result == true)
                {
                    var exportResult = optionsWindow.GetResult();
                    var hotel = await _hotelSettingsService.GetHotelInformation();

                    if (exportResult.SelectedColumns.Count == 0)
                    {
                        MessageBox.Show("Please select at least one column to export.", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                    }
                    else
                    {
                        if (hotel != null)
                        {
                            ExportHelper.ExportAndPrint(InventoryItemDataGrid, exportResult.SelectedColumns, exportResult.ExportFormat, exportResult.FileName, hotel.LogoUrl!, hotel.Name, hotel.Email, hotel.PhoneNumber, hotel.Address);
                        }
                    }

                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, ex.Source, MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
            finally
            {
                LoaderOverlay.Visibility = Visibility.Collapsed;
            }
        }

        private void AddInventoryItemButton_Click(object sender, RoutedEventArgs e)
        {
            var addInventoryItemDialog = new AddInventoryDialog(_stockKeepingRepository);
            addInventoryItemDialog.ShowDialog();
        }

        // delete inventory item
        private async void DeleteInventoryItemButton_Click(object sender, RoutedEventArgs e)
        {
            if (InventoryItemDataGrid.SelectedItem is Domain.ViewModels.StoreKepping.InventoryViewModel selectedItem)
            {
                var result = MessageBox.Show($"Are you sure you want to delete {selectedItem.Name}?", "Confirm Delete", MessageBoxButton.YesNo, MessageBoxImage.Warning);
                if (result == MessageBoxResult.Yes)
                {
                    try
                    {
                        await _stockKeepingRepository.DeleteInventoryItemAsync(selectedItem.Id);
                        await LoadInventoryItem();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
            else
            {
                MessageBox.Show("Please select an inventory item to delete.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        // edit inventory item
        private async void EditInventoryItemButton_Click(object sender, RoutedEventArgs e)
        {
            if (InventoryItemDataGrid.SelectedItem is Domain.ViewModels.StoreKepping.InventoryViewModel selectedItem)
            {
                var inventory = await _stockKeepingRepository.GetInventoryItemByIdAsync(selectedItem.Id);

                var updateInventoryDialog = new UpdateInventoryDialog(_stockKeepingRepository, inventory);
                updateInventoryDialog.ShowDialog();
            }
            else
            {
                MessageBox.Show("Please select an inventory item to edit.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private async void Page_Loaded(object sender, RoutedEventArgs e)
        {
            await LoadInventoryItem();
        }
    }
}
