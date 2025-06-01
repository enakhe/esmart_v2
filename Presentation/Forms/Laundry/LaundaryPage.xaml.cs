using ESMART.Infrastructure.Services;
using ESMART.Presentation.Forms.StockKeeping.Inventory;
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

namespace ESMART.Presentation.Forms.Laundry
{
    /// <summary>
    /// Interaction logic for LaundaryPage.xaml
    /// </summary>
    public partial class LaundaryPage : Page
    {
        private readonly GuestAccountService _guestAccountService;
        public LaundaryPage(GuestAccountService guestAccountService)
        {
            _guestAccountService = guestAccountService;
            InitializeComponent();
        }

        private async Task LoadLaundaryServiceItem()
        {
            LoaderOverlay.Visibility = Visibility.Visible;
            try
            {
                var laundaryService = await _guestAccountService.GetAllLaundaryItems();
                InventoryItemDataGrid.ItemsSource = laundaryService;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occured when getting laundary services. {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                LoaderOverlay.Visibility = Visibility.Collapsed;
            }
        }

        private async void AddItemButton_Click(object sender, RoutedEventArgs e)
        {
            var addLaundaryService = new AddLaundaryServiceDialog(_guestAccountService, null!);
            if (addLaundaryService.ShowDialog() == true)
            {
                await LoadLaundaryServiceItem();
            }

        }

        private void SearchButton_Click(object sender, RoutedEventArgs e)
        {

        }

        private void txtSearchBuilding_KeyDown(object sender, KeyEventArgs e)
        {

        }

        private async void EditButton_Click(object sender, RoutedEventArgs e)
        {
            if (InventoryItemDataGrid.SelectedItem is Domain.Entities.Laundry.Laundry selectedItem)
            {
                var addLaundaryService = new AddLaundaryServiceDialog(_guestAccountService, selectedItem!);
                if (addLaundaryService.ShowDialog() == true)
                {
                    await LoadLaundaryServiceItem();
                }
            }
            else
            {
                MessageBox.Show("Please select an inventory item to edit.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private async void Page_Loaded(object sender, RoutedEventArgs e)
        {
            await LoadLaundaryServiceItem();
        }
    }
}
