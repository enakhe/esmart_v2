using ESMART.Application.Common.Interface;
using ESMART.Domain.Entities.FrontDesk;
using ESMART.Domain.ViewModels.FrontDesk;
using ESMART.Infrastructure.Services;
using ESMART.Presentation.Forms.Export;
using ESMART.Presentation.Forms.FrontDesk.Booking;
using ESMART.Presentation.Utils;
using System.Windows;
using System.Windows.Controls;

namespace ESMART.Presentation.Forms.Reports
{
    /// <summary>
    /// Interaction logic for CurrentinHouseGuestReport.xaml
    /// </summary>
    public partial class CurrentinHouseGuestReport : Page
    {
        private readonly IBookingRepository _bookingRepository;
        private readonly IHotelSettingsService _hotelSettingsService;
        private readonly ITransactionRepository _transactionRepository;
        private readonly GuestAccountService _guestAccountService;

        public CurrentinHouseGuestReport(IBookingRepository bookingRepository, IHotelSettingsService hotelSettingsService, ITransactionRepository transactionRepository, GuestAccountService guestAccountService)
        {
            _bookingRepository = bookingRepository;
            _hotelSettingsService = hotelSettingsService;
            _transactionRepository = transactionRepository;
            _guestAccountService = guestAccountService;
            InitializeComponent();
        }

        private async Task LoadData()
        {
            LoaderOverlay.Visibility = Visibility.Visible;
            try
            {
                var expectedDepartureBooking = await _bookingRepository.GetInHouseGuest();
                if (expectedDepartureBooking != null)
                {
                    this.BookingDataGrid.ItemsSource = expectedDepartureBooking;
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

        private async void ExportButton_Click(object sender, RoutedEventArgs e)
        {
            LoaderOverlay.Visibility = Visibility.Visible;
            try
            {
                var columnNames = BookingDataGrid.Columns
                    .Where(c => c.Header != null)
                    .Select(c => c.Header.ToString())
                    .Where(name => !string.IsNullOrWhiteSpace(name) && name != "Operation")
                .ToList();

                var optionsWindow = new ExportDialog(columnNames, BookingDataGrid, _hotelSettingsService, $"Current In House Guests");
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
                            ExportHelper.ExportAndPrint(BookingDataGrid, exportResult.SelectedColumns, exportResult.ExportFormat, exportResult.FileName, hotel.LogoUrl!, hotel.Name, hotel.Email, hotel.PhoneNumber, hotel.Address);
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

        private async void ViewBookingDetails_Click(object sender, RoutedEventArgs e)
        {
            LoaderOverlay.Visibility = Visibility.Visible;
            try
            {
                if (sender is Button button && button.Tag is string Id)
                {
                    var selectedBooking = (BookingViewModel)BookingDataGrid.SelectedItem;

                    var booking = await _bookingRepository.GetBookingById(Id);

                    if (selectedBooking.Id != null)
                    {
                        var bookingDetailsDialog = new ViewBookingDetailsDialog(booking, _transactionRepository, _bookingRepository, _hotelSettingsService, _guestAccountService);
                        bookingDetailsDialog.ShowDialog();
                    }

                    else
                    {
                        MessageBox.Show("Please select a booking before viewing details.", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                    }
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

        private async void Page_Loaded(object sender, RoutedEventArgs e)
        {
            await LoadData();
        }
    }
}
