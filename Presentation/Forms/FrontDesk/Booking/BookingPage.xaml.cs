using ESMART.Application.Common.Interface;
using ESMART.Application.Common.Utils;
using ESMART.Domain.Entities.Data;
using ESMART.Domain.Entities.Verification;
using ESMART.Domain.ViewModels.FrontDesk;
using ESMART.Infrastructure.Services;
using ESMART.Presentation.Forms.Export;
using ESMART.Presentation.Forms.FrontDesk.Reservation;
using ESMART.Presentation.Forms.Verification;
using ESMART.Presentation.Session;
using ESMART.Presentation.Utils;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace ESMART.Presentation.Forms.FrontDesk.Booking
{
    /// <summary>
    /// Interaction logic for BookingPage.xaml
    /// </summary>
    public partial class BookingPage : Page
    {
        private readonly IBookingRepository _bookingRepository;
        private readonly IVerificationCodeService _verificationCodeService;
        private readonly IHotelSettingsService _hotelSettingsService;
        private readonly IGuestRepository _guestRepository;
        private readonly IRoomRepository _roomRepository;
        private readonly ITransactionRepository _transactionRepository;
        private readonly IReservationRepository _reservationRepository;
        private readonly IApplicationUserRoleRepository _applicationUserRoleRepository;
        private readonly IApplicationUserRoleRepository _userService;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly GuestAccountService _guestAccountService;
        private IServiceProvider _serviceProvider;

        public BookingPage(IBookingRepository bookingRepository, IVerificationCodeService verificationCodeService, IHotelSettingsService hotelSettingsService, IRoomRepository roomRepository, IGuestRepository guestRepository, ITransactionRepository transactionRepository, IReservationRepository reservationRepository, IApplicationUserRoleRepository applicationUserRoleRepository, IApplicationUserRoleRepository userService, UserManager<ApplicationUser> userManager, GuestAccountService guestAccountService )
        {
            _bookingRepository = bookingRepository;
            _verificationCodeService = verificationCodeService;
            _hotelSettingsService = hotelSettingsService;
            _roomRepository = roomRepository;
            _userService = userService;
            _userManager = userManager;
            _transactionRepository = transactionRepository;
            _guestRepository = guestRepository;
            _reservationRepository = reservationRepository;
            _applicationUserRoleRepository = applicationUserRoleRepository;
            InitializeComponent();            
            _guestAccountService = guestAccountService;

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

        private async Task LoadBooking()
        {
            try
            {
                LoaderOverlay.Visibility = Visibility.Visible;
                var allBooking = await _bookingRepository.GetAllBookingsAsync();

                if (allBooking != null)
                {
                    BookingDataGrid.ItemsSource = allBooking;
                    txtBookingCount.Text = allBooking.Count.ToString();
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

        private async void AddSingleBooking_Click(object sender, RoutedEventArgs e)
        {
            InitializeServices();

            AddBookingDialog addBookingDialog = _serviceProvider.GetRequiredService<AddBookingDialog>();
            if (addBookingDialog.ShowDialog() == true)
            {
                await LoadBooking();
            }
        }

        private async void AddBulkBooking_Click(object sender, RoutedEventArgs e)
        {
            InitializeServices();

            AddBulkBookingDialog addBulkBookingDialog = _serviceProvider.GetRequiredService<AddBulkBookingDialog>();
            if (addBulkBookingDialog.ShowDialog() == true)
            {
                await LoadBooking();
            }
        }

        private async void IssueCard_Click(object sender, RoutedEventArgs e)
        {
            LoaderOverlay.Visibility = Visibility.Visible;
            try
            {
                if (sender is Button button && button.Tag is string Id)
                {
                    var selectedBooking = (BookingViewModel)BookingDataGrid.SelectedItem;
                    var activeUser = await _applicationUserRoleRepository.GetUserById(AuthSession.CurrentUser!.Id);
                    if (selectedBooking.Id != null)
                    {
                        var booking = await _bookingRepository.GetBookingById(selectedBooking.Id);

                        var issueCard = new IssueCardDialog(_bookingRepository, _guestRepository, booking, _hotelSettingsService);
                        if (issueCard.ShowDialog() == true)
                        {
                            await LoadBooking();
                        }
                    }
                    else
                    {
                        MessageBox.Show("Please select a booking before issuing card.", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
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

        private async void ExtendStay_Click(object sender, RoutedEventArgs e)
        {
            LoaderOverlay.Visibility = Visibility.Visible;
            try
            {
                if (sender is Button button && button.Tag is string Id)
                {
                    var selectedBooking = (BookingViewModel)BookingDataGrid.SelectedItem;
                    var activeUser = await _applicationUserRoleRepository.GetUserById(AuthSession.CurrentUser!.Id);
                    if (selectedBooking.Id != null)
                    {
                        var booking = await _bookingRepository.GetBookingById(selectedBooking.Id);

                        var extendStaayDialog = new ExtendStayDialog(_guestRepository, _roomRepository, _hotelSettingsService, _bookingRepository, _verificationCodeService, _transactionRepository, booking, _reservationRepository, _applicationUserRoleRepository);
                        if (extendStaayDialog.ShowDialog() == true)
                        {
                            await LoadBooking();
                        }
                    }
                    else
                    {
                        MessageBox.Show("Please select a booking before issuing card.", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
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

        private async void TransferGuest_Click(object sender, RoutedEventArgs e)
        {
            LoaderOverlay.Visibility = Visibility.Visible;
            try
            {
                if (sender is Button button && button.Tag is string Id)
                {
                    var selectedBooking = (BookingViewModel)BookingDataGrid.SelectedItem;
                    var activeUser = await _applicationUserRoleRepository.GetUserById(AuthSession.CurrentUser!.Id);
                    if (selectedBooking.Id != null)
                    {
                        var booking = await _bookingRepository.GetBookingById(selectedBooking.Id);
                        var transferGuestDialog = new TransferGuestDialog(_guestRepository, _roomRepository, _hotelSettingsService, _bookingRepository, _verificationCodeService, _transactionRepository, booking, _reservationRepository, _applicationUserRoleRepository);
                        if (transferGuestDialog.ShowDialog() == true)
                        {
                            await LoadBooking();
                        }
                    }
                    else
                    {
                        MessageBox.Show("Please select a booking before issuing card.", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
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
                        var bookingDetailsDialog = new ViewBookingDetailsDialog(booking, _transactionRepository, _bookingRepository, _hotelSettingsService, _guestAccountService);
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

        private async void CancelBooking_Click(object sender, RoutedEventArgs e)
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
                        MessageBoxResult messageResult = MessageBox.Show("Are you sure you want to check out this booking?", "Confirmation", MessageBoxButton.YesNo, MessageBoxImage.Warning);
                        if (messageResult == MessageBoxResult.Yes)
                        {
                            var unPaidTransaction = await _transactionRepository.GetUnpaidTransactionItemsByGuestIdAsync(booking.GuestId);

                            var totalAmount = unPaidTransaction.Sum(ut => ut.BillPost);

                            if (totalAmount > 0)
                            {
                                var checkOutBooking = new CheckOutBooking(booking, booking.Guest, totalAmount, _reservationRepository, _transactionRepository, _hotelSettingsService, _verificationCodeService, _applicationUserRoleRepository, _bookingRepository, _guestRepository, _roomRepository);
                                if (checkOutBooking.ShowDialog() == true)
                                {
                                    MessageBox.Show("Successfully checkout guest", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                                }
                                
                                await LoadBooking();
                            }
                            else
                            {
                                booking.Status = Domain.Enum.BookingStatus.Completed;
                                booking.IsTrashed = true;
                                await _bookingRepository.UpdateBooking(booking);

                                booking.Room.Status = Domain.Entities.RoomSettings.RoomStatus.Dirty;
                                await _roomRepository.UpdateRoom(booking.Room);

                                MessageBox.Show("Successfully checkout guest", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                                await LoadBooking();
                            }
                        }
                    }

                    else
                    {
                        MessageBox.Show("Please select a booking before checking out.", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
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

        private async void DeleteBooking_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (sender is Button button && button.Tag is string Id)
                {
                    var selectedBooking = (BookingViewModel)BookingDataGrid.SelectedItem;

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
                                if (selectedBooking.Id != null)
                                {
                                    MessageBoxResult messageResult = MessageBox.Show("Are you sure you want to delete this booking?", "Confirmation", MessageBoxButton.YesNo, MessageBoxImage.Warning);
                                    if (messageResult == MessageBoxResult.Yes)
                                    {
                                        LoaderOverlay.Visibility = Visibility.Visible;
                                        await _bookingRepository.DeleteBooking(selectedBooking.Id);

                                        var room = await _roomRepository.GetRoomByNumber(selectedBooking.Room);
                                        if (room != null)
                                        {
                                            room.Status = Domain.Entities.RoomSettings.RoomStatus.Vacant;
                                            await _roomRepository.UpdateRoom(room);
                                        }

                                        await LoadBooking();
                                    }
                                }
                                else
                                {
                                    MessageBox.Show("Please select a booking before deleting.", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                                }
                            }
                        }
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

                var optionsWindow = new ExportDialog(columnNames, BookingDataGrid, _hotelSettingsService, "All Active Booking");
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

        //private async Task<bool> VerifyPayment(Domain.Entities.FrontDesk.Booking booking, ApplicationUser activeUser)
        //{
        //    var hotel = await _hotelSettingsService.GetHotelInformation();
        //    var bookingAccount = await _transactionRepository.GetBankAccountById(booking.AccountNumber);

        //    if (booking.Status != Domain.Enum.BookingStatus.Completed)
        //    {
        //        var verificationCode = new VerificationCode()
        //        {
        //            Code = string.Concat("BK", Guid.NewGuid().ToString().Split("-")[0].ToUpper().AsSpan(0, 5)),
        //            ServiceId = booking.BookingId,
        //            ApplicationUserId = AuthSession.CurrentUser?.Id
        //        };

        //        await _verificationCodeService.AddCode(verificationCode);

        //        if (hotel != null)
        //        {
        //            var response = await SenderHelper.SendOtp(
        //                hotel.PhoneNumber,
        //                hotel.Name,
        //                $"{bookingAccount.BankAccountNumber} ({bookingAccount.BankName}) | {bookingAccount.BankAccountName}",
        //                booking.Guest.FullName,
        //                "Booking",
        //                verificationCode.Code,
        //                booking.Balance,
        //                booking.PaymentMethod.ToString(),
        //                activeUser.FullName!,
        //                activeUser.PhoneNumber!
        //            );

        //            if (response.IsSuccessStatusCode)
        //            {
        //                MessageBox.Show("Kindly verify booking payment", "Code resent", MessageBoxButton.OK, MessageBoxImage.Information);

        //                VerifyPaymentWindow verifyPaymentWindow = new(
        //                    _verificationCodeService,
        //                    _hotelSettingsService,
        //                    _bookingRepository,
        //                    _transactionRepository,
        //                    booking.BookingId,
        //                    booking.Balance,
        //                    _applicationUserRoleRepository
        //                );

        //                if (verifyPaymentWindow.ShowDialog() == true)
        //                {
        //                    booking.Status = Domain.Enum.BookingStatus.Completed;
        //                    booking.Balance = 0;
        //                    await _bookingRepository.UpdateBooking(booking);

        //                    var transaction = await _transactionRepository.GetUnpaidTransactionItemsByServiceIdAsync(booking.BookingId, booking.GuestId, booking.Balance);

        //                    if(transaction != null)
        //                    {
        //                        transaction.Status = Domain.Enum.TransactionStatus.Paid;
        //                        await _transactionRepository.UpdateTransactionItemAsync(transaction);
        //                    }

        //                    return true;
        //                }
        //            }
        //            else
        //            {
        //                MessageBox.Show(
        //                    $"An error ocurred when sending code. This might be caused by network related issues or otp sender service.",
        //                    "Error",
        //                    MessageBoxButton.OKCancel,
        //                    MessageBoxImage.Error
        //                );

        //                return false;
        //            }
        //        }
        //    }

        //    return true;

        //}

        private async void Page_Loaded(object sender, RoutedEventArgs e)
        {
            await LoadBooking();
        }

        private async void txtSearchBuilding_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            bool isNull = Helper.AreAnyNullOrEmpty(txtSearchBuilding.Text);
            if (isNull)
            {
                await LoadBooking();
            }
        }

        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            bool isNull = Helper.AreAnyNullOrEmpty(txtSearchBuilding.Text);
            if (isNull)
            {
                await LoadBooking();
            }
            else
            {
                var searchText = txtSearchBuilding.Text.ToLower();
                var filteredBookings = await _bookingRepository.SearchBooking(searchText);

                if (filteredBookings == null || filteredBookings.Count == 0)
                {
                    await LoadBooking();
                }
                BookingDataGrid.ItemsSource = filteredBookings;
            }
        }

        private async void BookingDataGridRow_DoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (sender is DataGridRow row && row.Item is Domain.ViewModels.FrontDesk.BookingViewModel selectedBooking)
            {

                var booking = await _bookingRepository.GetBookingById(selectedBooking.Id);

                if (selectedBooking.Id != null)
                {
                    var bookingDetailsDialog = new ViewBookingDetailsDialog(booking, _transactionRepository, _bookingRepository, _hotelSettingsService, _guestAccountService);
                    bookingDetailsDialog.ShowDialog();
                }
            }
        }
    }
}
