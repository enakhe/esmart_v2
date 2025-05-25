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

        private async void Grid_Loaded(object sender, RoutedEventArgs e)
        {
            await LoadMenuItem();
        }
    }
}
