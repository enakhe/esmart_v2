using ESMART.Application.Common.Interface;
using ESMART.Domain.Entities.Verification;
using ESMART.Presentation.Forms.Verification;
using ESMART.Presentation.Session;
using ESMART.Presentation.Utils;
using System.Windows;

namespace ESMART.Presentation.Forms.FrontDesk.Reservation
{
    /// <summary>
    /// Interaction logic for BookReservationDialog.xaml
    /// </summary>
    public partial class BookReservationDialog : Window
    {
        private readonly IBookingRepository _bookingRepository;
        private readonly ITransactionRepository _transactionRepository;
        private readonly IHotelSettingsService _hotelSettingsService;
        private readonly IVerificationCodeService _verificationCodeService;
        private readonly IApplicationUserRoleRepository _applicationUserRoleRepository;
        private readonly decimal _amount;
        private readonly Domain.Entities.FrontDesk.Booking _booking;
        private readonly Domain.Entities.Transaction.Transaction _transaction;
        private readonly string _guest;
        public BookReservationDialog(IBookingRepository bookingRepository, ITransactionRepository transactionRepository, IHotelSettingsService hotelSettingsService, IVerificationCodeService verificationCodeService, decimal amount, Domain.Entities.FrontDesk.Booking booking, Domain.Entities.Transaction.Transaction transaction, string guest, IApplicationUserRoleRepository applicationUserRoleRepository)
        {
            _amount = amount;
            _booking = booking;
            _guest = guest;
            _transaction = transaction;
            _bookingRepository = bookingRepository;
            _transactionRepository = transactionRepository;
            _hotelSettingsService = hotelSettingsService;
            _verificationCodeService = verificationCodeService;
            _applicationUserRoleRepository = applicationUserRoleRepository;
            InitializeComponent();

            txtAmount.Text = _amount.ToString("N2");
        }

        public async void VerifyButton_Click(object sender, RoutedEventArgs e)
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
                    //var response = await SenderHelper.SendOtp(hotel.PhoneNumber, hotel.Name, _booking.AccountNumber, _guest, "Booking", verificationCode.Code, _booking.Balance, _booking.PaymentMethod.ToString(), activeUser.FullName!, activeUser.PhoneNumber!);
                    //if (response.IsSuccessStatusCode)
                    //{
                    //    MessageBox.Show("Kindly verify booking payment", "Code resent", MessageBoxButton.OK, MessageBoxImage.Information);

                    //    VerifyPaymentWindow verifyPaymentWindow = new(_verificationCodeService, _hotelSettingsService, _bookingRepository, _transactionRepository, _booking.BookingId, _amount, _applicationUserRoleRepository);
                    //    if (verifyPaymentWindow.ShowDialog() == true)
                    //    {
                    //        _booking.Status = Domain.Enum.BookingStatus.Completed;
                    //        //_booking.Balance = 0;
                    //        await _bookingRepository.AddBooking(_booking);

                    //        _transaction.BookingId = _booking.Id;
                    //        await _transactionRepository.UpdateTransactionAsync(_transaction);

                    //        this.DialogResult = true;
                    //    }
                    //}
                    //else
                    //{
                    //    MessageBox.Show($"An error ocurred when sending code. This might be caused by network related issues or otp sender service.", "Error", MessageBoxButton.OKCancel, MessageBoxImage.Error);
                    //}
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

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
        }
    }
}
