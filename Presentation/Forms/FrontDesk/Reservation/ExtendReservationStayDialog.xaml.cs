#nullable disable

using ESMART.Application.Common.Interface;
using ESMART.Application.Common.Utils;
using ESMART.Domain.Entities.Configuration;
using ESMART.Domain.Entities.Data;
using ESMART.Domain.Entities.FrontDesk;
using ESMART.Domain.Entities.Transaction;
using ESMART.Domain.Entities.Verification;
using ESMART.Domain.Enum;
using ESMART.Presentation.Forms.Verification;
using ESMART.Presentation.Session;
using ESMART.Presentation.Utils;
using System.Globalization;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace ESMART.Presentation.Forms.FrontDesk.Reservation
{
    /// <summary>
    /// Interaction logic for ExtendReservationStayDialog.xaml
    /// </summary>
    public partial class ExtendReservationStayDialog : Window
    {
        private readonly IGuestRepository _guestRepository;
        private readonly IHotelSettingsService _hotelSettingsService;
        private readonly IRoomRepository _roomRepository;
        private readonly IReservationRepository _reservationRepository;
        private readonly IVerificationCodeService _verificationCodeService;
        private readonly ITransactionRepository _transactionRepository;
        private readonly IBookingRepository _bookingRepository;
        private readonly IApplicationUserRoleRepository _applicationUserRoleRepository;
        private bool _suppressTextChanged = false;
        private readonly Domain.Entities.FrontDesk.Reservation _reservation;
        public ExtendReservationStayDialog(IGuestRepository guestRepository, IRoomRepository roomRepository, IHotelSettingsService hotelSettingsService, IReservationRepository reservationRepository, IVerificationCodeService verificationCodeService, ITransactionRepository transactionRepository, Domain.Entities.FrontDesk.Reservation reservation, IBookingRepository bookingRepository, IApplicationUserRoleRepository applicationUserRoleRepository)
        {
            _guestRepository = guestRepository;
            _roomRepository = roomRepository;
            _hotelSettingsService = hotelSettingsService;
            _verificationCodeService = verificationCodeService;
            _reservationRepository = reservationRepository;
            _transactionRepository = transactionRepository;
            _bookingRepository = bookingRepository;
            _applicationUserRoleRepository = applicationUserRoleRepository;
            _reservation = reservation;
            InitializeComponent();
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

        public async Task LoadBankAccount()
        {
            try
            {
                var accountNumber = await _transactionRepository.GetAllBankAccountAsync();

                cmbAccountNumber.ItemsSource = accountNumber;
                cmbAccountNumber.DisplayMemberPath = "BankAccountNumber";
                cmbAccountNumber.SelectedValuePath = "Id";
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void LoadDefaultSetting()
        {
            dtpCheckIn.DisplayDateStart = DateTime.Today;
            dtpCheckIn.SelectedDate = _reservation.ArrivateDate;
            dtpCheckOut.SelectedDate = _reservation.DepartureDate;

            txtRoom.Text = _reservation.Room.Number;
            txtRoomRate.Text = _reservation.Room.Rate.ToString();
            cmbAccountNumber.SelectedValue = _reservation.AccountNumber;
            cmbPaymentMethod.SelectedValue = _reservation.PaymentMethod;
        }

        private async void Save_Click(object sender, RoutedEventArgs e)
        {
            LoaderOverlay.Visibility = Visibility.Visible;
            try
            {
                if (!ValidateInputs(out var guestId, out var roomId, out var checkIn, out var checkOut, out var paymentMethod, out var totalAmount, out var discount, out var vat, out var serviceCharge, out var accountNumber, out var amountPaid))
                {
                    MessageBox.Show("Please fill in all required fields.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                var isRoomAvailable = await CheckIfRoomCanBeBooked(_reservation.Room.Number, checkIn, checkOut);

                if (isRoomAvailable)
                {
                    var booking = await UpdateReservation(ReservationStatus.Tentative, checkIn, checkOut, paymentMethod, totalAmount, discount, vat, serviceCharge, accountNumber, amountPaid);


                    await HandlePostBookingAsync(booking, totalAmount);

                    MessageBox.Show("Booking extended successfully", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                    this.DialogResult = true;
                }
                else
                {
                    MessageBox.Show("Room is not available for the selected dates. Please choose a different room or date range.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
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

        private async Task<bool> CheckIfRoomCanBeBooked(string roomno, DateTime checkIn, DateTime checkOut)
        {
            return await _reservationRepository.CanExtendStayAsync(_reservation.Id, _reservation.RoomId, checkOut);
        }

        private bool ValidateInputs(out string guestId, out string roomId, out DateTime checkIn, out DateTime checkOut, out PaymentMethod paymentMethod, out decimal totalAmount, out decimal discount, out decimal vat, out decimal serviceCharge, out string accountNumber, out decimal amountPaid)
        {
            guestId = String.Empty;
            roomId = String.Empty;
            checkIn = checkOut = DateTime.MinValue;
            paymentMethod = default;
            totalAmount = amountPaid = discount = vat = serviceCharge = 0;
            accountNumber = string.Empty;

            checkIn = dtpCheckIn.SelectedDate!.Value;
            checkOut = dtpCheckOut.SelectedDate!.Value;
            paymentMethod = Enum.Parse<PaymentMethod>(cmbPaymentMethod.SelectedValue.ToString()!);
            totalAmount = decimal.Parse(txtTotalAmount.Text.Replace("NGN", ""));
            discount = decimal.Parse(txtDiscount.Text);
            vat = decimal.Parse(txtVAT.Text);
            serviceCharge = decimal.Parse(txtServiceCharge.Text);
            accountNumber = ((BankAccount)cmbAccountNumber.SelectedItem).Id;

            return true;
        }

        private async Task<Domain.Entities.FrontDesk.Reservation> UpdateReservation(ReservationStatus status, DateTime checkIn, DateTime checkOut, PaymentMethod paymentMethod, decimal totalAmount, decimal discount, decimal vat, decimal serviceCharge, string accountNumber, decimal amountPaid)
        {
            var updatedBy = AuthSession.CurrentUser?.Id;
            var amount = decimal.Parse(txtRoomRate.Text);

            _reservation.DepartureDate = new DateTime(checkOut.Year, checkOut.Month, checkOut.Day, 12, 0, 0);
            _reservation.Amount = amount;
            _reservation.Discount = discount;
            _reservation.PaymentMethod = paymentMethod;
            _reservation.TotalAmount += totalAmount;
            _reservation.VAT = vat;
            _reservation.ServiceCharge = serviceCharge;
            _reservation.AccountNumber = accountNumber;
            _reservation.Status = status;

            await _reservationRepository.UpdateReservationAsync(_reservation);
            return _reservation;
        }

        private async Task HandlePostBookingAsync(Domain.Entities.FrontDesk.Reservation reservation, decimal amount)
        {
            var reservedRoom = await _roomRepository.GetRoomById(reservation.RoomId);
            var reservedGuest = await _guestRepository.GetGuestByIdAsync(reservation.GuestId);
            var reservedAccount = await _transactionRepository.GetBankAccountById(reservation.AccountNumber);
            var hotel = await _hotelSettingsService.GetHotelInformation();
            var transaction = await _transactionRepository.GetByInvoiceNumberAsync(reservation.ReservationId);
            var activeUser = await _applicationUserRoleRepository.GetUserById(AuthSession.CurrentUser!.Id);

            var transactionItem = new Domain.Entities.Transaction.TransactionItem
            {
                Amount = amount,
                TaxAmount = reservation.VAT,
                ServiceId = reservation.ReservationId,
                ServiceCharge = reservation.ServiceCharge,
                Discount = reservation.Discount,
                Category = Category.Accomodation,
                Type = TransactionType.Adjustment,
                BankAccount = $"{reservedAccount.BankAccountNumber} ({reservedAccount.BankName}) | {reservedAccount.BankAccountName}",
                DateAdded = DateTime.Now,
                ApplicationUserId = AuthSession.CurrentUser?.Id,
                TransactionId = transaction.Id!,
                Description = $"Reservation extended for {reservation.Guest.FullName} to {reservation.Room.Number} from {reservation.ArrivateDate} to {reservation.DepartureDate}"
            };

            await _transactionRepository.AddTransactionItemAsync(transactionItem);

            if (hotel != null)
            {
                var isVerifyPayment = await _hotelSettingsService.GetSettingAsync("VerifyTransaction");
                if (isVerifyPayment != null)
                {
                    var value = isVerifyPayment.Value;
                    if (value != null && value.Equals("true", StringComparison.CurrentCultureIgnoreCase))
                    {
                        await VerifyPayment(hotel, reservation, reservedGuest, activeUser);
                    }
                    else
                    {
                        reservation.Status = ReservationStatus.Confirmed;
                        reservation.TransactionStatus = TransactionStatus.Paid;

                        await _transactionRepository.UpdateTransactionItemAsync(transactionItem);
                        await _reservationRepository.UpdateReservationAsync(reservation);

                        await SenderHelper.SendEmail(
                             reservation.Guest.Email,
                             "Reservation Extension Payment Receipt",
                             "guest_receipt",
                             new ReceiptVariable
                             {
                                 accountNumber = $"{reservedAccount.BankAccountNumber} ({reservedAccount.BankName}) | {reservedAccount.BankAccountName}",
                                 amount = reservation.TotalAmount.ToString("N2"),
                                 guestName = reservation.Guest.FullName,
                                 hotelName = hotel.Name,
                                 invoiceNumber = transaction.InvoiceNumber,
                                 paymentMethod = reservation.PaymentMethod.ToString(),
                                 receptionist = activeUser.FullName,
                                 receptionistContact = activeUser.PhoneNumber,
                                 service = reservation.ReservationId,
                             }
                         );
                    }
                }
            }

            if (reservation.TransactionStatus != TransactionStatus.Unpaid)
            {
                transactionItem.Status = TransactionStatus.Paid;
            }

            await _transactionRepository.UpdateTransactionItemAsync(transactionItem);
        }

        private async Task VerifyPayment(Hotel hotel, Domain.Entities.FrontDesk.Reservation reservation, Domain.Entities.FrontDesk.Guest bookedGuest, ApplicationUser activeUser)
        {
            var reservedRoom = await _roomRepository.GetRoomById(reservation.RoomId);
            var reservedAccount = await _transactionRepository.GetBankAccountById(reservation.AccountNumber);
            var reservedGuest = await _guestRepository.GetGuestByIdAsync(reservation.GuestId);

            var verificationCode = new VerificationCode
            {
                Code = string.Concat("BK", Guid.NewGuid().ToString().Split("-")[0].ToUpper().AsSpan(0, 5)),
                ServiceId = reservation.ReservationId,
                ApplicationUserId = AuthSession.CurrentUser?.Id
            };

            await _verificationCodeService.AddCode(verificationCode);

            var response = await SenderHelper.SendOtp(
                hotel.PhoneNumber,
                hotel.Name,
                $"{reservedAccount.BankAccountNumber} ({reservedAccount.BankName}) | {reservedAccount.BankAccountName}",
                reservedGuest.FullName,
                "Reservation",
                verificationCode.Code,
                reservation.TotalAmount,
                reservation.PaymentMethod.ToString(),
                activeUser.FullName!,
                activeUser.PhoneNumber!
            );

            if (response.IsSuccessStatusCode)
            {
                var verifyPaymentWindow = new VerifyPaymentWindow(
                    _verificationCodeService,
                    _hotelSettingsService,
                    _bookingRepository,
                    _transactionRepository,
                    reservation.ReservationId,
                    reservation.TotalAmount,
                    _applicationUserRoleRepository
                );

                if (verifyPaymentWindow.ShowDialog() == true)
                {
                    reservation.Status = ReservationStatus.Confirmed;
                    reservation.TransactionStatus = TransactionStatus.Paid;
                }
                else
                {
                    await _verificationCodeService.DeleteAsync(verificationCode.Id);
                    reservation.TransactionStatus = TransactionStatus.Unpaid;
                }
                await _reservationRepository.UpdateReservationAsync(reservation);
            }
            else
            {
                MessageBox.Show("Reservation added successfully but could not verify payment. Payment will be flagged as pending.",
                    "Info", MessageBoxButton.OK, MessageBoxImage.Information);
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
                var totalAmount = Helper.CalculateTotal(totalPrice, decimal.Parse(txtDiscount.Text), decimal.Parse(txtVAT.Text), decimal.Parse(txtServiceCharge.Text));

                var currencySetting = await _hotelSettingsService.GetSettingAsync("CurrencySymbol");

                if (currencySetting != null)
                    txtTotalAmount.Text = currencySetting?.Value + " " + totalAmount.ToString("N2");
                else
                    txtTotalAmount.Text = "₦" + " " + totalAmount.ToString("N2");
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
            await LoadFinancialMetric();
            await LoadBankAccount();
            LoadPaymentMethod();
            LoadDefaultSetting();
        }

        private async void txtDays_TextChanged(object sender, TextChangedEventArgs e)
        {
            if ((int.TryParse(txtDays.Text, out int days) || days > 1) && dtpCheckIn.SelectedDate != null)
            {
                if (days > (_reservation.DepartureDate - _reservation.ArrivateDate).Days)
                {
                    btnSave.IsEnabled = true;
                }
                var checkOut = dtpCheckIn.SelectedDate.Value.AddDays(int.Parse(txtDays.Text));
                if (checkOut > dtpCheckIn.SelectedDate)
                {
                    dtpCheckOut.SelectedDate = checkOut;

                    bool isNull = Helper.AreAnyNullOrEmpty(txtRoomRate.Text);

                    if (!isNull)
                    {
                        var totalPrice = Helper.GetPriceByRateAndTime(dtpCheckIn.SelectedDate.Value, dtpCheckOut.SelectedDate.Value, decimal.Parse(txtRoomRate.Text));
                        var totalAmount = Helper.CalculateTotal(totalPrice, decimal.Parse(txtDiscount.Text), decimal.Parse(txtVAT.Text), decimal.Parse(txtServiceCharge.Text));

                        var currencySetting = await _hotelSettingsService.GetSettingAsync("CurrencySymbol");

                        if (currencySetting != null)
                            txtTotalAmount.Text = currencySetting?.Value + " " + totalAmount.ToString("N2");
                        else
                            txtTotalAmount.Text = "₦" + " " + totalAmount.ToString("N2");
                    }
                }
            }
        }

        private async void dtpCheckOut_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            if (dtpCheckOut.SelectedDate != null && dtpCheckIn.SelectedDate != null)
            {
                if (dtpCheckOut.SelectedDate <= dtpCheckIn.SelectedDate)
                {
                    dtpCheckOut.SelectedDate = dtpCheckIn.SelectedDate.Value.AddDays(1);
                }

                var days = (dtpCheckOut.SelectedDate.Value.Date - dtpCheckIn.SelectedDate.Value.Date).Days;
                txtDays.Text = days.ToString();

                bool isNull = Helper.AreAnyNullOrEmpty(txtRoomRate.Text);

                if (!isNull)
                {
                    var totalPrice = Helper.GetPriceByRateAndTime(
                        dtpCheckIn.SelectedDate.Value,
                        dtpCheckOut.SelectedDate.Value,
                        decimal.Parse(txtRoomRate.Text)
                    );

                    var currencySetting = await _hotelSettingsService.GetSettingAsync("CurrencySymbol");

                    if (currencySetting != null)
                    {
                        txtTotalAmount.Text = currencySetting.Value + " " +
                            Helper.CalculateTotal(totalPrice,
                                decimal.Parse(txtDiscount.Text),
                                decimal.Parse(txtVAT.Text),
                                decimal.Parse(txtServiceCharge.Text)).ToString("N2");
                    }
                    else
                    {
                        txtTotalAmount.Text = "₦" + " " +
                            Helper.CalculateTotal(totalPrice,
                                decimal.Parse(txtDiscount.Text),
                                decimal.Parse(txtVAT.Text),
                                decimal.Parse(txtServiceCharge.Text)).ToString("N2");
                    }
                }
            }
        }

        private async void dtpCheckIn_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            if (dtpCheckIn.SelectedDate != null)
            {
                dtpCheckOut.DisplayDateStart = dtpCheckIn.SelectedDate.Value.AddDays(1);

                if (dtpCheckOut.SelectedDate == null || dtpCheckOut.SelectedDate <= dtpCheckIn.SelectedDate)
                {
                    dtpCheckOut.SelectedDate = dtpCheckIn.SelectedDate.Value.AddDays(1);
                }

                var days = (dtpCheckOut.SelectedDate.Value.Date - dtpCheckIn.SelectedDate.Value.Date).Days;
                txtDays.Text = days.ToString();

                bool isNull = Helper.AreAnyNullOrEmpty(txtRoomRate.Text);

                if (!isNull)
                {
                    var totalPrice = Helper.GetPriceByRateAndTime(dtpCheckIn.SelectedDate.Value, dtpCheckOut.SelectedDate.Value, decimal.Parse(txtRoomRate.Text));

                    var currencySetting = await _hotelSettingsService.GetSettingAsync("CurrencySymbol");

                    if (currencySetting != null)
                        txtTotalAmount.Text = currencySetting.Value + " " + Helper.CalculateTotal(totalPrice, decimal.Parse(txtDiscount.Text), decimal.Parse(txtVAT.Text), decimal.Parse(txtServiceCharge.Text)).ToString("N2");
                    else
                        txtTotalAmount.Text = "₦" + " " + Helper.CalculateTotal(totalPrice, decimal.Parse(txtDiscount.Text), decimal.Parse(txtVAT.Text), decimal.Parse(txtServiceCharge.Text)).ToString("N2");
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
