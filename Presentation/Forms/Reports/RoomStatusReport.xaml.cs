using ESMART.Application.Common.Interface;
using ESMART.Application.Common.Utils;
using ESMART.Domain.Entities.RoomSettings;
using ESMART.Presentation.Forms.Export;
using ESMART.Presentation.Forms.FrontDesk.Room;
using ESMART.Presentation.Utils;
using System.Windows;
using System.Windows.Controls;

namespace ESMART.Presentation.Forms.Reports
{
    /// <summary>
    /// Interaction logic for RoomStatusReport.xaml
    /// </summary>
    public partial class RoomStatusReport : Page
    {
        private readonly IRoomRepository _roomRepository;
        private readonly ITransactionRepository _transactionRepository;
        private readonly IBookingRepository _bookingRepository;
        private readonly IHotelSettingsService _hotelSettingsService;

        public RoomStatusReport(IRoomRepository roomRepository, ICardRepository cardRepository, ITransactionRepository transactionRepository, IBookingRepository bookingRepository, IHotelSettingsService hotelSettingsService)
        {
            _roomRepository = roomRepository;
            _transactionRepository = transactionRepository;
            _bookingRepository = bookingRepository;
            _roomRepository = roomRepository;
            _hotelSettingsService = hotelSettingsService;
            InitializeComponent();
        }

        public void LoadStatus()
        {
            try
            {
                var status = Enum.GetValues(typeof(RoomStatus))
                    .Cast<RoomStatus>()
                    .Select(e => new { Id = (int)e, Name = e.ToString() })
                    .ToList();

                cmbStatus.ItemsSource = status;
                cmbStatus.DisplayMemberPath = "Name";
                cmbStatus.SelectedValuePath = "Name";
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public async Task LoadRoom(string status)
        {
            LoaderOverlay.Visibility = Visibility.Visible;
            try
            {
                var rooms = await _roomRepository.FilterByStatus(status);
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

        private async void RoomDetails_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is string Id)
            {
                var selectedRoom = (Domain.ViewModels.RoomSetting.RoomViewModel)RoomDataGrid.SelectedItem;
                if (selectedRoom.Id != null)
                {
                    var room = await _roomRepository.GetRoomById(selectedRoom.Id);

                    RoomDetailsDialog roomDetails = new RoomDetailsDialog(_roomRepository, _transactionRepository, _bookingRepository, _hotelSettingsService, room);

                    if (roomDetails.ShowDialog() == true)
                    {
                        bool isNull = Helper.AreAnyNullOrEmpty(cmbStatus.SelectedValue.ToString()!);

                        if (!isNull)
                        {
                            await LoadRoom(cmbStatus.SelectedValue.ToString()!);
                        }
                    }
                }
                else
                {
                    MessageBox.Show("Please select a guest before editing.", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
        }

        private async void cmbStatus_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            bool isNull = Helper.AreAnyNullOrEmpty(cmbStatus.SelectedValue.ToString()!);

            if (!isNull)
            {
                await LoadRoom(cmbStatus.SelectedValue.ToString()!);
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

                var optionsWindow = new ExportDialog(columnNames, RoomDataGrid, _hotelSettingsService);
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

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            LoadStatus();
        }
    }
}
