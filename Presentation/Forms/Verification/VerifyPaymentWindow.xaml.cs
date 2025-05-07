#nullable disable

using ESMART.Application.Common.Interface;
using ESMART.Application.Common.Utils;
using ESMART.Domain.Entities.FrontDesk;
using ESMART.Domain.Entities.Verification;
using ESMART.Domain.Enum;
using ESMART.Presentation.Session;
using ESMART.Presentation.Utils;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;

namespace ESMART.Presentation.Forms.Verification
{
    /// <summary>
    /// Interaction logic for VerifyPaymentWindow.xaml
    /// </summary>
    public partial class VerifyPaymentWindow : Window
    {
        private readonly IVerificationCodeService _verificationCodeService;
        private readonly IHotelSettingsService _hotelSettingsService;
        private readonly IBookingRepository _bookingRepository;
        private readonly ITransactionRepository _transactionRepository;
        private string _serviceId;
        private DispatcherTimer _timer;
        private TimeSpan _timeRemaining;
        public VerifyPaymentWindow(IVerificationCodeService verificationCodeService, IHotelSettingsService hotelSettingsService, IBookingRepository bookingRepository, ITransactionRepository transactionRepository, string serviceId)
        {
            _verificationCodeService = verificationCodeService;
            _hotelSettingsService = hotelSettingsService;
            _bookingRepository = bookingRepository;
            _transactionRepository = transactionRepository;
            _serviceId = serviceId;
            InitializeComponent();
            StartCountdown(TimeSpan.FromMinutes(20));
        }

        private void txtCode_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (sender is not TextBox textBox) return;

            int caretIndex = textBox.CaretIndex;

            string transformed = new(textBox.Text.Take(5).Select(c => char.IsLetter(c) ? char.ToUpper(c) : c).ToArray());

            if (textBox.Text != transformed)
            {
                textBox.Text = transformed;
                textBox.CaretIndex = Math.Min(caretIndex, transformed.Length);
            }
        }

        private void StartCountdown(TimeSpan countdownTime)
        {
            _timeRemaining = countdownTime;

            _timer = new DispatcherTimer();
            _timer.Interval = TimeSpan.FromSeconds(1);
            _timer.Tick += Timer_Tick;
            _timer.Start();

            UpdateTimerText();
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            if (_timeRemaining.TotalSeconds <= 0)
            {
                _timer.Stop();
                TimerText.Text = "00:00";
            }
            else
            {
                _timeRemaining = _timeRemaining.Subtract(TimeSpan.FromSeconds(1));
                UpdateTimerText();
            }
        }

        private void UpdateTimerText()
        {
            TimerText.Text = _timeRemaining.ToString(@"mm\:ss");
        }

        public async void VerifyButton_Click(object sender, RoutedEventArgs e)
        {
            bool isNull = Helper.AreAnyNullOrEmpty(txtCode.Text);
            if (!isNull)
            {
                LoaderOverlay.Visibility = Visibility.Visible;
                try
                {
                    var enteredCode = txtPrefix.Text + txtCode.Text;

                    var code = await _verificationCodeService.GetCodeByCode(enteredCode);

                    if (code != null)
                    {
                        if (code.ExpiresAt < DateTime.Now)
                        {
                            MessageBox.Show("Verification code has expired, kindly check the code or resend another one", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                            await _verificationCodeService.DeleteAsync(code.Id);
                        }
                        else
                        {
                            var isValid = await _verificationCodeService.VerifyCodeAsync(_serviceId, enteredCode);

                            if (isValid)
                            {
                                MessageBox.Show("Successfully verified OTP, kindly issue a card for the guest", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                                await _verificationCodeService.DeleteAsync(code.Id);

                                var transaction = await _transactionRepository.GetUnpaidTransactionItemsByServiceIdAsync(_serviceId);
                                if (transaction != null)
                                {
                                    transaction.Status = TransactionStatus.Paid;
                                }

                                this.DialogResult = true;
                            }
                            else
                            {
                                MessageBox.Show("Invalid OTP, kindly check the code or try again", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
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
            else
            {
                MessageBox.Show("Please select enter in the code.", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        public async void ResendButton_Click(object sender, RoutedEventArgs e)
        {
            LoaderOverlay.Visibility = Visibility.Visible;
            try
            {
                var oldCode = await _verificationCodeService.GetCodeByServiceId(_serviceId);
                if (oldCode != null)
                {
                    await _verificationCodeService.DeleteAsync(oldCode.Id);
                }

                var hotel = await _hotelSettingsService.GetHotelInformation();
                if (hotel != null)
                {
                    var verificationCode = new VerificationCode()
                    {
                        Code = string.Concat("BK", Guid.NewGuid().ToString().Split("-")[0].ToUpper().AsSpan(0, 5)),
                        ServiceId = _serviceId,
                        ApplicationUserId = AuthSession.CurrentUser?.Id
                    };

                    await _verificationCodeService.AddCode(verificationCode);

                    var booking = await _bookingRepository.GetBookingById(_serviceId);

                    var response = await SenderHelper.SendOtp(hotel, booking.AccountNumber, booking.Guest, "Booking", verificationCode.Code, booking.TotalAmount);
                    if (response.IsSuccessStatusCode)
                    {
                        MessageBox.Show("New Verification code has been sent", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
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
    }
}
