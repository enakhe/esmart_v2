using ESMART.Application.Common.Interface;
using ESMART.Application.Common.Utils;
using ESMART.Application.Interface;
using ESMART.Domain.Entities.RoomSettings;
using ESMART.Domain.Entities.Verification;
using ESMART.Domain.Enum;
using ESMART.Presentation.Forms.Verification;
using ESMART.Presentation.Session;
using ESMART.Presentation.Utils;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace ESMART.Presentation.Forms.FrontDesk.Booking
{
    /// <summary>
    /// Interaction logic for AddBookingDialog.xaml
    /// </summary>
    public partial class AddBookingDialog : Window
    {
        private readonly IGuestRepository _guestRepository;
        private readonly IHotelSettingsService _hotelSettingsService;
        private readonly IRoomRepository _roomRepository;
        private readonly IBookingRepository _bookingRepository;
        private readonly IVerificationCodeService _verificationCodeService;
        private bool _suppressTextChanged = false;

        public AddBookingDialog(IGuestRepository guestRepository, IRoomRepository roomRepository, IHotelSettingsService hotelSettingsService, IBookingRepository bookingRepository, IVerificationCodeService verificationCodeService)
        {
            _guestRepository = guestRepository;
            _roomRepository = roomRepository;
            _hotelSettingsService = hotelSettingsService;
            _verificationCodeService = verificationCodeService;
            _bookingRepository = bookingRepository;
            InitializeComponent();
        }


        private async Task LoadFinancialMetric()
        {
            try
            {
                LoaderOverlay.Visibility = Visibility.Visible;

                var financialSettings = await _hotelSettingsService.GetSettingsByCategoryAsync("Financial Settings");
                if (financialSettings != null)
                {
                    foreach (var setting in financialSettings)
                    {
                        switch (setting.Key)
                        {
                            case "VAT":
                                txtVAT.Text = setting.Value;
                                break;
                            case "ServiceCharge":
                                txtServiceCharge.Text = setting.Value;
                                break;
                            case "Discount":
                                txtDiscount.Text = setting.Value;
                                break;
                        }
                    }
                }
            }
            catch (Exception ex) 
            {
                MessageBox.Show($"Error loading financial metrics: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                LoaderOverlay.Visibility = Visibility.Collapsed;
            }
        }

        private async Task LoadGuests()
        {
            LoaderOverlay.Visibility = Visibility.Visible;
            try
            {
                var guest = await _guestRepository.GetAllGuestsAsync();

                if (guest != null)
                {
                    cmbGuest.ItemsSource = guest;
                    cmbGuest.DisplayMemberPath = "FullName";
                    cmbGuest.SelectedValuePath = "Id";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading guests: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                LoaderOverlay.Visibility = Visibility.Collapsed;
            }
        }

        private async Task LoadRooms()
        {
            LoaderOverlay.Visibility = Visibility.Visible;
            try
            {
                var rooms = await _roomRepository.GetAvailableRooms();

                if (rooms != null)
                {
                    cmbRoom.ItemsSource = rooms;
                    cmbRoom.DisplayMemberPath = "Number";
                    cmbRoom.SelectedValuePath = "Id";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading rooms: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                LoaderOverlay.Visibility = Visibility.Collapsed;
            }
        }

        public void LoadPaymentMethod()
        {
            try
            {
                var method = Enum.GetValues<PaymentMethod>()
                    .Cast<PaymentMethod>()
                    .Select(e => new { Id = (int)e, Name = e.ToString() })
                    .ToList();

                cmbPaymentMethod.ItemsSource = method;
                cmbPaymentMethod.DisplayMemberPath = "Name";
                cmbPaymentMethod.SelectedValuePath = "Name";
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void LoadDefaultSetting()
        {
            dtpCheckIn.SelectedDate = DateTime.Now;
            dtpCheckOut.SelectedDate = DateTime.Now.AddDays(1);
        }

        private async void Save_Click(object sender, RoutedEventArgs e)
        {
            LoaderOverlay.Visibility = Visibility.Visible;
            try
            {
                if (cmbGuest.SelectedItem == null || cmbRoom.SelectedItem == null)
                {
                    MessageBox.Show("Please select a guest and a room.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                var guest = ((Domain.Entities.FrontDesk.Guest)cmbGuest.SelectedItem).Id;
                var room = ((Room)cmbRoom.SelectedItem).Id;
                var checkIn = dtpCheckIn.SelectedDate!.Value;
                var checkOut = dtpCheckOut.SelectedDate!.Value;
                var paymentMethod = Enum.Parse<PaymentMethod>(cmbPaymentMethod.SelectedValue.ToString()!);
                var totalAmount = decimal.Parse(txtTotalAmount.Text.Replace("NGN", ""));
                var discount = decimal.Parse(txtDiscount.Text);
                var vat = decimal.Parse(txtVAT.Text);
                var serviceCharge = decimal.Parse(txtServiceCharge.Text);
                var accountNumber = cmbAccountNumber.Text;
                var status = PaymentStatus.Pending;
                var createdBy = AuthSession.CurrentUser?.Id;
                var amount = decimal.Parse(txtRoomRate.Text);

                var booking = new Domain.Entities.FrontDesk.Booking
                {
                    BookingId = string.Concat("BK", Guid.NewGuid().ToString().Split("-")[0].ToUpper().AsSpan(0, 5)),
                    CheckIn = checkIn,
                    CheckOut = new DateTime(checkOut.Year, checkOut.Month, checkOut.Day, 12, 0, 0),
                    Amount = amount,
                    Status = status,
                    AccountNumber = accountNumber.ToString(),
                    Discount = discount,
                    VAT = vat,
                    ServiceCharge = serviceCharge,
                    TotalAmount = totalAmount,
                    PaymentMethod = paymentMethod,

                    GuestId = guest,
                    RoomId = room,
                    CreatedBy = createdBy,
                    UpdatedBy = createdBy,

                    DateCreated = DateTime.Now,
                    DateModified = DateTime.Now,
                };

                var result = await _bookingRepository.AddBooking(booking);

                if (!result.Succeeded)
                {
                    var sb = new StringBuilder();
                    foreach (var item in result.Errors)
                    {
                        sb.AppendLine(item);
                    }

                    MessageBox.Show(sb.ToString(), "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                else
                {
                    var bookedResult = await _roomRepository.GetRoomById(booking.RoomId);
                    var bookedGuest = await _guestRepository.GetGuestByIdAsync(booking.GuestId!);
                    var hotel = await _hotelSettingsService.GetHotelInformation();

                    if (!bookedResult.Succeeded)
                    {
                        var sb = new StringBuilder();
                        foreach (var item in bookedResult.Errors)
                        {
                            sb.AppendLine(item);
                        }

                        MessageBox.Show(sb.ToString(), "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }

                    var bookedRoom = bookedResult.Response;
                    if (bookedRoom != null)
                    {
                        bookedRoom.Status = RoomStatus.Booked;
                        await _roomRepository.UpdateRoom(bookedRoom);
                    }

                    if (hotel != null)
                    {
                        var verificationCode = new VerificationCode()
                        {
                            Code = booking.BookingId,
                            BookingId = booking.Id,
                            IssuedBy = AuthSession.CurrentUser?.Id
                        };

                        await _verificationCodeService.AddCode(verificationCode);

                        var response = await SenderHelper.SendOtp(hotel, booking, bookedGuest.Response, "Booking", verificationCode.Code);
                        if (response.IsSuccessStatusCode)
                        {
                            VerifyPaymentWindow verifyPaymentWindow = new(_verificationCodeService, _hotelSettingsService, _bookingRepository, booking);
                            if (verifyPaymentWindow.ShowDialog() == true)
                            {
                                
                            }
                        }
                    }

                    MessageBox.Show("Booking added successfully", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                    this.DialogResult = true;
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

        // Allow only numbers and one decimal point
        private void DecimalInput_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !Regex.IsMatch(e.Text, @"[\d.]");
        }

        // Prevent user from entering multiple decimal points
        private async void DecimalInput_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (_suppressTextChanged) return;

            var textBox = sender as TextBox;
            if (string.IsNullOrWhiteSpace(textBox?.Text)) return;

            if (decimal.TryParse(textBox.Text.Replace(",", ""), out decimal value))
            {
                _suppressTextChanged = true;
                textBox.Text = string.Format(CultureInfo.InvariantCulture, "{0:N}", value);
                textBox.CaretIndex = textBox.Text.Length;
                _suppressTextChanged = false;
            }

            if (dtpCheckIn.SelectedDate != null && dtpCheckOut.SelectedDate != null)
            {
                var totalPrice = Helper.GetPriceByRateAndTime(dtpCheckIn.SelectedDate.Value, dtpCheckOut.SelectedDate.Value, decimal.Parse(txtRoomRate.Text));

                var currencySetting = await _hotelSettingsService.GetSettingAsync("CurrencySymbol");

                if (currencySetting != null)
                    txtTotalAmount.Text = currencySetting?.Value + " " + Helper.CalculateTotal(totalPrice, decimal.Parse(txtDiscount.Text), decimal.Parse(txtVAT.Text), decimal.Parse(txtServiceCharge.Text)).ToString();
                else
                    txtTotalAmount.Text = "₦" + " " + Helper.CalculateTotal(totalPrice, decimal.Parse(txtDiscount.Text), decimal.Parse(txtVAT.Text), decimal.Parse(txtServiceCharge.Text)).ToString();
            }
        }

        // Allow navigation keys (backspace, delete, arrows)
        private void DecimalInput_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            e.Handled = !(e.Key >= Key.D0 && e.Key <= Key.D9 ||
                          e.Key >= Key.NumPad0 && e.Key <= Key.NumPad9 ||
                          e.Key == Key.Back || e.Key == Key.Delete ||
                          e.Key == Key.Left || e.Key == Key.Right ||
                          e.Key == Key.Decimal || e.Key == Key.OemPeriod);
        }

        private void NumberOnly_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !IsTextNumeric(e.Text);
        }

        private bool IsTextNumeric(string text)
        {
            return text.All(char.IsDigit);
        }

        private void EditButton_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            txtRoomRate.IsEnabled = true;
        }

        private async void Window_Activated(object sender, EventArgs e)
        {
            await LoadGuests();
            await LoadRooms();
            await LoadFinancialMetric();
            LoadPaymentMethod();
            LoadDefaultSetting();
        }

        private async void cmbRoom_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cmbRoom.SelectedItem is Room selectedRoom)
            {
                txtRoomRate.Text = selectedRoom.Rate.ToString();
                txtRoomRate.IsEnabled = false;

                if (dtpCheckIn.SelectedDate != null && dtpCheckOut.SelectedDate != null)
                {
                    var totalPrice = Helper.GetPriceByRateAndTime(dtpCheckIn.SelectedDate.Value, dtpCheckOut.SelectedDate.Value, selectedRoom.Rate);

                    var currencySetting = await _hotelSettingsService.GetSettingAsync("CurrencySymbol");

                    if (currencySetting != null)
                        txtTotalAmount.Text = currencySetting?.Value + " " + Helper.CalculateTotal(totalPrice, decimal.Parse(txtDiscount.Text), decimal.Parse(txtVAT.Text), decimal.Parse(txtServiceCharge.Text)).ToString();
                    else
                        txtTotalAmount.Text = "₦" + " " + Helper.CalculateTotal(totalPrice, decimal.Parse(txtDiscount.Text), decimal.Parse(txtVAT.Text), decimal.Parse(txtServiceCharge.Text)).ToString();
                }
            }
        }

        private async void txtDays_TextChanged(object sender, TextChangedEventArgs e)
        {
            if ((int.TryParse(txtDays.Text, out int days) || days > 1) && dtpCheckIn.SelectedDate != null)
            {
                var checkOut = dtpCheckIn.SelectedDate.Value.AddDays(int.Parse(txtDays.Text));
                if (checkOut > dtpCheckIn.SelectedDate)
                {
                    dtpCheckOut.SelectedDate = checkOut;

                    bool isNull = Helper.AreAnyNullOrEmpty(txtRoomRate.Text);

                    if (!isNull)
                    {
                        var totalPrice = Helper.GetPriceByRateAndTime(dtpCheckIn.SelectedDate.Value, dtpCheckOut.SelectedDate.Value, decimal.Parse(txtRoomRate.Text));

                        var currencySetting = await _hotelSettingsService.GetSettingAsync("CurrencySymbol");

                        if (currencySetting != null)
                            txtTotalAmount.Text = currencySetting?.Value + " " + Helper.CalculateTotal(totalPrice, decimal.Parse(txtDiscount.Text), decimal.Parse(txtVAT.Text), decimal.Parse(txtServiceCharge.Text)).ToString();
                        else
                            txtTotalAmount.Text = "₦" + " " + Helper.CalculateTotal(totalPrice, decimal.Parse(txtDiscount.Text), decimal.Parse(txtVAT.Text), decimal.Parse(txtServiceCharge.Text)).ToString();
                    }
                }
            }
        }

        private async void dtpCheckOut_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            if (dtpCheckIn.SelectedDate != null && dtpCheckOut.SelectedDate != null)
            {
                var days = (dtpCheckOut.SelectedDate.Value - dtpCheckIn.SelectedDate.Value).Days;

                if (days > 0)
                {
                    txtDays.Text = days.ToString();

                    bool isNull = Helper.AreAnyNullOrEmpty(txtRoomRate.Text);

                    if (!isNull)
                    {
                        var totalPrice = Helper.GetPriceByRateAndTime(dtpCheckIn.SelectedDate.Value, dtpCheckOut.SelectedDate.Value, decimal.Parse(txtRoomRate.Text));

                        var currencySetting = await _hotelSettingsService.GetSettingAsync("CurrencySymbol");

                        if (currencySetting != null)
                            txtTotalAmount.Text = currencySetting?.Value + " " + Helper.CalculateTotal(totalPrice, decimal.Parse(txtDiscount.Text), decimal.Parse(txtVAT.Text), decimal.Parse(txtServiceCharge.Text)).ToString();
                        else
                            txtTotalAmount.Text = "₦" + " " + Helper.CalculateTotal(totalPrice, decimal.Parse(txtDiscount.Text), decimal.Parse(txtVAT.Text), decimal.Parse(txtServiceCharge.Text)).ToString();
                    }
                }
            }
        }

        private async void dtpCheckIn_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            if (dtpCheckIn.SelectedDate != null && dtpCheckOut.SelectedDate != null)
            {
                var days = (dtpCheckOut.SelectedDate.Value - dtpCheckIn.SelectedDate.Value).Days;

                if (days > 0)
                {
                    txtDays.Text = days.ToString();

                    bool isNull = Helper.AreAnyNullOrEmpty(txtRoomRate.Text);

                    if (!isNull)
                    {
                        var totalPrice = Helper.GetPriceByRateAndTime(dtpCheckIn.SelectedDate.Value, dtpCheckOut.SelectedDate.Value, decimal.Parse(txtRoomRate.Text));

                        var currencySetting = await _hotelSettingsService.GetSettingAsync("CurrencySymbol");

                        if (currencySetting != null)
                            txtTotalAmount.Text = currencySetting?.Value + " " + Helper.CalculateTotal(totalPrice, decimal.Parse(txtDiscount.Text), decimal.Parse(txtVAT.Text), decimal.Parse(txtServiceCharge.Text)).ToString();
                        else
                            txtTotalAmount.Text = "₦" + " " + Helper.CalculateTotal(totalPrice, decimal.Parse(txtDiscount.Text), decimal.Parse(txtVAT.Text), decimal.Parse(txtServiceCharge.Text)).ToString();
                    }
                }
            }
        }

        private void VatTextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            InputFormatter.AllowDecimalOnly(sender, e);
        }

        private void VatTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            InputFormatter.FormatAsPercentageOnLostFocus(sender, e);
        }

        private void VatTextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            InputFormatter.StripPercentageOnGotFocus(sender, e);
        }


        // Service Charge
        private void ServiceChargeTextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            InputFormatter.AllowDecimalOnly(sender, e);
        }

        private void ServiceChargeTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            InputFormatter.FormatAsPercentageOnLostFocus(sender, e);
        }

        private void ServiceChargeTextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            InputFormatter.StripPercentageOnGotFocus(sender, e);
        }


        // Discount
        private void DiscountTextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            InputFormatter.AllowDecimalOnly(sender, e);
        }

        private void DiscountTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            InputFormatter.FormatAsDecimalOnLostFocus(sender, e);
        }

        private void DiscountTextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            InputFormatter.StripPercentageOnGotFocus(sender, e);
        }

        private void txtDiscount_GotFocus(object sender, RoutedEventArgs e)
        {
            InputFormatter.StripPercentageOnGotFocus(sender, e);
        }

        private void txtDiscount_TextChanged(object sender, TextChangedEventArgs e)
        {
            InputFormatter.FormatAsDecimalOnLostFocus(sender, e);
        }

        private void txtVAT_TextChanged(object sender, TextChangedEventArgs e)
        {
            InputFormatter.FormatAsDecimalOnLostFocus(sender, e);
        }

        private void txtServiceCharge_TextChanged(object sender, TextChangedEventArgs e)
        {
            InputFormatter.FormatAsDecimalOnLostFocus(sender, e);
        }
    }
}
