using ESMART.Application.Common.Interface;
using ESMART.Infrastructure.Repositories.Configuration;
using ESMART.Presentation.Forms.Export;
using ESMART.Presentation.Forms.RoomSetting.Room;
using ESMART.Presentation.Utils;
using Microsoft.Extensions.DependencyInjection;
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

namespace ESMART.Presentation.Forms.FrontDesk.Room
{
    /// <summary>
    /// Interaction logic for RoomPage.xaml
    /// </summary>
    public partial class RoomPage : Page
    {
        private readonly IRoomRepository _roomRepository;
        private readonly ITransactionRepository _transactionRepository;
        private readonly IBookingRepository _bookingRepository;
        private readonly IHotelSettingsService _hotelSettingsService;
        private readonly ICardRepository _cardRepository;
        public RoomPage(IRoomRepository roomRepository, ICardRepository cardRepository, ITransactionRepository transactionRepository, IBookingRepository bookingRepository, IHotelSettingsService hotelSettingsService)
        {
            _roomRepository = roomRepository;
            _cardRepository = cardRepository;
            _transactionRepository = transactionRepository;
            _bookingRepository = bookingRepository;
            _roomRepository = roomRepository;
            _hotelSettingsService = hotelSettingsService;
            InitializeComponent();
        }

        public async Task LoadRoom()
        {
            LoaderOverlay.Visibility = Visibility.Visible;
            try
            {
                var rooms = await _roomRepository.GetAllRooms();
                RoomDataGrid.ItemsSource = rooms;
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

        private async void AddRoomButton_Click(object sender, RoutedEventArgs e)
        {
            var services = new ServiceCollection();
            DependencyInjection.ConfigureServices(services);
            var serviceProvider = services.BuildServiceProvider();
            AddRoomDialog addRoom = serviceProvider.GetRequiredService<AddRoomDialog>();
            if (addRoom.ShowDialog() == true)
            {
                await LoadRoom();
            }
        }

        private async void EditRoomButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is string Id)
            {
                var selectedRoom = (Domain.Entities.RoomSettings.Room)RoomDataGrid.SelectedItem;
                if (selectedRoom.Id != null)
                {
                    var room = await _roomRepository.GetRoomById(selectedRoom.Id);

                    UpdateRoomDialog updateRoomDialog = new(_roomRepository, room);

                    if (updateRoomDialog.ShowDialog() == true)
                    {
                        await LoadRoom();
                    }
                }
                else
                {
                    MessageBox.Show("Please select a guest before editing.", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
        }

        private async void DeleteRoom_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (sender is Button button && button.Tag is string Id)
                {
                    var selectedRoom = (Domain.Entities.RoomSettings.Room)RoomDataGrid.SelectedItem;
                    if (selectedRoom.Id != null)
                    {
                        MessageBoxResult messageResult = MessageBox.Show("Are you sure you want to delete this room?", "Confirmation", MessageBoxButton.YesNo, MessageBoxImage.Warning);
                        if (messageResult == MessageBoxResult.Yes)
                        {
                            LoaderOverlay.Visibility = Visibility.Visible;

                            await _roomRepository.DeleteRoom(selectedRoom.Id);
                            await LoadRoom();
                        }
                    }
                    else
                    {
                        MessageBox.Show("Please select a guest before deleting.", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
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

        private async void ShowRoom_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is string Id)
            {
                var selectedRoom = (Domain.Entities.RoomSettings.Room)RoomDataGrid.SelectedItem;
                if (selectedRoom.Id != null)
                {
                    var room = await _roomRepository.GetRoomById(selectedRoom.Id);

                    ShowRoomCardDialog showRoomCardDialog = new ShowRoomCardDialog(_cardRepository, room);

                    if (showRoomCardDialog.ShowDialog() == true)
                    {
                        await LoadRoom();
                    }
                }
                else
                {
                    MessageBox.Show("Please select a guest before editing.", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
        }

        private async void RoomDetails_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is string Id)
            {
                var selectedRoom = (Domain.Entities.RoomSettings.Room)RoomDataGrid.SelectedItem;
                if (selectedRoom.Id != null)
                {
                    var room = await _roomRepository.GetRoomById(selectedRoom.Id);

                    RoomDetailsDialog roomDetails = new RoomDetailsDialog(_roomRepository, _transactionRepository, _bookingRepository, _hotelSettingsService, room);

                    if (roomDetails.ShowDialog() == true)
                    {
                        await LoadRoom();
                    }
                }
                else
                {
                    MessageBox.Show("Please select a guest before editing.", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
        }

        private async Task LoadMetrics()
        {
            try
            {
                var roomCount = await _roomRepository.GetRoomNumber();

                txtRoomCount.Text = roomCount.ToString("N0");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async void ExportButton_Click(object sender, RoutedEventArgs e)
        {
            LoaderOverlay.Visibility = Visibility.Visible;
            try
            {
                var columnNames = RoomDataGrid.Columns
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
                            ExportHelper.ExportAndPrint(RoomDataGrid, exportResult.SelectedColumns, exportResult.ExportFormat, exportResult.FileName, hotel.LogoUrl!, hotel.Name, hotel.Email, hotel.PhoneNumber, hotel.Address);
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
            await LoadMetrics();
            await LoadRoom();
        }
    }
}
