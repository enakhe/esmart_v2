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
        public InventoryItemPage(IStockKeepingRepository stockKeepingRepository)
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
                InventoryItemDataGrid.ItemsSource = inventoryItems;
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

        private void AddInventoryItemButton_Click(object sender, RoutedEventArgs e)
        {
            var addInventoryItemDialog = new AddInventoryDialog(_stockKeepingRepository);
            addInventoryItemDialog.ShowDialog();
        }

        private async void Page_Loaded(object sender, RoutedEventArgs e)
        {
            await LoadInventoryItem();
        }
    }
}
