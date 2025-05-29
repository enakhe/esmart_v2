using ESMART.Application.Common.Interface;
using ESMART.Application.Common.Utils;
using ESMART.Infrastructure.Repositories.Configuration;
using ESMART.Infrastructure.Services;
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

namespace ESMART.Presentation.Forms.StockKeeping.Order
{
    /// <summary>
    /// Interaction logic for OrderPage.xaml
    /// </summary>
    public partial class OrderPage : Page
    {
        private readonly IStockKeepingRepository _stockKeepingRepository;
        private readonly IBookingRepository _bookingRepository;
        private readonly IGuestRepository _guestRepository;
        private readonly ITransactionRepository _transactionRepository;
        private readonly IHotelSettingsService _hotelSettingsService;
        private readonly GuestAccountService _guestAccountService;
        public OrderPage(IStockKeepingRepository stockKeepingRepository, IBookingRepository bookingRepository, IGuestRepository guestRepository, ITransactionRepository transactionRepository, IHotelSettingsService hotelSettingsService, GuestAccountService guestAccountService)
        {
            _stockKeepingRepository = stockKeepingRepository;
            _bookingRepository = bookingRepository;
            _hotelSettingsService = hotelSettingsService;
            _transactionRepository = transactionRepository;
            _guestRepository = guestRepository;
            _guestAccountService = guestAccountService;
            InitializeComponent();

            txtFrom.SelectedDate = DateTime.Now.AddDays(-7);
            txtTo.SelectedDate = DateTime.Now.AddDays(1);
        }

        private async Task LoadOrder()
        {
            LoaderOverlay.Visibility = Visibility.Visible;
            try
            {
                var from = txtFrom.SelectedDate ?? DateTime.Now.AddDays(-7);
                var to = txtTo.SelectedDate ?? DateTime.Now;

                var orders = await _stockKeepingRepository.GetOrdersByDateRangeAsync(from, to);
                OrderListView.ItemsSource = orders;

                txtMenuItemCount.Text = orders.Count.ToString();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred while loading orders: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                LoaderOverlay.Visibility = Visibility.Collapsed;
            }
        }

        // Add order
        private async void AddOrderButton_Click(object sender, RoutedEventArgs e)
        {
            OrderDialog orderDialog = new OrderDialog(_stockKeepingRepository, _bookingRepository, _guestRepository, _transactionRepository, _guestAccountService)
            {
                Owner = Window.GetWindow(this)
            };

            if(orderDialog.ShowDialog() == true)
            {
                await LoadOrder();
            }
        }

        private async void ExportButton_Click(object sender, RoutedEventArgs e)
        {
            LoaderOverlay.Visibility = Visibility.Visible;
            try
            {
                OrderListView.RowDetailsVisibilityMode = DataGridRowDetailsVisibilityMode.Visible;

                var columnNames = OrderListView.Columns
                    .Where(c => c.Header != null)
                    .Select(c => c.Header.ToString())
                .Where(name => !string.IsNullOrWhiteSpace(name) && name != "Operation")
                .ToList();

                var optionsWindow = new ExportDialog(columnNames, OrderListView, _hotelSettingsService, "All Menu Orders", "OrderItems");
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
                            ExportHelper.ExportAndPrint(OrderListView, exportResult.SelectedColumns, exportResult.ExportFormat, exportResult.FileName, hotel.LogoUrl!, hotel.Name, hotel.Email, hotel.PhoneNumber, hotel.Address);
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

        private async void Page_Loaded(object sender, RoutedEventArgs e)
        {
            await LoadOrder();
        }

        private async void txtSearchBuilding_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            bool isNull = Helper.AreAnyNullOrEmpty(txtSearchBuilding.Text);
            if (isNull)
            {
                await LoadOrder();
            }
        }

        private async void FilterButton_Click(object sender, RoutedEventArgs e)
        {
            LoaderOverlay.Visibility = Visibility.Visible;
            try
            {
                await LoadOrder();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred while filtering orders: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                LoaderOverlay.Visibility = Visibility.Collapsed;
            }
        }

        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            bool isNull = Helper.AreAnyNullOrEmpty(txtSearchBuilding.Text);
            if (isNull)
            {
                await LoadOrder();
            }
            else
            {
                var searchText = txtSearchBuilding.Text.ToLower();
                var filteredBookings = await _stockKeepingRepository.GetOrdersBySearchAsync(searchText);

                if (filteredBookings == null || filteredBookings.Count == 0)
                {
                    await LoadOrder();
                }
                OrderListView.ItemsSource = filteredBookings;
            }
        }
    }
}
