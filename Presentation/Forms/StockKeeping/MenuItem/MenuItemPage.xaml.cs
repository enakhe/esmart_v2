using ESMART.Application.Common.Interface;
using ESMART.Application.Common.Utils;
using ESMART.Infrastructure.Repositories.Configuration;
using ESMART.Presentation.Forms.Export;
using ESMART.Presentation.Forms.StockKeeping.MenuCategory;
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

namespace ESMART.Presentation.Forms.StockKeeping.MenuItem
{
    /// <summary>
    /// Interaction logic for MenuItemPage.xaml
    /// </summary>
    public partial class MenuItemPage : Page
    {
        private readonly IStockKeepingRepository _stockKeepingRepository;
        private readonly IHotelSettingsService _hotelSettingsService;
        public MenuItemPage(IStockKeepingRepository stockKeepingRepository, IHotelSettingsService hotelSettingsService)
        {
            _stockKeepingRepository = stockKeepingRepository;
            _hotelSettingsService = hotelSettingsService;
            InitializeComponent();
        }

        private async Task LoadMenuItem()
        {
            LoaderOverlay.Visibility = Visibility.Visible;
            try
            {
                var menuItems = await _stockKeepingRepository.GetMenuItemsAsync();
                MenuItemDataGrid.ItemsSource = menuItems;

                txtMenuItemCount.Text = menuItems.Count.ToString();
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

        public async void AddMenuItem_Click(object sender, RoutedEventArgs e)
        {
            var addMenuItemDialog = new AddMenuItemDialog(_stockKeepingRepository);

            if (addMenuItemDialog.ShowDialog() == true)
            {
                await LoadMenuItem();
            }
        }

        private async void ExportButton_Click(object sender, RoutedEventArgs e)
        {
            LoaderOverlay.Visibility = Visibility.Visible;
            try
            {
                var columnNames = MenuItemDataGrid.Columns
                    .Where(c => c.Header != null)
                    .Select(c => c.Header.ToString())
                .Where(name => !string.IsNullOrWhiteSpace(name) && name != "Operation")
                    .ToList();

                var optionsWindow = new ExportDialog(columnNames, MenuItemDataGrid, _hotelSettingsService, "All Menu Item");
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
                            ExportHelper.ExportAndPrint(MenuItemDataGrid, exportResult.SelectedColumns, exportResult.ExportFormat, exportResult.FileName, hotel.LogoUrl!, hotel.Name, hotel.Email, hotel.PhoneNumber, hotel.Address);
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

        // delete menu item
        private async void DeleteMenuItem_Click(object sender, RoutedEventArgs e)
        {
            if (MenuItemDataGrid.SelectedItem is null)
            {
                MessageBox.Show("Please select a menu item to delete.", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            var selectedMenuItem = MenuItemDataGrid.SelectedItem as Domain.ViewModels.StoreKepping.MenuItemViewModel;
            if (selectedMenuItem == null)
            {
                MessageBox.Show("Invalid menu item selected.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            var result = MessageBox.Show($"Are you sure you want to delete the menu item '{selectedMenuItem.Name}'?", "Confirm Delete", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (result == MessageBoxResult.Yes)
            {
                LoaderOverlay.Visibility = Visibility.Visible;
                try
                {
                    await _stockKeepingRepository.DeleteMenuItemAsync(selectedMenuItem.Id);
                    await LoadMenuItem();
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
        }

        // update mwnu item
        private async void EditMenuItem_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is string Id)
            {
                var selectedItem = (Domain.ViewModels.StoreKepping.MenuItemViewModel)MenuItemDataGrid.SelectedItem;

                if (selectedItem.Id != null)
                {
                    var menuItem = await _stockKeepingRepository.GetMenuItemByIdAsync(selectedItem.Id);

                    UpdateMenuItemDialog updateMenuItemDialog = new UpdateMenuItemDialog(_stockKeepingRepository, menuItem!);
                    if (updateMenuItemDialog.ShowDialog() == true)
                    {
                        _ = LoadMenuItem();
                    }
                }
                else
                {
                    MessageBox.Show("Please select a guest before editing.", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
        }

        private async void Page_Loaded(object sender, RoutedEventArgs e)
        {
            await LoadMenuItem();
        }

        private async void txtSearchBuilding_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            bool isNull = Helper.AreAnyNullOrEmpty(txtSearchBuilding.Text);
            if (isNull)
            {
                await LoadMenuItem();
            }
        }

        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            bool isNull = Helper.AreAnyNullOrEmpty(txtSearchBuilding.Text);
            if (isNull)
            {
                await LoadMenuItem();
            }
            else
            {
                var searchText = txtSearchBuilding.Text.ToLower();
                var filteredBookings = await _stockKeepingRepository.SearchMenuItemsAsync(searchText);

                if (filteredBookings == null || filteredBookings.Count == 0)
                {
                    await LoadMenuItem();
                }
                MenuItemDataGrid.ItemsSource = filteredBookings;
            }
        }
    }
}
