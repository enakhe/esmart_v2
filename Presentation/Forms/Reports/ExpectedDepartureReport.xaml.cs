using ESMART.Application.Common.Interface;
using ESMART.Domain.ViewModels.FrontDesk;
using ESMART.Presentation.Forms.Export;
using ESMART.Presentation.Forms.FrontDesk.Booking;
using ESMART.Presentation.Utils;
using System.Windows;
using System.Windows.Controls;

namespace ESMART.Presentation.Forms.Reports
{
    /// <summary>
    /// Interaction logic for ExpectedDepartureReport.xaml
    /// </summary>
    public partial class ExpectedDepartureReport : Page
    {
        private readonly IBookingRepository _bookingRepository;
        private readonly IHotelSettingsService _hotelSettingsService;
        private readonly ITransactionRepository _transactionRepository;
        public ExpectedDepartureReport(IBookingRepository bookingRepository, IHotelSettingsService hotelSettingsService, ITransactionRepository transactionRepository)
        {
            _bookingRepository = bookingRepository;
            _hotelSettingsService = hotelSettingsService;
            _transactionRepository = transactionRepository;
            InitializeComponent();
        }

        private void LoadDefaultSetting()
        {
            txtFrom.SelectedDate = DateTime.Now;
            txtTo.SelectedDate = DateTime.Now.AddDays(1);
        }

        private async Task LoadData()
        {
            LoaderOverlay.Visibility = Visibility.Visible;
            try
            {
                var expectedDepartureBooking = await _bookingRepository.GetExpectedDepartureBooking(DateTime.Now, DateTime.Now.AddDays(1));
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

                var optionsWindow = new ExportDialog(columnNames, BookingDataGrid, _hotelSettingsService, "Expected Departure Booking");
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

        private async Task LoadExpectedDepartureBookingByDate()
        {
            LoaderOverlay.Visibility = Visibility.Visible;
            try
            {
                var fromDate = txtFrom.SelectedDate.Value!;
                var toDate = txtTo.SelectedDate.Value!;

                if (fromDate > toDate)
                {
                    MessageBox.Show("From date cannot be greater than To date", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                var expectedDepartureBooking = await _bookingRepository.GetExpectedDepartureBooking(fromDate, toDate);

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

        private async void FilterButton_Click(object sender, RoutedEventArgs e)
        {
            await LoadExpectedDepartureBookingByDate();
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
                        var bookingDetailsDialog = new ViewBookingDetailsDialog(booking, _transactionRepository, _bookingRepository, _hotelSettingsService);
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
            LoadDefaultSetting();
            await LoadData();
        }
    }
}
