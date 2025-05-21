#nullable disable

using ESMART.Application.Common.Interface;
using ESMART.Domain.ViewModels.FrontDesk;
using ESMART.Presentation.Forms.Export;
using ESMART.Presentation.Utils;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interop;

namespace ESMART.Presentation.Forms.FrontDesk.Booking
{
    /// <summary>
    /// Interaction logic for ViewBookingDetailsDialog.xaml
    /// </summary>
    public partial class ViewBookingDetailsDialog : Window
    {
        private readonly Domain.Entities.FrontDesk.Booking _booking;
        private readonly ITransactionRepository _transactionRepository;
        private readonly IBookingRepository _bookingRepository;
        private readonly IHotelSettingsService _hotelSettingsService;
        public ViewBookingDetailsDialog(Domain.Entities.FrontDesk.Booking booking, ITransactionRepository transactionRepository, IBookingRepository bookingRepository, IHotelSettingsService hotelSettingsService)
        {
            _booking = booking;
            _transactionRepository = transactionRepository;
            _bookingRepository = bookingRepository;
            _hotelSettingsService = hotelSettingsService;
            InitializeComponent();

            Loaded += DisableMinimizeButton;
        }

        private void LoadBookinDetails()
        {
            LoaderOverlay.Visibility = Visibility.Visible;
            try
            {
                var bookingViewModel = new BookingViewModel()
                {
                    Id = _booking.Id,
                    Guest = _booking.Guest.FullName,
                    PhoneNumber = _booking.Guest.PhoneNumber,
                    Room = _booking.Room.Number,
                    CheckIn = _booking.CheckIn,
                    CheckOut = _booking.CheckOut,
                    PaymentMethod = _booking.PaymentMethod.ToString(),
                    Duration = _booking.Duration.ToString(),
                    Status = _booking.Status.ToString(),
                    TotalAmount = _booking.TotalAmount.ToString("N2"),
                    CreatedBy = _booking.ApplicationUser?.FullName,
                    DateCreated = _booking.DateCreated,
                    DateModified = _booking.DateModified
                };

                this.DataContext = bookingViewModel;
            }
            catch (Exception ex)
            {
                var sb = new StringBuilder();
                sb.AppendLine(ex.Message);
                MessageBox.Show(sb.ToString(), "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                LoaderOverlay.Visibility = Visibility.Collapsed;
            }
        }

        private void LoadDefaultSetting()
        {
            txtFrom.SelectedDate = DateTime.Now;
            txtTo.SelectedDate = DateTime.Now.AddDays(1);
        }

        private async Task LoadBookingTransactionHistory()
        {
            LoaderOverlay.Visibility = Visibility.Visible;
            try
            {
                var bookingTransactionItem = await _transactionRepository.GetTransactionItemByBookingIdAsync(_booking.Id);
                if (bookingTransactionItem != null)
                {
                    this.TransactionItemDataGrid.ItemsSource = bookingTransactionItem;
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

        private async Task LoadBookingTransactionByDate()
        {
            LoaderOverlay.Visibility = Visibility.Visible;
            try
            {
                var fromDate = txtFrom.SelectedDate.Value;
                var toDate = txtTo.SelectedDate.Value;

                if (fromDate > toDate)
                {
                    MessageBox.Show("From date cannot be greater than To date", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                var bookingTransactionItem = await _transactionRepository.GetTransactionItemByBookingIdAndDate(_booking.Id, fromDate, toDate);

                if (bookingTransactionItem != null)
                {
                    this.TransactionItemDataGrid.ItemsSource = bookingTransactionItem;
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
            await LoadBookingTransactionByDate();
        }

        private async void TabControl_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (e.Source is TabControl tabControl)
            {
                var selectedTab = tabControl.SelectedItem as TabItem;

                if (selectedTab == tbTransactionHistory)
                {
                    await LoadBookingTransactionHistory();
                }
            }
        }

        private async void ExportButton_Click(object sender, RoutedEventArgs e)
        {
            LoaderOverlay.Visibility = Visibility.Visible;
            try
            {
                var columnNames = TransactionItemDataGrid.Columns
                    .Where(c => c.Header != null)
                    .Select(c => c.Header.ToString())
                    .Where(name => !string.IsNullOrWhiteSpace(name) && name != "Operation")
                    .ToList();

                var optionsWindow = new ExportDialog(columnNames);
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
                            ExportHelper.ExportAndPrint(TransactionItemDataGrid, exportResult.SelectedColumns, exportResult.ExportFormat, exportResult.FileName, hotel.LogoUrl!, hotel.Name, hotel.Email, hotel.PhoneNumber, hotel.Address);
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

        private void Window_Activated(object sender, EventArgs e)
        {
            LoadBookinDetails();
            LoadDefaultSetting();
        }

        private void DisableMinimizeButton(object sender, RoutedEventArgs e)
        {
            var hwnd = new WindowInteropHelper(this).Handle;
            int currentStyle = GetWindowLong(hwnd, GWL_STYLE);
            SetWindowLong(hwnd, GWL_STYLE, currentStyle & ~WS_MINIMIZEBOX);
        }

        private const int GWL_STYLE = -16;
        private const int WS_MINIMIZEBOX = 0x00020000;

        [DllImport("user32.dll")]
        private static extern int GetWindowLong(IntPtr hWnd, int nIndex);

        [DllImport("user32.dll")]
        private static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);
    }
}
