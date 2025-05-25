using ESMART.Application.Common.Interface;
using ESMART.Infrastructure.Repositories.Configuration;
using ESMART.Infrastructure.Repositories.FrontDesk;
using ESMART.Presentation.Forms.Export;
using ESMART.Presentation.Forms.FrontDesk.Guest;
using ESMART.Presentation.Forms.StockKeeping.MenuItem;
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

namespace ESMART.Presentation.Forms.StockKeeping.MenuCategory
{
    /// <summary>
    /// Interaction logic for MenuCategoryPage.xaml
    /// </summary>
    public partial class MenuCategoryPage : Page
    {
        private readonly IStockKeepingRepository _stockKeepingRepository;
        private readonly IHotelSettingsService _hotelSettingsService;
        public MenuCategoryPage(IStockKeepingRepository stockKeepingRepository, IHotelSettingsService hotelSettingsService)
        {
            _stockKeepingRepository = stockKeepingRepository;
            _hotelSettingsService = hotelSettingsService;
            InitializeComponent();
        }

        private async Task LoadMenuCategory()
        {
            LoaderOverlay.Visibility = Visibility.Visible;
            try
            {
                var menuCategory = await _stockKeepingRepository.GetAllMenuItemCategoriesViewModelAsync();
                MenuItemDataGrid.ItemsSource = menuCategory;

                txtCount.Text = menuCategory.Count.ToString();
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
        public async void AddMenuCategory_Click(object sender, RoutedEventArgs e)
        {
            var addmenuCategoryDialog = new AddMenuCategoryDialog(_stockKeepingRepository);

            if (addmenuCategoryDialog.ShowDialog() == true)
            {
                await LoadMenuCategory();
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

                var optionsWindow = new ExportDialog(columnNames, MenuItemDataGrid, _hotelSettingsService);
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

        // delete menu category
        private async void DeleteMenuCategory_Click(object sender, RoutedEventArgs e)
        {
            if (MenuItemDataGrid.SelectedItem is null)
            {
                MessageBox.Show("Please select a menu category to delete.", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            var selectedCategory = (Domain.Entities.StoreKeeping.MenuCategory)MenuItemDataGrid.SelectedItem;
            if (MessageBox.Show($"Are you sure you want to delete the menu category '{selectedCategory.Name}'?", "Confirm Delete", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                LoaderOverlay.Visibility = Visibility.Visible;
                try
                {
                    await _stockKeepingRepository.DeleteMenuItemCategoryAsync(selectedCategory.Id);
                    await LoadMenuCategory();
                    MessageBox.Show("Menu category deleted successfully.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
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

        private async void EditGuest_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is string Id)
            {
                var selectedItem = (Domain.ViewModels.StoreKepping.MenuCategoryViewModel)MenuItemDataGrid.SelectedItem;

                if (selectedItem.Id != null)
                {
                    var menuCategory = await _stockKeepingRepository.GetMenuItemCategoryByIdAsync(selectedItem.Id);

                    UpdateMenuCategoryDialog updateMenuCategoryDialog = new UpdateMenuCategoryDialog(_stockKeepingRepository, menuCategory);
                    if (updateMenuCategoryDialog.ShowDialog() == true)
                    {
                        _ = LoadMenuCategory();
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
            await LoadMenuCategory();
        }
    }
}
