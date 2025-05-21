#nullable disable

using ESMART.Application.Common.Interface;
using ESMART.Domain.ViewModels.RoomSetting;
using ESMART.Presentation.Forms.Export;
using ESMART.Presentation.Utils;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows;
using System.Windows.Interop;

namespace ESMART.Presentation.Forms.FrontDesk.Room
{
    /// <summary>
    /// Interaction logic for RoomDetailsDialog.xaml
    /// </summary>
    public partial class RoomDetailsDialog : Window
    {
        private readonly ITransactionRepository _transactionRepository;
        private readonly IBookingRepository _bookingRepository;
        private readonly IHotelSettingsService _hotelSettingsService;
        private readonly IRoomRepository _roomRepository;
        private readonly Domain.Entities.RoomSettings.Room _room;
        public RoomDetailsDialog(IRoomRepository roomRepository, ITransactionRepository transactionRepository, IBookingRepository bookingRepository, IHotelSettingsService hotelSettingsService, Domain.Entities.RoomSettings.Room room)
        {
            _roomRepository = roomRepository;
            _transactionRepository = transactionRepository;
            _bookingRepository = bookingRepository;
            _roomRepository = roomRepository;
            _hotelSettingsService = hotelSettingsService;
            _room = room;
            InitializeComponent();

            Loaded += DisableMinimizeButton;
        }

        private void LoadRoomDetails()
        {
            LoaderOverlay.Visibility = Visibility.Visible;
            try
            {
                var roomViewModel = new RoomViewModel()
                {
                    Id = _room.Id,
                    Number = _room.Number,
                    Rate = _room.Rate.ToString("N2"),
                    Building = _room.Building.Name + $" ({_room.Building.Number})",
                    RoomType = _room.RoomType.Name,
                    Area = _room.Area.Name + $" ({_room.Area.Number})",
                    Floor = _room.Floor.Name + $" ({_room.Floor.Number})",
                    Status = _room.Status,
                    CreatedBy = _room.ApplicationUser?.FullName,
                    DateCreated = _room.DateCreated,
                    DateModified = _room.DateModified
                };

                this.DataContext = roomViewModel;
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
                var roomTransactionItem = await _transactionRepository.GetTransactionItemByRoomIdAsync(_room.Id);
                if (roomTransactionItem != null)
                {
                    this.TransactionItemDataGrid.ItemsSource = roomTransactionItem;
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

                var reservationTransactionItem = await _transactionRepository.GetTransactionItemByRoomIdAndDate(_room.Id, fromDate, toDate);

                if (reservationTransactionItem != null)
                {
                    this.TransactionItemDataGrid.ItemsSource = reservationTransactionItem;
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

        private async void Window_Activated(object sender, EventArgs e)
        {
            LoadRoomDetails();
            await LoadBookingTransactionHistory();
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
