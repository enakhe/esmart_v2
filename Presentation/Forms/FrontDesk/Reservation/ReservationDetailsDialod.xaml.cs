using ESMART.Application.Common.Dtos;
using ESMART.Application.Common.Interface;
using ESMART.Domain.Entities.FrontDesk;
using ESMART.Domain.ViewModels.FrontDesk;
using ESMART.Domain.ViewModels.RoomSetting;
using ESMART.Infrastructure.Repositories.FrontDesk;
using ESMART.Infrastructure.Services;
using ESMART.Presentation.Forms.FrontDesk.Booking;
using ESMART.Presentation.Forms.Home;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace ESMART.Presentation.Forms.FrontDesk.Reservation
{
    /// <summary>
    /// Interaction logic for ReservationDetailsDialod.xaml
    /// </summary>
    public partial class ReservationDetailsDialod : Window
    {
        private readonly RoomTypeReservationViewModel _reservationDto;
        private readonly GuestAccountService _guestAccountService;
        private readonly IRoomRepository _roomRepository;
        private readonly IGuestRepository _guestRepository;
        private readonly IHotelSettingsService _hotelSettingsService;
        private readonly IBookingRepository _bookingRepository;
        private readonly ITransactionRepository _transactionRepository;
        public ReservationDetailsDialod(IRoomRepository roomRepository, IGuestRepository guestRepository, IBookingRepository bookingRepository, IHotelSettingsService hotelSettingsService, ITransactionRepository transactionRepository, RoomTypeReservationViewModel reservationDto, GuestAccountService guestAccountService)
        {
            _roomRepository = roomRepository;
            _guestRepository = guestRepository;
            _bookingRepository = bookingRepository;
            _hotelSettingsService = hotelSettingsService;
            _transactionRepository = transactionRepository;
            _reservationDto = reservationDto;
            _guestAccountService = guestAccountService;
            InitializeComponent();

            this.DataContext = _reservationDto;
            Loaded += DisableMinimizeButton;

        }

        private async Task LoadReservationTransactionHistory()
        {
            LoaderOverlay.Visibility = Visibility.Visible;
            try
            {
                var roomTransactionItem = await _guestAccountService.GetMatchingGuestTransactionsAsync(_reservationDto.Id);
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

        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            await LoadReservationTransactionHistory();
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

        private async void CancleButton_Click(object sender, RoutedEventArgs e)
        {
            var result = MessageBox.Show(
                "Are you sure you want to cancel this reservation?",
                "Confirm Cancellation",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (result == MessageBoxResult.Yes)
            {
                try
                {

                    await _guestAccountService.CancelReservationAsync(_reservationDto.Id);
                    MessageBox.Show("Reservation has been successfully canceled.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error: {ex.Message}", "Cancellation Failed", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private async void BookReservation_Click(object sender, RoutedEventArgs e)
        {
            var result = MessageBox.Show(
                "Are you sure you want to book this reservation?",
                "Confirm Booking",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);


            if (result == MessageBoxResult.Yes)
            {
                await _guestAccountService.ConvertReservationToBookingAsync(_reservationDto.Id, _reservationDto.GuestId);

                // Fetch reservation details
                var reservation = await _guestAccountService.GetReservationByIdAsync(_reservationDto.Id)
                    ?? throw new Exception("Reservation not found.");

                // Ensure room type matches the reserved room type
                var availableRooms = await _roomRepository.GetAvailableRooms();
                var matchingRooms = availableRooms.Where(r => r.RoomTypeId == reservation.RoomTypeId).ToList();

                if (!matchingRooms.Any())
                {
                    MessageBox.Show("No available rooms match the reserved room type.", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                // Prepare view model with only matching rooms (convert Room to SelectableRoomViewModel)
                var selectedRoomVm = new IndexPageViewModel();
                foreach (var room in matchingRooms)
                {
                    var selectableRoom = new SelectableRoomViewModel(room);
                    selectableRoom.IsSelected = true;

                    selectedRoomVm.Rooms.Add(selectableRoom);
                    selectedRoomVm.SelectedRooms.Add(selectableRoom);
                }

                // Open dialog with filtered rooms
                var dialog = new AddBulkBookingDialog(
                    _roomRepository,
                    _hotelSettingsService,
                    _guestRepository,
                    _transactionRepository,
                    _guestAccountService,
                    selectedRoomVm
                );

                if (dialog.ShowDialog() == true)
                {
                    this.DialogResult = true;
                }
            }
        }
    }
}
