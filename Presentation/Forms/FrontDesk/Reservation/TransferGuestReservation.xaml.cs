using ESMART.Application.Common.Interface;
using ESMART.Application.Common.Utils;
using ESMART.Domain.Entities.FrontDesk;
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

namespace ESMART.Presentation.Forms.FrontDesk.Reservation
{
    /// <summary>
    /// Interaction logic for TransferGuestReservation.xaml
    /// </summary>
    public partial class TransferGuestReservation : Window
    {
        private readonly IGuestRepository _guestRepository;
        private readonly IHotelSettingsService _hotelSettingsService;
        private readonly IRoomRepository _roomRepository;
        private readonly IReservationRepository _reservationRepository;
        private readonly IVerificationCodeService _verificationCodeService;
        private readonly ITransactionRepository _transactionRepository;
        private readonly IBookingRepository _bookingRepository;
        private bool _suppressTextChanged = false;
        private readonly Domain.Entities.FrontDesk.Reservation _reservation;

        public TransferGuestReservation(IGuestRepository guestRepository, IRoomRepository roomRepository, IHotelSettingsService hotelSettingsService, IReservationRepository reservationRepository, IVerificationCodeService verificationCodeService, ITransactionRepository transactionRepository, Domain.Entities.FrontDesk.Reservation reservation, IBookingRepository bookingRepository)
        {
            _guestRepository = guestRepository;
            _roomRepository = roomRepository;
            _hotelSettingsService = hotelSettingsService;
            _verificationCodeService = verificationCodeService;
            _reservationRepository = reservationRepository;
            _transactionRepository = transactionRepository;
            _bookingRepository = bookingRepository;
            _reservation = reservation;
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

        private async Task LoadRooms()
        {
            LoaderOverlay.Visibility = Visibility.Visible;
            try
            {
                var rooms = await _roomRepository.GetAvailableRooms();

                if (rooms != null)
                {
                    var avalableTransferRooms = rooms.Where(r => r.Rate >= _reservation.Room.Rate && r.Number != _reservation.Room.Number).ToList();

                    cmbRoom.ItemsSource = avalableTransferRooms;
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
            LoaderOverlay.Visibility = Visibility.Visible;
            try
            {
                dtpCheckIn.SelectedDate = _reservation.ArrivateDate;
                dtpCheckOut.SelectedDate = _reservation.DepartureDate;

                txtDays.Text = (dtpCheckOut.SelectedDate.Value.Date - dtpCheckIn.SelectedDate.Value.Date).Days.ToString();
                txtDiscount.Text = _reservation.Discount.ToString();
                txtVAT.Text = _reservation.VAT.ToString();
                txtServiceCharge.Text = _reservation.ServiceCharge.ToString();
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

        private async void Save_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                LoaderOverlay.Visibility = Visibility.Visible;

                if (cmbRoom.SelectedItem is not Room selectedRoom)
                {
                    MessageBox.Show("Please select a room.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                decimal oldRate = _reservation.Room.Rate;
                decimal newRate = selectedRoom.Rate;

                var checkIn = dtpCheckOut.SelectedDate!.Value;
                var checkOut = dtpCheckIn.SelectedDate!.Value;
                int numberOfDays = (checkIn - checkOut).Days;

                decimal differencePerDay = newRate - oldRate;
                decimal totalDifference = Helper.GetPriceByRateAndTime(checkIn, checkOut, differencePerDay);
                decimal totalAmount = Helper.CalculateTotal(
                    Helper.GetPriceByRateAndTime(dtpCheckIn.SelectedDate.Value, dtpCheckOut.SelectedDate.Value, newRate),
                    decimal.Parse(txtDiscount.Text),
                    decimal.Parse(txtVAT.Text),
                    decimal.Parse(txtServiceCharge.Text)
                );

                if (differencePerDay != 0)
                {
                    string message = differencePerDay > 0
                        ? $"The new room is ₦{differencePerDay:N2} higher per day. Total additional charge: ₦{Math.Abs(totalDifference):N2}.\nDo you want to proceed?"
                        : $"The new room is ₦{Math.Abs(differencePerDay):N2} cheaper per day. Total refund/discount: ₦{Math.Abs(totalDifference):N2}.\nDo you want to proceed?";

                    if (MessageBox.Show(message, "Confirm Room Transfer", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.No)
                    {
                        return;
                    }
                }

                _reservation.RoomId = selectedRoom.Id;
                _reservation.Room = selectedRoom;
                _reservation.Amount += Helper.GetPriceByRateAndTime(dtpCheckIn.SelectedDate.Value, dtpCheckOut.SelectedDate.Value, newRate);

                if (differencePerDay != 0)
                {
                    _reservation.TotalAmount += totalAmount;
                }

                var isRoomAvailable = await CheckIfRoomCanBeBooked(selectedRoom.Number, dtpCheckIn.SelectedDate.Value, dtpCheckOut.SelectedDate.Value);

                if (isRoomAvailable)
                {
                    await _reservationRepository.UpdateReservationAsync(_reservation);

                    await HandlePostBookingAsync(_reservation, differencePerDay, totalAmount);

                    MessageBox.Show("Guest transferred successfully.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                    this.DialogResult = true;
                }
                else
                {
                    MessageBox.Show("The selected room is not available for the selected dates.", "Room Unavailable", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
               
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error saving transfer: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                LoaderOverlay.Visibility = Visibility.Collapsed;
            }
        }

        private async Task<bool> CheckIfRoomCanBeBooked(string roomno, DateTime checkIn, DateTime checkOut)
        {
            var reservations = await _reservationRepository.GetReservationsByRoomNoAndDateRangeAsync(roomno, checkIn, checkOut);
            if (reservations.Count > 0)
            {
                return false;
            }
            return true;
        }

        private async Task HandlePostBookingAsync(Domain.Entities.FrontDesk.Reservation reservation, decimal differencePerDay, decimal amount)
        {

            if (differencePerDay != 0)
            {
                var reservedRoom = await _roomRepository.GetRoomById(reservation.RoomId);
                var reservedGuest = await _guestRepository.GetGuestByIdAsync(reservation.GuestId);
                var hotel = await _hotelSettingsService.GetHotelInformation();

                var transaction = await _transactionRepository.GetByInvoiceNumberAsync(reservation.ReservationId);

                if (reservedRoom != null)
                {
                    reservedRoom.Status = RoomStatus.Reserved;
                    await _roomRepository.UpdateRoom(reservedRoom);
                }

                if (hotel != null)
                {
                    var verificationCode = new VerificationCode
                    {
                        Code = string.Concat("BK", Guid.NewGuid().ToString().Split("-")[0].ToUpper().AsSpan(0, 5)),
                        ServiceId = reservation.ReservationId,
                        ApplicationUserId = AuthSession.CurrentUser?.Id
                    };

                    await _verificationCodeService.AddCode(verificationCode);

                    var response = await SenderHelper.SendOtp(hotel, reservation.AccountNumber, reservedGuest, "Reservation", verificationCode.Code, amount);
                    if (response.IsSuccessStatusCode)
                    {
                        var verifyPaymentWindow = new VerifyPaymentWindow(_verificationCodeService, _hotelSettingsService, _bookingRepository, _transactionRepository, reservation.ReservationId);
                        if (verifyPaymentWindow.ShowDialog() == true)
                        {
                            if (reservation.AmountPaid == reservation.TotalAmount)
                            {
                                reservation.TransactionStatus = TransactionStatus.Paid;
                            }
                            else if (reservation.TotalAmount > reservation.AmountPaid)
                            {
                                reservation.TransactionStatus = TransactionStatus.Pending;
                            }
                        }
                        else
                        {
                            reservation.TransactionStatus = TransactionStatus.Pending;
                        }

                        await _reservationRepository.UpdateReservationAsync(reservation);
                    }
                    else
                    {
                        MessageBox.Show("Reservation room transfered successfully but could not verify payment. Payment will be flagged as pending.",
                            "Info", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                }

                if (transaction != null)
                {
                    var transactionItem = new Domain.Entities.Transaction.TransactionItem
                    {
                        Amount = amount,
                        TaxAmount = reservation.VAT,
                        ServiceId = reservation.ReservationId,
                        ServiceCharge = reservation.ServiceCharge,
                        Discount = reservation.Discount,
                        Category = Category.Accomodation,
                        Type = TransactionType.Adjustment,
                        BankAccount = reservation.AccountNumber,
                        DateAdded = DateTime.Now,
                        ApplicationUserId = AuthSession.CurrentUser?.Id,
                        TransactionId = transaction.Id,
                        Description = $"Reservation room transfered for {reservation.Guest.FullName} to {reservation.Room.Number} from {reservation.ArrivateDate} to {reservation.DepartureDate}"
                    };

                    if (reservation.TransactionStatus != TransactionStatus.Unpaid)
                    {
                        transactionItem.Status = TransactionStatus.Paid;
                    }
                    else
                    {
                        transactionItem.Status = TransactionStatus.Unpaid;
                    }

                    await _transactionRepository.AddTransactionItemAsync(transactionItem);
                }
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
                var currencySymbol = currencySetting?.Value ?? "₦";

                decimal newTotalAmount = Helper.CalculateTotal(
                    totalPrice,
                    decimal.Parse(txtDiscount.Text),
                    decimal.Parse(txtVAT.Text),
                    decimal.Parse(txtServiceCharge.Text)
                );

                decimal amountToDisplay;

                if (_reservation.Room.Rate == decimal.Parse(txtRoomRate.Text))
                {
                    amountToDisplay = 0;
                }
                else if (decimal.Parse(txtRoomRate.Text) > _reservation.Room.Rate)
                {
                    amountToDisplay = newTotalAmount - _reservation.AmountPaid;
                }
                else
                {
                    amountToDisplay = 0;
                }

                txtTotalAmount.Text = currencySymbol + " " + amountToDisplay.ToString("N2");
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
            await LoadRooms();
            await LoadFinancialMetric();
            LoadDefaultSetting();
            LoadPaymentMethod();
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
                    var currencySymbol = currencySetting?.Value ?? "₦";

                    decimal newTotalAmount = Helper.CalculateTotal(
                        totalPrice,
                        decimal.Parse(txtDiscount.Text),
                        decimal.Parse(txtVAT.Text),
                        decimal.Parse(txtServiceCharge.Text)
                    );

                    decimal amountToDisplay;

                    if (_reservation.Room.Rate == selectedRoom.Rate)
                    {
                        amountToDisplay = 0;
                        cmbAccountNumber.IsEnabled = false;
                        cmbPaymentMethod.IsEnabled = false;
                    }
                    else if (selectedRoom.Rate > _reservation.Room.Rate)
                    {
                        amountToDisplay = newTotalAmount - _reservation.AmountPaid;
                    }
                    else
                    {
                        amountToDisplay = 0;
                    }

                    txtTotalAmount.Text = currencySymbol + " " + amountToDisplay.ToString("N2");
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
                        var currencySymbol = currencySetting?.Value ?? "₦";

                        decimal newTotalAmount = Helper.CalculateTotal(
                            totalPrice,
                            decimal.Parse(txtDiscount.Text),
                            decimal.Parse(txtVAT.Text),
                            decimal.Parse(txtServiceCharge.Text)
                        );

                        decimal amountToDisplay;

                        if (_reservation.Room.Rate == decimal.Parse(txtRoomRate.Text))
                        {
                            amountToDisplay = 0;
                        }
                        else if (decimal.Parse(txtRoomRate.Text) > _reservation.Room.Rate)
                        {
                            amountToDisplay = newTotalAmount - _reservation.AmountPaid;
                        }
                        else
                        {
                            amountToDisplay = 0;
                        }

                        txtTotalAmount.Text = currencySymbol + " " + amountToDisplay.ToString("N2");
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
                        var currencySymbol = currencySetting?.Value ?? "₦";

                        decimal newTotalAmount = Helper.CalculateTotal(
                            totalPrice,
                            decimal.Parse(txtDiscount.Text),
                            decimal.Parse(txtVAT.Text),
                            decimal.Parse(txtServiceCharge.Text)
                        );

                        decimal amountToDisplay;

                        if (_reservation.Room.Rate == decimal.Parse(txtRoomRate.Text))
                        {
                            amountToDisplay = 0;
                        }
                        else if (decimal.Parse(txtRoomRate.Text) > _reservation.Room.Rate)
                        {
                            amountToDisplay = newTotalAmount - _reservation.AmountPaid;
                        }
                        else
                        {
                            amountToDisplay = 0;
                        }

                        txtTotalAmount.Text = currencySymbol + " " + amountToDisplay.ToString("N2");
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
                        var currencySymbol = currencySetting?.Value ?? "₦";

                        decimal newTotalAmount = Helper.CalculateTotal(
                            totalPrice,
                            decimal.Parse(txtDiscount.Text),
                            decimal.Parse(txtVAT.Text),
                            decimal.Parse(txtServiceCharge.Text)
                        );

                        decimal amountToDisplay;

                        if (_reservation.Room.Rate == decimal.Parse(txtRoomRate.Text))
                        {
                            amountToDisplay = 0;
                        }
                        else if (decimal.Parse(txtRoomRate.Text) > _reservation.Room.Rate)
                        {
                            amountToDisplay = newTotalAmount - _reservation.AmountPaid;
                        }
                        else
                        {
                            amountToDisplay = 0;
                        }

                        txtTotalAmount.Text = currencySymbol + " " + amountToDisplay.ToString("N2");
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
