using ESMART.Application.Common.Dtos;
using ESMART.Application.Common.Interface;
using ESMART.Application.Common.Utils;
using ESMART.Domain.Entities.Data;
using ESMART.Domain.Entities.FrontDesk;
using ESMART.Infrastructure.Services;
using ESMART.Presentation.Forms.Export;
using ESMART.Presentation.Session;
using ESMART.Presentation.Utils;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;


namespace ESMART.Presentation.Forms.FrontDesk.Guest
{
    public partial class GuestPage : Page
    {
        private readonly IGuestRepository _guestRepository;
        private readonly ITransactionRepository _transactionRepository;
        private readonly IHotelSettingsService _hotelSettingsService;
        private readonly IBookingRepository _bookingRepository;
        private readonly IApplicationUserRoleRepository _userService;
        private readonly GuestAccountService _guestAccountService;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IRoomRepository _roomRepository;
        private IServiceProvider _serviceProvider;

        public GuestPage(IGuestRepository guestRepository, ITransactionRepository transactionRepository, IHotelSettingsService hotelSettingsService, IApplicationUserRoleRepository userService, UserManager<ApplicationUser> userManager, IBookingRepository bookingRepository, GuestAccountService guestAccountService, IRoomRepository roomRepository)
        {
            _guestRepository = guestRepository;
            _transactionRepository = transactionRepository;
            _userService = userService;
            _userManager = userManager;
            _bookingRepository = bookingRepository;
            _guestAccountService = guestAccountService;
            _roomRepository = roomRepository;
            _hotelSettingsService = hotelSettingsService;
            InitializeComponent();
        }

        private void InitializeServices()
        {
            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .Build();

            var services = new ServiceCollection();
            DependencyInjection.ConfigureServices(services, configuration);
            _serviceProvider = services.BuildServiceProvider();
        }

        public async Task LoadGuests()
        {
            LoaderOverlay.Visibility = Visibility.Visible;
            try
            {
                var guests = await _guestAccountService.GetInHouseGuestAsync();
                GuestDataGrid.ItemsSource = guests;
                txtGuestCount.Text = guests.Count.ToString();
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

        private async void AddGuest_Click(object sender, RoutedEventArgs e)
        {
            InitializeServices();

            AddGuestDialog addGuestDialog = _serviceProvider.GetRequiredService<AddGuestDialog>();
            if (addGuestDialog.ShowDialog() == true)
            {
                await LoadGuests();
            }
        }

        private async void SearchGuest_Click(object sender, RoutedEventArgs e)
        {
            LoaderOverlay.Visibility = Visibility.Visible;
            try
            {
                var searchText = txtSearch.Text.Trim();
                if (string.IsNullOrEmpty(searchText) || searchText == "Search")
                {
                    await LoadGuests();
                    return;
                }
                var guests = await _guestRepository.SearchGuestAsync(searchText);
                GuestDataGrid.ItemsSource = guests;
                txtGuestCount.Text = guests.Count.ToString();
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

        private async void GuestDataGrid_Loaded(object sender, RoutedEventArgs e)
        {
            await LoadGuests();
        }

        private void EditGuest_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is string Id)
            {
                var selectedGuest = (Domain.Entities.FrontDesk.Guest)GuestDataGrid.SelectedItem;

                if (selectedGuest.Id != null)
                {
                    UpdateGuestDialog updateGuestDialog = new UpdateGuestDialog(selectedGuest.Id, _guestRepository);
                    if (updateGuestDialog.ShowDialog() == true)
                    {
                        _ = LoadGuests();
                    }
                }
                else
                {
                    MessageBox.Show("Please select a guest before editing.", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
        }

        private void FundAccount_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is string Id)
            {
                var selectedGuest = (Domain.Entities.FrontDesk.Guest)GuestDataGrid.SelectedItem;
                if (selectedGuest.Id != null)
                {
                    FundGuestAccountDialog fundGuestAccountDialog = new FundGuestAccountDialog(selectedGuest, _guestRepository, _bookingRepository, _transactionRepository, _guestAccountService);
                    if (fundGuestAccountDialog.ShowDialog() == true)
                    {
                        _ = LoadGuests();
                    }
                }
                else
                {
                    MessageBox.Show("Please select a guest before funding their account.", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
        }

        private async void ViewGuest_Click(Object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is string Id)
            {
                var selectedGuest = (Domain.Entities.FrontDesk.Guest)GuestDataGrid.SelectedItem;
                if (selectedGuest.Id != null)
                {
                    GuestDetailsDialog viewGuestDialog = new GuestDetailsDialog(selectedGuest.Id, _guestRepository, _transactionRepository, _hotelSettingsService, _bookingRepository, _guestAccountService);
                    if (viewGuestDialog.ShowDialog() == true)
                    {
                        await LoadGuests();
                    }
                }
                else
                {
                    MessageBox.Show("Please select a guest before viewing.", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
        }

        private async void DeleteGuest_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is string Id)
            {
                var userId = AuthSession.CurrentUser?.Id;

                if (userId != null)
                {
                    var user = await _userService.GetUserById(userId);
                    if (user != null)
                    {
                        bool isAdmin = await _userManager.IsInRoleAsync(user, DefaultRoles.Administrator.ToString()) ||
                                        await _userManager.IsInRoleAsync(user, DefaultRoles.Admin.ToString()) ||
                                        await _userManager.IsInRoleAsync(user, DefaultRoles.Manager.ToString());
                        if (!isAdmin)
                        {
                            MessageBox.Show("You are not authorized to perform this action", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                        else
                        {
                            var selectedGuest = (Domain.Entities.FrontDesk.Guest)GuestDataGrid.SelectedItem;
                            if (selectedGuest.Id != null)
                            {
                                LoaderOverlay.Visibility = Visibility.Visible;
                                try
                                {
                                    MessageBoxResult result = MessageBox.Show("Are you sure you want to delete this guest?", "Confirmation", MessageBoxButton.YesNo, MessageBoxImage.Warning);
                                    if (result == MessageBoxResult.Yes)
                                    {
                                        await _guestRepository.DeleteGuestAsync(selectedGuest.Id);
                                        await LoadGuests();
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
                            else
                            {
                                MessageBox.Show("Please select a guest before deleting.", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                            }
                        }
                    }
                }

            }
        }

        private async void ExportButton_Click(object sender, RoutedEventArgs e)
        {
            LoaderOverlay.Visibility = Visibility.Visible;
            try
            {
                var columnNames = GuestDataGrid.Columns
                    .Where(c => c.Header != null)
                    .Select(c => c.Header.ToString())
                    .Where(name => !string.IsNullOrWhiteSpace(name) && name != "Operation")
                    .ToList();

                var optionsWindow = new ExportDialog(columnNames, GuestDataGrid, _hotelSettingsService, "All Guest Data");
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
                            ExportHelper.ExportAndPrint(GuestDataGrid, exportResult.SelectedColumns, exportResult.ExportFormat, exportResult.FileName, hotel.LogoUrl!, hotel.Name, hotel.Email, hotel.PhoneNumber, hotel.Address);
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

        private void txtSearch_GotFocus(object sender, RoutedEventArgs e)
        {
            if (txtSearch.Text == "Search")
            {
                txtSearch.Text = "";
                txtSearch.Foreground = Brushes.Black;
            }
        }

        private void txtSearch_LostFocus(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtSearch.Text))
            {
                txtSearch.Text = "Search";
                txtSearch.Foreground = Brushes.Gray;
            }
        }

        private async void GuestDataGridRow_DoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (sender is DataGridRow row && row.Item is InHouseGuest guest)
            {
                if (guest != null)
                {
                    GuestDetailsDialog viewGuestDialog = new GuestDetailsDialog(guest.GuestId, _guestRepository, _transactionRepository, _hotelSettingsService, _bookingRepository, _guestAccountService);
                    if (viewGuestDialog.ShowDialog() == true)
                    {
                        await LoadGuests();
                    }
                }
                else
                {
                    MessageBox.Show("Please select a guest before viewing.", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
        }

        private async void GuestDataGridRow_DoubleClick2(object sender, MouseButtonEventArgs e)
        {
            if (sender is DataGridRow row && row.Item is Domain.Entities.FrontDesk.Guest guest)
            {
                if (guest != null)
                {
                    GuestDetailsDialog viewGuestDialog = new GuestDetailsDialog(guest.Id, _guestRepository, _transactionRepository, _hotelSettingsService, _bookingRepository, _guestAccountService);
                    if (viewGuestDialog.ShowDialog() == true)
                    {
                        await LoadGuests();
                    }
                }
                else
                {
                    MessageBox.Show("Please select a guest before viewing.", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
        }

        private async void InHouseGuest_Click(object sender, RoutedEventArgs e)
        {
            LoaderOverlay.Visibility = Visibility.Visible;
            try
            {
                GuestDataGrid.Visibility = Visibility.Collapsed;
                AllGuestDataGrid.Visibility = Visibility.Visible;

                var guests = await _guestRepository.GetAllGuestsAsync();
                AllGuestDataGrid.ItemsSource = guests;
                txtGuestCount.Text = guests.Count.ToString();
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

        private async void InHouse_Click(object sender, RoutedEventArgs e)
        {
            LoaderOverlay.Visibility = Visibility.Visible;
            try
            {
                AllGuestDataGrid.Visibility = Visibility.Collapsed;
                GuestDataGrid.Visibility = Visibility.Visible;

                var guests = await _guestAccountService.GetInHouseGuestAsync();
                GuestDataGrid.ItemsSource = guests;
                txtGuestCount.Text = guests.Count.ToString();
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

        private async void GuestDataGrid_CellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
        {
            try
            {
                if (e.Row.Item is Application.Common.Dtos.InHouseGuest editedItem)
                {
                    var availableRoomsTask = _roomRepository.GetAvailableRooms();
                    var roomTask = _roomRepository.GetRoomByNumber(editedItem.RoomNumber);
                    var vatSettingTask = _hotelSettingsService.GetSettingAsync("VAT");
                    var serviceChargeSettingTask = _hotelSettingsService.GetSettingAsync("ServiceCharge");
                    var roomBookingTask = _guestAccountService.GetRoomBookingByRoomIdAsync(editedItem.RoomId);

                    await Task.WhenAll(availableRoomsTask, roomTask, vatSettingTask, serviceChargeSettingTask, roomBookingTask);

                    var availableRooms = availableRoomsTask.Result;
                    var room = roomTask.Result;
                    var vatSetting = vatSettingTask.Result;
                    var serviceChargeSetting = serviceChargeSettingTask.Result;
                    var roomBooking = roomBookingTask.Result;

                    if (room == null || roomBooking == null)
                    {
                        MessageBox.Show("Invalid room or booking details!", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                        return;
                    }

                    if (!decimal.TryParse(vatSetting?.Value, out decimal vat) || !decimal.TryParse(serviceChargeSetting?.Value, out decimal serviceCharge))
                    {
                        MessageBox.Show("Invalid VAT or Service Charge settings!", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                        return;
                    }

                    var discount = Helper.FindPercentage(room.Rate, editedItem.Discount);
                    var (rack, discountPrice, serviceFeeAmount, tax, final) = Helper.CalculateRackAndDiscountedTotal(room.Rate, vat, serviceCharge, discount);

                    UpdateRoomBooking(roomBooking, editedItem, rack, discountPrice, serviceFeeAmount, tax);
                    await _guestAccountService.UpdateRoomBookingAsync(roomBooking);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error updating record: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void UpdateRoomBooking(RoomBooking roomBooking, Application.Common.Dtos.InHouseGuest guest, decimal rack, decimal discountPrice, decimal serviceFeeAmount, decimal tax)
        {
            roomBooking.CheckOut = guest.CheckOutDate;
            roomBooking.CheckIn = guest.CheckInDate;
            roomBooking.OccupantName = guest.GuestName;
            roomBooking.RoomId = guest.RoomId;
            roomBooking.OccupantPhoneNumber = guest.PhoneNumber;
            roomBooking.Rate = rack;
            roomBooking.Tax = tax;
            roomBooking.Discount = discountPrice;
            roomBooking.ServiceCharge = serviceFeeAmount;
        }

        private async void Guest_Loaded(object sender, RoutedEventArgs e)
        {
            await LoadGuests();
        }
    }
}
