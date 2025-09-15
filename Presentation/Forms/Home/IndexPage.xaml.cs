using ESMART.Application.Common.Interface;
using ESMART.Domain.Entities.RoomSettings;
using ESMART.Domain.ViewModels.RoomSetting;
using ESMART.Infrastructure.Repositories.Configuration;
using ESMART.Infrastructure.Services;
using ESMART.Presentation.Forms.FrontDesk.Booking;
using ESMART.Presentation.Forms.FrontDesk.Room;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace ESMART.Presentation.Forms.Home
{
    /// <summary>
    /// Interaction logic for Index.xaml
    /// </summary>
    public partial class IndexPage : Page
    {
        private readonly IRoomRepository _roomRepository;
        private readonly IGuestRepository _guestRepository;
        private readonly IHotelSettingsService _hotelSettingsService;
        private readonly IBookingRepository _bookingRepository;
        private readonly ITransactionRepository _transactionRepository;
        private readonly ICardRepository _cardRepository;
        private readonly IndexPageViewModel _viewModel;
        private readonly GuestAccountService _guestAccountService;
        private readonly IVerificationCodeService _verificationCodeService;
        private readonly IApplicationUserRoleRepository _applicationUserRoleRepository;

        public IndexPage(IRoomRepository roomRepository, IGuestRepository guestRepository, IBookingRepository bookingRepository, IHotelSettingsService hotelSettingsService, ITransactionRepository transactionRepository, GuestAccountService guestAccountService, ICardRepository cardRepository, IVerificationCodeService verificationCodeService, IApplicationUserRoleRepository applicationUserRoleRepository)
        {
            _roomRepository = roomRepository;
            _guestRepository = guestRepository;
            _bookingRepository = bookingRepository;
            _hotelSettingsService = hotelSettingsService;
            _transactionRepository = transactionRepository;
            _guestAccountService = guestAccountService;
            _cardRepository = cardRepository;
            _verificationCodeService = verificationCodeService;
            _applicationUserRoleRepository = applicationUserRoleRepository;
            _viewModel = new IndexPageViewModel();
            this.DataContext = _viewModel;
            InitializeComponent();
            _cardRepository = cardRepository;
        }

        public async Task LoadRoom()
        {
            try
            {
                var rooms = await _roomRepository.GetAllRooms();

                _viewModel.Rooms.Clear();

                foreach (var room in rooms)
                    _viewModel.Rooms.Add(new SelectableRoomViewModel(room));
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async Task LoadMetrics()
        {
            try
            {
                var roomCount = await _roomRepository.GetRoomNumber();
                var guestCount = await _guestRepository.GetGuestNumber();
                var bookingCount = await _bookingRepository.GetBookingNumber();
                var inHouseGuestCount = await _guestRepository.GetInHouseGuestNumber();

                txtRoomCount.Text = roomCount.ToString("N0");
                txtGuestCount.Text = guestCount.ToString("N0");
                txtBookingCount.Text = bookingCount.ToString("N0");
                txtInHouseGuestCount.Text = inHouseGuestCount.ToString("N0");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void RoomCard_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (sender is Border border && border.Tag is SelectableRoomViewModel room)
            {
                var createCardDialog = new CreateCardDialog(room.Room, _hotelSettingsService, _verificationCodeService, _applicationUserRoleRepository, _guestAccountService, _bookingRepository, _transactionRepository)
                {
                    Owner = Window.GetWindow(this)
                };

                createCardDialog.ShowDialog();
            }
        }

        private async void Page_Loaded(object sender, RoutedEventArgs e)
        {
            await LoadRoom();
            await LoadMetrics();
        }

        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {

        }

        private async void Border_ContextMenuOpening(object sender, ContextMenuEventArgs e)
        {
            if (sender is Border border && border.ContextMenu != null && border.Tag is SelectableRoomViewModel selectedRoom )
            {
                var room = await _roomRepository.GetRoomById(selectedRoom.Room.Id);

                if(room.Status != Domain.Entities.RoomSettings.RoomStatus.Vacant)
                {
                    // Example: disable "Delete" based on condition
                    var bookItem = border.ContextMenu.Items
                        .OfType<MenuItem>().FirstOrDefault(m => m.Header.ToString() == "Check In");

                    if (bookItem != null)
                        bookItem.IsEnabled = false;
                }
               
            }
        }


        private async void BookRoom_Click(object sender, RoutedEventArgs e)
        {
            if (sender is MenuItem menu && menu.Tag is SelectableRoomViewModel room)
            {

                MessageBoxResult result = MessageBox.Show("Are you sure you want to check in this room?", "Check In Confirmation", MessageBoxButton.YesNoCancel, MessageBoxImage.Warning);

                if (result.Equals(MessageBoxResult.Yes))
                {
                    // Prepare a temporary view model with only the clicked room
                    var selectedRoomVm = new IndexPageViewModel();
                    room.IsSelected = true;
                    selectedRoomVm.Rooms.Add(room);
                    selectedRoomVm.SelectedRooms.Add(room);

                    // Open dialog with only this room selected
                    var dialog = new AddBulkBookingDialog(
                        _roomRepository,
                        _hotelSettingsService,
                        _guestRepository,
                        _transactionRepository,
                        _guestAccountService,
                        selectedRoomVm,
                        _bookingRepository,
                        _verificationCodeService,
                        _applicationUserRoleRepository
                    );

                    if (dialog.ShowDialog() == true)
                    {
                        await LoadRoom();
                    }
                } 
            }
        }

        private async void ShowRoomMenu_Click(object sender, RoutedEventArgs e)
        {
            if (sender is MenuItem menu && menu.Tag is SelectableRoomViewModel room)
            {
                var selectedRoom = await _roomRepository.GetRoomById(room.Room.Id);

                if (selectedRoom != null)
                {
                    ShowRoomCardDialog showRoomCardDialog = new ShowRoomCardDialog(_cardRepository, selectedRoom);

                    if (showRoomCardDialog.ShowDialog() == true)
                    {
                        await LoadRoom();
                    }
                }
            }
        }

        private async void ViewBillMenu_Click(object sender, RoutedEventArgs e)
        {
            if (sender is MenuItem menu && menu.Tag is SelectableRoomViewModel room)
            {
                var selectedRoom = await _roomRepository.GetRoomById(room.Room.Id);

                if (selectedRoom != null)
                {
                    RoomDetailsDialog roomDetails = new RoomDetailsDialog(_roomRepository, _transactionRepository, _bookingRepository, _hotelSettingsService, _guestAccountService, selectedRoom);

                    if (roomDetails.ShowDialog() == true)
                    {
                        await LoadRoom();
                    }
                }
            }
        }
    }
}
