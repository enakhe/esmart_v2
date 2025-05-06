using ESMART.Application.Common.Interface;
using System.Windows;
using System.Windows.Controls;

namespace ESMART.Presentation.Forms.Home
{
    /// <summary>
    /// Interaction logic for Index.xaml
    /// </summary>
    public partial class IndexPage : Page
    {
        private readonly IRoomRepository _roomRepository;
        private readonly IGuestRepository _guestRepository;
        private readonly IBookingRepository _bookingRepository;
        private readonly IndexPageViewModel _viewModel;

        public IndexPage(IRoomRepository roomRepository, IGuestRepository guestRepository, IBookingRepository bookingRepository)
        {
            _roomRepository = roomRepository;
            _guestRepository = guestRepository;
            _bookingRepository = bookingRepository;
            _viewModel = new IndexPageViewModel();
            this.DataContext = _viewModel;
            InitializeComponent();
        }

        public async Task LoadRoom()
        {
            try
            {
                var rooms = await _roomRepository.GetAllRooms();
                _viewModel.Rooms.Clear();
                foreach (var room in rooms)
                    _viewModel.Rooms.Add(room);
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

        private async void Page_Loaded(object sender, RoutedEventArgs e)
        {
            await LoadRoom();
            await LoadMetrics();
        }
    }
}
