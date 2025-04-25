using ESMART.Application.Common.Interface;
using ESMART.Application.Common.Utils;
using ESMART.Domain.Entities.FrontDesk;
using ESMART.Domain.Entities.Verification;
using ESMART.Domain.Enum;
using ESMART.Infrastructure.Repositories.Configuration;
using ESMART.Infrastructure.Repositories.FrontDesk;
using ESMART.Presentation.Session;
using ESMART.Presentation.Utils;
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
using System.Windows.Shapes;
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
        private readonly Booking _booking;
        private DispatcherTimer _timer;
        private TimeSpan _timeRemaining;
        public VerifyPaymentWindow(IVerificationCodeService verificationCodeService, IHotelSettingsService hotelSettingsService, IBookingRepository bookingRepository, Booking booking)
        {
            _verificationCodeService = verificationCodeService;
            _hotelSettingsService = hotelSettingsService;
            _bookingRepository = bookingRepository;
            _booking = booking;
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
                MessageBox.Show("Time's up!");
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
                        if(code.ExpiresAt < DateTime.Now)
                        {
                            MessageBox.Show("Verification code has expired, kindly check the code or resend another one", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                        else
                        {
                            var isValid = await _verificationCodeService.VerifyCodeAsync(_booking.Id, enteredCode);

                            if (isValid)
                            {
                                MessageBox.Show("Successfully verified OTP, kindly issue a card for the guest", "Success", MessageBoxButton.OK, MessageBoxImage.Information);

                                var paidBookingResult = await _bookingRepository.GetBookingById(_booking.Id);

                                if (paidBookingResult != null)
                                {
                                    var paidBooking = paidBookingResult.Response;
                                    if (paidBooking != null)
                                    {
                                        paidBooking.Status = PaymentStatus.Completed;
                                        await _bookingRepository.UpdateBooking(paidBooking);
                                    }
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
                var oldCode = await _verificationCodeService.GetCodeByBookingId(_booking.Id);
                if(oldCode != null)
                {
                    await _verificationCodeService.DeleteAsync(oldCode.Id);
                }

                var hotel = await _hotelSettingsService.GetHotelInformation();
                if (hotel != null)
                {
                    var verificationCode = new VerificationCode()
                    {
                        Code = string.Concat("BK", Guid.NewGuid().ToString().Split("-")[0].ToUpper().AsSpan(0, 5)),
                        BookingId = _booking.Id,
                        IssuedBy = AuthSession.CurrentUser?.Id
                    };

                    await _verificationCodeService.AddCode(verificationCode);

                    var response = await SenderHelper.SendOtp(hotel, _booking, _booking.Guest, "Booking", verificationCode.Code, _booking.TotalAmount);
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
