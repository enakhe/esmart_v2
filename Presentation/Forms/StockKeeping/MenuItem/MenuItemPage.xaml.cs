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

namespace ESMART.Presentation.Forms.StockKeeping.MenuItem
{
    /// <summary>
    /// Interaction logic for MenuItemPage.xaml
    /// </summary>
    public partial class MenuItemPage : Page
    {
        private readonly IStockKeepingRepository _stockKeepingRepository;

        public MenuItemPage(IStockKeepingRepository stockKeepingRepository)
        {
            _stockKeepingRepository = stockKeepingRepository;
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

        private async void Grid_Loaded(object sender, RoutedEventArgs e)
        {
            await LoadMenuItem();
        }
    }
}
