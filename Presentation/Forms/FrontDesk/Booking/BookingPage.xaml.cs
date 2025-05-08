using ESMART.Application.Common.Interface;
using ESMART.Domain.Entities.Verification;
using ESMART.Domain.ViewModels.FrontDesk;
using ESMART.Presentation.Forms.Export;
using ESMART.Presentation.Forms.Verification;
using ESMART.Presentation.Session;
using ESMART.Presentation.Utils;
using Microsoft.Extensions.DependencyInjection;
using System.Windows;
using System.Windows.Controls;

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
        public BookingPage(IBookingRepository bookingRepository, IVerificationCodeService verificationCodeService, IHotelSettingsService hotelSettingsService, IRoomRepository roomRepository, IGuestRepository guestRepository, ITransactionRepository transactionRepository, IReservationRepository reservationRepository, IApplicationUserRoleRepository applicationUserRoleRepository)
        {
            _bookingRepository = bookingRepository;
            _verificationCodeService = verificationCodeService;
            _hotelSettingsService = hotelSettingsService;
            _roomRepository = roomRepository;
            _transactionRepository = transactionRepository;
            _guestRepository = guestRepository;
            _reservationRepository = reservationRepository;
            _applicationUserRoleRepository = applicationUserRoleRepository;
            InitializeComponent();
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
            var services = new ServiceCollection();
            DependencyInjection.ConfigureServices(services);
            var serviceProvider = services.BuildServiceProvider();

            AddBookingDialog addBookingDialog = serviceProvider.GetRequiredService<AddBookingDialog>();
            if (addBookingDialog.ShowDialog() == true)
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
                        var hotel = await _hotelSettingsService.GetHotelInformation();
                        var booking = await _bookingRepository.GetBookingById(selectedBooking.Id);
                        if (booking.Status != Domain.Enum.BookingStatus.Completed)
                        {
                            var verificationCode = new VerificationCode()
                            {
                                Code = string.Concat("BK", Guid.NewGuid().ToString().Split("-")[0].ToUpper().AsSpan(0, 5)),
                                ServiceId = booking.BookingId,
                                ApplicationUserId = AuthSession.CurrentUser?.Id
                            };

                            await _verificationCodeService.AddCode(verificationCode);

                            if (hotel != null)
                            {
                                var response = await SenderHelper.SendOtp(hotel.PhoneNumber, hotel.Name, booking.AccountNumber, booking.Guest.FullName, "Booking", verificationCode.Code, booking.Receivables, booking.PaymentMethod.ToString(), activeUser.FullName!, activeUser.PhoneNumber!);

                                if (response.IsSuccessStatusCode)
                                {
                                    MessageBox.Show("Kindly verify booking payment", "Code resent", MessageBoxButton.OK, MessageBoxImage.Information);

                                    VerifyPaymentWindow verifyPaymentWindow = new(_verificationCodeService, _hotelSettingsService, _bookingRepository, _transactionRepository, booking.BookingId, booking.Receivables, _applicationUserRoleRepository);
                                    if (verifyPaymentWindow.ShowDialog() == true)
                                    {
                                        booking.Status = Domain.Enum.BookingStatus.Completed;
                                        booking.Receivables = 0;
                                        await _bookingRepository.UpdateBooking(booking);
                                        IssueCardDialog issueCardDialog = new IssueCardDialog(_bookingRepository, _guestRepository, booking, _hotelSettingsService);
                                        if (issueCardDialog.ShowDialog() == true)
                                        {
                                            await LoadBooking();
                                        }
                                    }
                                }
                                else
                                {
                                    MessageBox.Show($"An error ocurred when sending code. This might be caused by network related issues or otp sender service.", "Error", MessageBoxButton.OKCancel, MessageBoxImage.Error);
                                }
                            }
                        }
                        else
                        {
                            IssueCardDialog issueCardDialog = new IssueCardDialog(_bookingRepository, _guestRepository, booking, _hotelSettingsService);
                            if (issueCardDialog.ShowDialog() == true)
                            {
                                await LoadBooking();
                            }
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
                        var hotel = await _hotelSettingsService.GetHotelInformation();
                        var booking = await _bookingRepository.GetBookingById(selectedBooking.Id);

                        if (booking != null)
                        {
                            if (booking.Status != Domain.Enum.BookingStatus.Completed)
                            {
                                var verificationCode = new VerificationCode()
                                {
                                    Code = string.Concat("BK", Guid.NewGuid().ToString().Split("-")[0].ToUpper().AsSpan(0, 5)),
                                    ServiceId = booking.BookingId,
                                    ApplicationUserId = AuthSession.CurrentUser?.Id
                                };

                                await _verificationCodeService.AddCode(verificationCode);

                                if (hotel != null)
                                {
                                    var response = await SenderHelper.SendOtp(hotel.PhoneNumber, hotel.Name, booking.AccountNumber, booking.Guest.FullName, "Booking", verificationCode.Code, booking.Receivables, booking.PaymentMethod.ToString(), activeUser.FullName!, activeUser.PhoneNumber!);

                                    if (response.IsSuccessStatusCode)
                                    {
                                        MessageBox.Show("Kindly verify booking payment", "Code resent", MessageBoxButton.OK, MessageBoxImage.Information);

                                        VerifyPaymentWindow verifyPaymentWindow = new(_verificationCodeService, _hotelSettingsService, _bookingRepository, _transactionRepository, booking.BookingId, booking.Receivables, _applicationUserRoleRepository);
                                        if (verifyPaymentWindow.ShowDialog() == true)
                                        {
                                            booking.Status = Domain.Enum.BookingStatus.Completed;
                                            booking.Receivables = 0;
                                            await _bookingRepository.UpdateBooking(booking);

                                            ExtendStayDialog extendStayDialog = new ExtendStayDialog(_guestRepository, _roomRepository, _hotelSettingsService, _bookingRepository, _verificationCodeService, _transactionRepository, booking, _reservationRepository, _applicationUserRoleRepository);
                                            if (extendStayDialog.ShowDialog() == true)
                                            {
                                                IssueCardDialog issueCardDialog = new IssueCardDialog(_bookingRepository, _guestRepository, booking, _hotelSettingsService);
                                                if (issueCardDialog.ShowDialog() == true)
                                                {
                                                    await LoadBooking();
                                                }
                                            }
                                        }
                                    }
                                    else
                                    {
                                        MessageBox.Show($"An error ocurred when sending code. This might be caused by network related issues or otp sender service.", "Error", MessageBoxButton.OKCancel, MessageBoxImage.Error);
                                    }
                                }
                            }
                            else
                            {
                                ExtendStayDialog extendStayDialog = new ExtendStayDialog(_guestRepository, _roomRepository, _hotelSettingsService, _bookingRepository, _verificationCodeService, _transactionRepository, booking, _reservationRepository, _applicationUserRoleRepository);
                                if (extendStayDialog.ShowDialog() == true)
                                {
                                    IssueCardDialog issueCardDialog = new IssueCardDialog(_bookingRepository, _guestRepository, booking, _hotelSettingsService);
                                    if (issueCardDialog.ShowDialog() == true)
                                    {
                                        await LoadBooking();
                                    }
                                }
                            }
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
                        var hotel = await _hotelSettingsService.GetHotelInformation();
                        var booking = await _bookingRepository.GetBookingById(selectedBooking.Id);

                        if (booking != null)
                        {
                            if (booking.Status != Domain.Enum.BookingStatus.Completed)
                            {
                                var verificationCode = new VerificationCode()
                                {
                                    Code = string.Concat("BK", Guid.NewGuid().ToString().Split("-")[0].ToUpper().AsSpan(0, 5)),
                                    ServiceId = booking.BookingId,
                                    ApplicationUserId = AuthSession.CurrentUser?.Id
                                };

                                await _verificationCodeService.AddCode(verificationCode);

                                if (hotel != null)
                                {
                                    var response = await SenderHelper.SendOtp(hotel.PhoneNumber, hotel.Name, booking.AccountNumber, booking.Guest.FullName, "Booking", verificationCode.Code, booking.TotalAmount, booking.PaymentMethod.ToString(), activeUser.FullName!, activeUser.PhoneNumber!);
                                    if (response.IsSuccessStatusCode)
                                    {
                                        MessageBox.Show("Kindly verify booking payment", "Code resent", MessageBoxButton.OK, MessageBoxImage.Information);

                                        VerifyPaymentWindow verifyPaymentWindow = new(_verificationCodeService, _hotelSettingsService, _bookingRepository, _transactionRepository, booking.BookingId, booking.Receivables, _applicationUserRoleRepository);
                                        if (verifyPaymentWindow.ShowDialog() == true)
                                        {
                                            booking.Status = Domain.Enum.BookingStatus.Completed;
                                            await _bookingRepository.UpdateBooking(booking);

                                            TransferGuestDialog transferGuestDialog = new TransferGuestDialog(_guestRepository, _roomRepository, _hotelSettingsService, _bookingRepository, _verificationCodeService, _transactionRepository, booking, _reservationRepository, _applicationUserRoleRepository);
                                            if (transferGuestDialog.ShowDialog() == true)
                                            {
                                                IssueCardDialog issueCardDialog = new IssueCardDialog(_bookingRepository, _guestRepository, booking, _hotelSettingsService);
                                                if (issueCardDialog.ShowDialog() == true)
                                                {
                                                    await LoadBooking();
                                                }
                                            }
                                        }
                                    }
                                    else
                                    {
                                        MessageBox.Show($"An error ocurred when sending code. This might be caused by network related issues or otp sender service.", "Error", MessageBoxButton.OKCancel, MessageBoxImage.Error);
                                    }
                                }
                            }
                            else
                            {
                                TransferGuestDialog transferGuestDialog = new TransferGuestDialog(_guestRepository, _roomRepository, _hotelSettingsService, _bookingRepository, _verificationCodeService, _transactionRepository, booking, _reservationRepository, _applicationUserRoleRepository);
                                if (transferGuestDialog.ShowDialog() == true)
                                {
                                    IssueCardDialog issueCardDialog = new IssueCardDialog(_bookingRepository, _guestRepository, booking, _hotelSettingsService);
                                    if (issueCardDialog.ShowDialog() == true)
                                    {
                                        await LoadBooking();
                                    }
                                }
                            }
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
                        var bookingDetailsDialog = new ViewBookingDetailsDialog(booking, _transactionRepository, _bookingRepository, _hotelSettingsService);
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

        private async void DeleteBooking_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (sender is Button button && button.Tag is string Id)
                {
                    var selectedBooking = (BookingViewModel)BookingDataGrid.SelectedItem;

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
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                LoaderOverlay.Visibility = Visibility.Collapsed;
            }
        }

        private async void Page_Loaded(object sender, RoutedEventArgs e)
        {
            await LoadBooking();
        }
    }
}
