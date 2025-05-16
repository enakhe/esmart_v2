using ESMART.Application.Common.Interface;
using ESMART.Domain.Entities.Verification;
using ESMART.Presentation.Forms.Verification;
using ESMART.Presentation.Session;
using ESMART.Presentation.Utils;
using System.Windows;

namespace ESMART.Presentation.Forms.FrontDesk.Reservation
{
    /// <summary>
    /// Interaction logic for CancelReservationDialog.xaml
    /// </summary>
    public partial class CancelReservationDialog : Window
    {
        private readonly ITransactionRepository _transactionRepository;
        private readonly IHotelSettingsService _hotelSettingsService;
        private readonly IVerificationCodeService _verificationCodeService;
        private readonly IApplicationUserRoleRepository _applicationUserRoleRepository;
        private readonly IBookingRepository _bookingRepository;
        private readonly IGuestRepository _guestRepository;
        private readonly Domain.Entities.FrontDesk.Reservation _reservation;
        private readonly decimal _amount;
        private readonly decimal _refundPercentage;
        public CancelReservationDialog(Domain.Entities.FrontDesk.Reservation reservation, decimal amount, decimal refundPercentage, IReservationRepository reservationRepository, ITransactionRepository transactionRepository, IHotelSettingsService hotelSettingsService, IVerificationCodeService verificationCodeService, IApplicationUserRoleRepository applicationUserRoleRepository, IBookingRepository bookingRepository, IGuestRepository guestRepository)
        {
            _amount = amount;
            _refundPercentage = refundPercentage;
            _reservation = reservation;
            _transactionRepository = transactionRepository;
            _hotelSettingsService = hotelSettingsService;
            _verificationCodeService = verificationCodeService;
            _applicationUserRoleRepository = applicationUserRoleRepository;
            _bookingRepository = bookingRepository;
            _guestRepository = guestRepository;

            InitializeComponent();
            txtAmount.Text = _reservation.AmountPaid.ToString("N2");
            txtPercent.Text = _refundPercentage.ToString("N2") + "%";
            txtExpectedAmount.Text = _amount.ToString("N2");
        }

        private async void RefundButton_Click(object sender, RoutedEventArgs e)
        {
            LoaderOverlay.Visibility = Visibility.Visible;
            try
            {
                var reservedGuest = await _guestRepository.GetGuestByIdAsync(_reservation.GuestId);
                var hotel = await _hotelSettingsService.GetHotelInformation();
                var activeUser = await _applicationUserRoleRepository.GetUserById(AuthSession.CurrentUser!.Id);

                if (hotel != null)
                {
                    var verificationCode = new VerificationCode
                    {
                        Code = string.Concat("BK", Guid.NewGuid().ToString().Split("-")[0].ToUpper().AsSpan(0, 5)),
                        ServiceId = _reservation.ReservationId,
                        ApplicationUserId = AuthSession.CurrentUser?.Id
                    };

                    await _verificationCodeService.AddCode(verificationCode);

                    var response = await SenderHelper.SendRefundOtp(hotel.PhoneNumber, hotel.Name, reservedGuest.FullName, "Reservation Refund", verificationCode.Code, _amount, _reservation.PaymentMethod.ToString(), activeUser.FullName!, activeUser.PhoneNumber!);
                    if (response.IsSuccessStatusCode)
                    {
                        var verifyPaymentWindow = new VerifyPaymentWindow(_verificationCodeService, _hotelSettingsService, _bookingRepository, _transactionRepository, _reservation.ReservationId, _amount, _applicationUserRoleRepository);
                        if (verifyPaymentWindow.ShowDialog() == true)
                        {
                            this.DialogResult = true;
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

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
        }
    }
}
