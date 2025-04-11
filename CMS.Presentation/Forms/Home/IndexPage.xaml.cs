using ESMART.Application.Interface;
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
        private readonly IndexPageViewModel _viewModel;

        public IndexPage(IRoomRepository roomRepository)
        {
            InitializeComponent();
            _roomRepository = roomRepository;
            _viewModel = new IndexPageViewModel();
            this.DataContext = _viewModel;
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

        private async void Page_Loaded(object sender, RoutedEventArgs e)
        {
            await LoadRoom();
        }
    }

}
