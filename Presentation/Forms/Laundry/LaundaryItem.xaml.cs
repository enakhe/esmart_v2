using ESMART.Application.Common.Interface;
using ESMART.Application.Common.Utils;
using ESMART.Infrastructure.Repositories.Configuration;
using ESMART.Infrastructure.Repositories.StockKeeping;
using ESMART.Infrastructure.Services;
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

namespace ESMART.Presentation.Forms.Laundry
{
    /// <summary>
    /// Interaction logic for LaundaryItem.xaml
    /// </summary>
    public partial class LaundaryItem : Page
    {
        private readonly GuestAccountService _guestAccountService;
        private readonly ITransactionRepository _transactionRepository;
        private readonly IStockKeepingRepository _stockKeepingRepository;
        private readonly IHotelSettingsService _hotelSettingsService;
        public LaundaryItem(GuestAccountService guestAccountService, ITransactionRepository transactionRepository, IStockKeepingRepository stockKeepingRepository, IHotelSettingsService hotelSettingsService)
        {
            _guestAccountService = guestAccountService;
            _transactionRepository = transactionRepository;
            _stockKeepingRepository = stockKeepingRepository;
            _hotelSettingsService = hotelSettingsService;
            InitializeComponent();
        }

        private async Task LoadOrder()
        {
            LoaderOverlay.Visibility = Visibility.Visible;
            try
            {
                var from = txtFrom.SelectedDate ?? DateTime.Now.AddDays(-7);
                var to = txtTo.SelectedDate ?? DateTime.Now;

                var orders = await _stockKeepingRepository.GetLaundaryOrdersByDateRangeAsync(from, to);
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

        private void AddItemButton_Click(object sender, RoutedEventArgs e)
        {
            AddLaundaryOrderDialog laundaryOrderDialog = new AddLaundaryOrderDialog(_guestAccountService, _transactionRepository);
            laundaryOrderDialog.ShowDialog();
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
                var filteredBookings = await _stockKeepingRepository.GetLaundryOrdersBySearchAsync(searchText);

                if (filteredBookings == null || filteredBookings.Count == 0)
                {
                    await LoadOrder();
                }
                OrderListView.ItemsSource = filteredBookings;
            }
        }

        private async void PrintButon_Click(object sender, RoutedEventArgs e)
        {
            LoaderOverlay.Visibility = Visibility.Visible;
            try
            {
                if (sender is Button button && button.Tag is string id)
                {
                    var order = await _stockKeepingRepository.GetLaundryOrderByIdAsync(id);
                    if (order != null)
                    {
                        var hotel = await _hotelSettingsService.GetHotelInformation();

                        var printer = new PrintHelper();
                        var doc = printer.CreateLaundryReceipt(order.OrderId, order.RoomBooking.OccupantName, order.OrderItems, hotel);

                        string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmm");
                        PrintReceipt(doc);
                        PrintHelper.SaveFlowDocumentToFile(doc, $"{order.RoomBooking.OccupantName.Replace(" ", "-")}-Receipt-{timestamp}");
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

        private void PrintReceipt(FlowDocument receiptDocument)
        {
            PrintDialog printDialog = new PrintDialog();
            if (printDialog.ShowDialog() == true)
            {
                printDialog.PrintDocument(((IDocumentPaginatorSource)receiptDocument).DocumentPaginator, "Bar Receipt");
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

        private async void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            LoaderOverlay.Visibility = Visibility.Visible;
            try
            {
                if (sender is Button button && button.Tag is string id)
                {
                    var order = await _stockKeepingRepository.GetLaundryOrderByIdAsync(id);

                    // Ask for confirmation before proceeding
                    var result = MessageBox.Show($"Are you sure you want to cancel laundry order {order.OrderId}?",
                        "Confirm Cancellation", MessageBoxButton.YesNo, MessageBoxImage.Question);

                    // Only proceed if user confirms
                    if (result == MessageBoxResult.Yes)
                    {
                        await _guestAccountService.CancelLaundryOrderAsync(id);

                        // Show success message
                        MessageBox.Show($"Order {order.OrderId} has been successfully cancelled.",
                            "Cancellation Successful", MessageBoxButton.OK, MessageBoxImage.Information);

                        await LoadOrder();
                    }
                }
            }
            catch (Exception ex)
            {
                // Show error message
                MessageBox.Show($"Failed to cancel order.\nError: {ex.Message}",
                    "Error", MessageBoxButton.OK, MessageBoxImage.Error);
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
    }
}
