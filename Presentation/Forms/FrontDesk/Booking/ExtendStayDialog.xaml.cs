using ESMART.Application.Common.Interface;
using ESMART.Application.Common.Utils;
using ESMART.Domain.Entities.Configuration;
using ESMART.Domain.Entities.Data;
using ESMART.Domain.Entities.FrontDesk;
using ESMART.Domain.Entities.RoomSettings;
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
using System.Windows.Media;

namespace ESMART.Presentation.Forms.FrontDesk.Booking
{
    /// <summary>
    /// Interaction logic for ExtendStayDialog.xaml
    /// </summary>
    public partial class ExtendStayDialog : Window
    {
        private readonly IGuestRepository _guestRepository;
        private readonly IHotelSettingsService _hotelSettingsService;
        private readonly IRoomRepository _roomRepository;
        private readonly IBookingRepository _bookingRepository;
        private readonly IVerificationCodeService _verificationCodeService;
        private readonly ITransactionRepository _transactionRepository;
        private readonly IReservationRepository _reservationRepository;
        private readonly IApplicationUserRoleRepository _applicationUserRoleRepository;
        private bool _suppressTextChanged = false;
        private Domain.Entities.FrontDesk.Booking _booking;
        private bool _isUnpaid = false;
        public ExtendStayDialog(IGuestRepository guestRepository, IRoomRepository roomRepository, IHotelSettingsService hotelSettingsService, IBookingRepository bookingRepository, IVerificationCodeService verificationCodeService, ITransactionRepository transactionRepository, Domain.Entities.FrontDesk.Booking booking, IReservationRepository reservationRepository, IApplicationUserRoleRepository applicationUserRoleRepository)
        {
            _guestRepository = guestRepository;
            _roomRepository = roomRepository;
            _hotelSettingsService = hotelSettingsService;
            _verificationCodeService = verificationCodeService;
            _bookingRepository = bookingRepository;
            _transactionRepository = transactionRepository;
            _reservationRepository = reservationRepository;
            _applicationUserRoleRepository = applicationUserRoleRepository;
            _booking = booking;
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
                                txtVAT.Text = decimal.Parse(setting.Value).ToString("N2");
                                break;
                            case "ServiceCharge":
                                txtServiceCharge.Text = decimal.Parse(setting.Value).ToString("N2");
                                break;
                            case "Discount":
                                txtDiscount.Text = decimal.Parse(setting.Value).ToString("N2");
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
            dtpCheckIn.SelectedDate = _booking.CheckIn;
            dtpCheckOut.SelectedDate = _booking.CheckOut.AddDays(1);

            txtRoom.Text = _booking.Room.Number;
            txtRoomRate.Text = _booking.Room.Rate.ToString();
            cmbAccountNumber.SelectedValue = _booking.AccountNumber;
            cmbPaymentMethod.SelectedValue = _booking.PaymentMethod;
        }

        private async void Save_Click(object sender, RoutedEventArgs e)
        {
            LoaderOverlay.Visibility = Visibility.Visible;
            try
            {
                if (!ValidateInputs(out var guestId, out var roomId, out var checkIn, out var checkOut, out var paymentMethod, out var totalAmount, out var discount, out var vat, out var serviceCharge, out var accountNumber))
                {
                    MessageBox.Show("Please fill in all required fields.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                var isRoomAvailable = await CheckIfRoomCanBeBooked(_booking.Room.Number, checkIn, checkOut);
                var amount = Helper.GetPriceByRateAndTime(checkIn, checkOut, decimal.Parse(txtRoomRate.Text)) - Helper.GetPriceByRateAndTime(_booking.CheckIn, _booking.CheckOut, _booking.Room.Rate);

                if (isRoomAvailable)
                {
                    var booking = await UpdateBooking(BookingStatus.Pending, checkIn, checkOut, paymentMethod, totalAmount, discount, vat, serviceCharge, accountNumber);
                    await HandlePostBookingAsync(booking, totalAmount, amount);

                    MessageBox.Show("Booking extended successfully", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                    this.DialogResult = true;
                }
                else
                {
                    MessageBox.Show("Room is not available for the selected dates.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
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
            var reservations = await _reservationRepository.GetReservationsByRoomNoAndDateRangeAsync(roomno, checkIn, checkOut);
            if (reservations.Count > 0)
            {
                return false;
            }
            return true;
        }

        private bool ValidateInputs(out string guestId, out string roomId, out DateTime checkIn, out DateTime checkOut, out PaymentMethod paymentMethod, out decimal totalAmount, out decimal discount, out decimal vat, out decimal serviceCharge, out string accountNumber)
        {
            guestId = String.Empty;
            roomId = String.Empty;
            checkIn = checkOut = DateTime.MinValue;
            paymentMethod = default;
            totalAmount = discount = vat = serviceCharge = 0;
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

        private async Task<Domain.Entities.FrontDesk.Booking> UpdateBooking(BookingStatus status, DateTime checkIn, DateTime checkOut, PaymentMethod paymentMethod, decimal totalAmount, decimal discount, decimal vat, decimal serviceCharge, string accountNumber)
        {
            var updatedBy = AuthSession.CurrentUser?.Id;
            var amount = Helper.GetPriceByRateAndTime(checkIn, checkOut, decimal.Parse(txtRoomRate.Text));

            _booking.CheckOut = new DateTime(checkOut.Year, checkOut.Month, checkOut.Day, 12, 0, 0);
            _booking.Amount = amount;
            _booking.Discount = discount;
            _booking.PaymentMethod = paymentMethod;
            _booking.TotalAmount += totalAmount;
            _booking.VAT = vat;
            _booking.ServiceCharge = serviceCharge;
            _booking.AccountNumber = accountNumber;
            _booking.Status = status;
            _booking.UpdatedBy = updatedBy;

            await _bookingRepository.UpdateBooking(_booking);
            return _booking;
        }

        private decimal CalculateCharges(decimal charge, decimal basePrice)
        {
            var total = basePrice * (charge / 100);
            return total;
        }

        private async Task HandlePostBookingAsync(Domain.Entities.FrontDesk.Booking booking, decimal amount, decimal basePrice)
        {
            var bookedRoom = await _roomRepository.GetRoomById(booking.RoomId);
            var bookedGuest = await _guestRepository.GetGuestByIdAsync(booking.GuestId);
            var bookingAccount = await _transactionRepository.GetBankAccountById(booking.AccountNumber);
            var hotel = await _hotelSettingsService.GetHotelInformation();
            var activeUser = await _applicationUserRoleRepository.GetUserById(AuthSession.CurrentUser!.Id);

            var transaction = await _transactionRepository.GetByBookingIdAsync(booking.Id);

            var tax = CalculateCharges(booking.VAT, basePrice);
            var serviceCharge = CalculateCharges(booking.ServiceCharge, basePrice);
            var discount = CalculateCharges(booking.Discount, basePrice);

            var transactionItem = new Domain.Entities.Transaction.TransactionItem
            {
                Amount = basePrice,
                ServiceId = booking.BookingId,
                TaxAmount = tax,
                ServiceCharge = serviceCharge,
                Discount = discount,
                TotalAmount = basePrice + tax + serviceCharge - discount,
                Category = Category.Accomodation,
                Type = TransactionType.Adjustment,
                Status = TransactionStatus.Unpaid,
                BankAccount = $"{bookingAccount.BankAccountNumber}",
                DateAdded = DateTime.Now,
                ApplicationUserId = AuthSession.CurrentUser?.Id,
                TransactionId = transaction?.Id,
                Description = $"Booking extended for {booking.Guest.FullName} in room {booking.Room.Number} from {booking.CheckIn} to {booking.CheckOut}"
            };

            await _transactionRepository.AddTransactionItemAsync(transactionItem);

            if (bookedRoom != null)
            {
                bookedRoom.Status = RoomStatus.Booked;
                await _roomRepository.UpdateRoom(bookedRoom);
            }

            if (hotel != null)
            {
                var isVerifyPayment = await _hotelSettingsService.GetSettingAsync("VerifyTransaction");
                if (isVerifyPayment != null)
                {
                    var value = isVerifyPayment.Value;
                    if (value != null && value.Equals("true", StringComparison.CurrentCultureIgnoreCase))
                    {
                        await VerifyPayment(hotel, booking, bookedGuest, activeUser, transaction!, transactionItem, amount, bookingAccount);
                    }
                    else
                    {
                        booking.Status = BookingStatus.Completed;

                        if (await CheckIfGuestHasAccount(bookedGuest))
                        {
                            await ChargeGuestAccount(bookedGuest, amount);
                            await AddGuestTransaction(bookedGuest, amount);

                            if (await CheckIfGuestHasMoney(bookedGuest, amount))
                            {
                                transactionItem.Status = TransactionStatus.Paid;
                            }
                            else
                            {
                                transactionItem.Status = TransactionStatus.Unpaid;
                                MessageBox.Show("Guest does not have enough balance to pay for the booking. Payment will be flagged as pending.", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
                            }
                        }

                        if(_isUnpaid)
                        {
                            booking.Receivables += amount;
                            transactionItem.Status = TransactionStatus.Unpaid;
                        }
                        else
                        {
                            transactionItem.Status = TransactionStatus.Paid;
                        }

                        await _transactionRepository.UpdateTransactionItemAsync(transactionItem);
                        await _bookingRepository.UpdateBooking(booking);

                        await SenderHelper.SendEmail(
                             bookedGuest.Email,
                             "Booking Extension Payment Receipt",
                             "guest_receipt",
                             new ReceiptVariable
                             {
                                 accountNumber = $"{bookingAccount.BankAccountNumber} ({bookingAccount.BankName}) | {bookingAccount.BankAccountName}",
                                 amount = booking.TotalAmount.ToString("N2"),
                                 guestName = bookedGuest.FullName,
                                 hotelName = hotel.Name,
                                 invoiceNumber = transaction!.InvoiceNumber,
                                 paymentMethod = booking.PaymentMethod.ToString(),
                                 receptionist = activeUser.FullName,
                                 receptionistContact = activeUser.PhoneNumber,
                                 service = booking.BookingId,
                             }
                         );
                    }
                }

            }
        }

        private async Task VerifyPayment(Hotel hotel, Domain.Entities.FrontDesk.Booking booking, Domain.Entities.FrontDesk.Guest bookedGuest, ApplicationUser activeUser, Transaction transaction, TransactionItem transactionItem, decimal amount, BankAccount bookingAccount)
        {
            var verificationCode = new VerificationCode
            {
                Code = string.Concat("BK", Guid.NewGuid().ToString().Split("-")[0].ToUpper().AsSpan(0, 5)),
                ServiceId = booking.BookingId,
                ApplicationUserId = AuthSession.CurrentUser?.Id
            };

            await _verificationCodeService.AddCode(verificationCode);

            var response = await SenderHelper.SendOtp(
                hotel.PhoneNumber,
                hotel.Name,
                $"{bookingAccount.BankAccountNumber} ({bookingAccount.BankName}) | {bookingAccount.BankAccountName}",
                bookedGuest.FullName,
                "Booking",
                verificationCode.Code,
                amount,
                booking.PaymentMethod.ToString(),
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
                    booking.BookingId,
                    amount,
                    _applicationUserRoleRepository
                );

                if (verifyPaymentWindow.ShowDialog() == true)
                {
                    booking.Status = BookingStatus.Completed;

                    transactionItem.Status = TransactionStatus.Paid;
                    await _transactionRepository.UpdateTransactionItemAsync(transactionItem);

                    await SenderHelper.SendEmail(
                        bookedGuest.Email,
                        "Booking Payment Receipt",
                        "guest_receipt",
                        new ReceiptVariable
                        {
                            accountNumber = $"{bookingAccount.BankAccountNumber} ({bookingAccount.BankName}) | {bookingAccount.BankAccountName}",
                            amount = booking.TotalAmount.ToString("N2"),
                            guestName = bookedGuest.FullName,
                            hotelName = hotel.Name,
                            invoiceNumber = transaction.InvoiceNumber,
                            paymentMethod = booking.PaymentMethod.ToString(),
                            receptionist = activeUser.FullName,
                            receptionistContact = activeUser.PhoneNumber,
                            service = booking.BookingId,
                        }
                    );
                }
                else
                {
                    booking.Status = BookingStatus.Pending;
                    booking.Receivables += amount;

                    await _verificationCodeService.DeleteAsync(verificationCode.Id);
                }

            }
            else
            {
                booking.Receivables += booking.TotalAmount;
                MessageBox.Show(
                    "Booking extended successfully but could not verify payment. Payment will be flagged as pending.",
                    "Info",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information
                );
            }

            await _bookingRepository.UpdateBooking(booking);
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
                await CalculateTotal();
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

        private async void txtDays_TextChanged(object sender, TextChangedEventArgs e)
        {
            if ((int.TryParse(txtDays.Text, out int days) || days > 1) && dtpCheckIn.SelectedDate != null)
            {
                if (days > (_booking.CheckOut.Date - _booking.CheckIn.Date).Days)
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
                        await CalculateTotal();
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
                    await CalculateTotal();
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
                    await CalculateTotal();
                }
            }
        }

        private async void txtServiceCharge_TextChanged(object sender, TextChangedEventArgs e)
        {
            bool isNull = Helper.AreAnyNullOrEmpty(txtServiceCharge.Text);
            if(!isNull)
            {
                if (dtpCheckIn.SelectedDate != null && dtpCheckOut.SelectedDate != null)
                {
                    await CalculateTotal();
                }
            }
            else
            {
                txtServiceCharge.Text = 0.ToString("N2");
            }
        }

        private async void txtDiscount_TextChanged(object sender, TextChangedEventArgs e)
        {
            bool isNull = Helper.AreAnyNullOrEmpty(txtDiscount.Text);
            if (!isNull)
            {
                if (dtpCheckIn.SelectedDate != null && dtpCheckOut.SelectedDate != null)
                {
                    await CalculateTotal();
                }
            }
            else
            {
                txtDiscount.Text = 0.ToString();
            }
        }

        private async void txtVAT_TextChanged(object sender, TextChangedEventArgs e)
        {
            bool isNull = Helper.AreAnyNullOrEmpty(txtVAT.Text);
            if(!isNull)
            {
                if (dtpCheckIn.SelectedDate != null && dtpCheckOut.SelectedDate != null)
                {
                    await CalculateTotal();
                }
            }
            else
            {
                txtVAT.Text = 0.ToString();
            }
        }

        public async Task CalculateTotal()
        {
            var totalPrice = Helper.GetPriceByRateAndTime(dtpCheckIn.SelectedDate!.Value, dtpCheckOut.SelectedDate!.Value, decimal.Parse(txtRoomRate.Text));
            var basePrice = Helper.CalculateTotal(totalPrice, decimal.Parse(txtDiscount.Text), decimal.Parse(txtVAT.Text), decimal.Parse(txtServiceCharge.Text));
            var totalAmount =  basePrice - _booking.TotalAmount;

            var currencySetting = await _hotelSettingsService.GetSettingAsync("CurrencySymbol");

            if (currencySetting != null)
                txtTotalAmount.Text = currencySetting?.Value + " " + totalAmount.ToString("N2");
            else
                txtTotalAmount.Text = "₦" + " " + totalAmount.ToString("N2");
        }

        private async Task<bool> CheckIfGuestHasAccount(Domain.Entities.FrontDesk.Guest guest)
        {
            LoaderOverlay.Visibility = Visibility.Visible;
            try
            {
                var guestAccount = await _guestRepository.GetGuestAccountByGuestIdAsync(guest.Id);
                if (guestAccount != null && !guestAccount.IsClosed)
                {
                    return true;
                }

                return false;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
            finally
            {
                LoaderOverlay.Visibility = Visibility.Collapsed;
            }
        }

        private async Task ChargeGuestAccount(Domain.Entities.FrontDesk.Guest guest, decimal amount)
        {
            LoaderOverlay.Visibility = Visibility.Visible;
            try
            {
                var guestAccount = await _guestRepository.GetGuestAccountByGuestIdAsync(guest.Id);

                if (guestAccount != null)
                {
                    guestAccount.FundedBalance -= amount;
                    guestAccount.TotalCharges += amount;
                    guestAccount.LastFunded = DateTime.Now;

                    await _guestRepository.UpdateGuestAccountAsync(guestAccount);
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

        private async Task AddGuestTransaction(Domain.Entities.FrontDesk.Guest guest, decimal amount)
        {
            LoaderOverlay.Visibility = Visibility.Visible;
            try
            {
                var guestTransaction = new GuestTransaction
                {
                    Id = Guid.NewGuid().ToString(),
                    GuestId = guest.Id,
                    Amount = amount,
                    TransactionType = TransactionType.Debit,
                    Description = "Booking Payment",
                    Date = DateTime.Now,
                    ApplicationUserId = AuthSession.CurrentUser?.Id
                };
                await _guestRepository.AddGuestTransactionAsync(guestTransaction);
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

        private void MarkAsUnPaid_Click(object sender, RoutedEventArgs e)
        {
            _isUnpaid = !_isUnpaid;
            UnpaidBtn.Background = _isUnpaid
                ? new SolidColorBrush(Colors.Red)
                : new SolidColorBrush(Colors.Black);
        }

        private async Task<bool> CheckIfGuestHasMoney(Domain.Entities.FrontDesk.Guest guest, decimal amount)
        {
            try
            {
                var guestAccount = await _guestRepository.GetGuestAccountByGuestIdAsync(guest.Id);
                if (guestAccount != null && guestAccount.FundedBalance > amount)
                {
                    return true;
                }
                return false;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
        }

        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            await LoadFinancialMetric();
            await LoadBankAccount();
            LoadPaymentMethod();
            LoadDefaultSetting();

            if (await CheckIfGuestHasAccount(_booking.Guest))
            {
                btnAddRecipe.Visibility = Visibility.Collapsed;
                this.Height = 550;
            }
        }
    }

}
