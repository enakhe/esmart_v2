using ESMART.Application.Common.Interface;
using ESMART.Domain.Entities.Transaction;
using ESMART.Domain.Entities.Verification;
using ESMART.Domain.ViewModels.Transaction;
using ESMART.Presentation.Forms.Export;
using ESMART.Presentation.Forms.FrontDesk.Guest;
using ESMART.Presentation.Forms.Verification;
using ESMART.Presentation.Session;
using ESMART.Presentation.Utils;
using System.Windows;

namespace ESMART.Presentation.Forms.FrontDesk.Booking
{
    /// <summary>
    /// Interaction logic for CheckOutBooking.xaml
    /// </summary>
    public partial class CheckOutBooking : Window
    {
        private readonly ITransactionRepository _transactionRepository;
        private readonly IHotelSettingsService _hotelSettingsService;
        private readonly IVerificationCodeService _verificationCodeService;
        private readonly IApplicationUserRoleRepository _applicationUserRoleRepository;
        private readonly IBookingRepository _bookingRepository;
        private readonly IGuestRepository _guestRepository;
        private readonly Domain.Entities.FrontDesk.Booking _booking;
        private readonly Domain.Entities.FrontDesk.Guest _guest;
        private readonly decimal _amount;

        public CheckOutBooking(Domain.Entities.FrontDesk.Booking booking, Domain.Entities.FrontDesk.Guest guest, decimal amount, IReservationRepository reservationRepository, ITransactionRepository transactionRepository, IHotelSettingsService hotelSettingsService, IVerificationCodeService verificationCodeService, IApplicationUserRoleRepository applicationUserRoleRepository, IBookingRepository bookingRepository, IGuestRepository guestRepository)
        {
            _amount = amount;
            _booking = booking;
            _guest = guest;
            _bookingRepository = bookingRepository;
            _guestRepository = guestRepository;
            _transactionRepository = transactionRepository;
            _transactionRepository = transactionRepository;
            _hotelSettingsService = hotelSettingsService;
            _verificationCodeService = verificationCodeService;
            _applicationUserRoleRepository = applicationUserRoleRepository;
            InitializeComponent();

            txtExpectedAmount.Text = _amount.ToString("N2");
        }

        private async Task LoadTransactions()
        {
            LoaderOverlay.Visibility = Visibility.Visible;
            try
            {
                var transactions = await _transactionRepository.GetGroupedTransactionsByGuestIdAsync(_booking.GuestId);
                AccountTransactionStatement.ItemsSource = transactions;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error Loading Transactions", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                LoaderOverlay.Visibility = Visibility.Collapsed;
            }
        }


        private async void VerifyButton_Click(Object sender, RoutedEventArgs e)
        {
            LoaderOverlay.Visibility = Visibility.Visible;
            try
            {
                var hotel = await _hotelSettingsService.GetHotelInformation();
                var activeUser = await _applicationUserRoleRepository.GetUserById(AuthSession.CurrentUser!.Id);

                var verificationCode = new VerificationCode()
                {
                    Code = string.Concat("BK", Guid.NewGuid().ToString().Split("-")[0].ToUpper().AsSpan(0, 5)),
                    ServiceId = _booking.BookingId,
                    ApplicationUserId = AuthSession.CurrentUser?.Id
                };

                await _verificationCodeService.AddCode(verificationCode);

                if (hotel != null)
                {
                    var response = await SenderHelper.SendOtp(hotel.PhoneNumber, hotel.Name, _booking.AccountNumber, _booking.Guest.FullName, "Booking", verificationCode.Code, _amount, _booking.PaymentMethod.ToString(), activeUser.FullName!, activeUser.PhoneNumber!);
                    if (response.IsSuccessStatusCode)
                    {
                        MessageBox.Show("Kindly verify booking payment", "Code resent", MessageBoxButton.OK, MessageBoxImage.Information);

                        VerifyPaymentWindow verifyPaymentWindow = new(
                            _verificationCodeService,
                            _hotelSettingsService,
                            _bookingRepository,
                            _transactionRepository,
                            _booking.BookingId,
                            _amount,
                            _applicationUserRoleRepository
                        );

                        if (verifyPaymentWindow.ShowDialog() == true)
                        {
                            this.DialogResult = true;
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

        private async void ViewGuest_Click(Object sender, RoutedEventArgs e)
        {
            LoaderOverlay.Visibility = Visibility.Visible;
            try
            {
                var columnNames = AccountTransactionStatement.Columns
                    .Where(c => c.Header != null)
                    .Select(c => c.Header.ToString())
                    .Where(name => !string.IsNullOrWhiteSpace(name) && name != "Operation")
                    .ToList();

                var transactions = await _transactionRepository.GetGroupedTransactionsByGuestIdAsync(_booking.GuestId);
                // Flatten and bind all transaction items
                var allTransactionItems = transactions
                    .SelectMany(t => t.TransactionItems)
                    .OrderByDescending(ti => ti.Date)
                .ToList();

                var optionsWindow = new ExportBillDialog(columnNames!, AccountTransactionStatement, _hotelSettingsService, _booking, allTransactionItems);
                var result = optionsWindow.ShowDialog();

                if (result == true)
                {
                    var exportResult = optionsWindow.GetResult();
                    var hotel = await _hotelSettingsService.GetHotelInformation();

                    if (exportResult.SelectedColumns.Count == 0)
                    {
                        MessageBox.Show("Please select at least one column to export.", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
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

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            LoadTransactions();
        }
    }
}
