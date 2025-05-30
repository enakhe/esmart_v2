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
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace ESMART.Presentation.Forms.FrontDesk.Booking
{
    /// <summary>
    /// Interaction logic for TransferGuestDialog.xaml
    /// </summary>
    public partial class TransferGuestDialog : Window
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

        public TransferGuestDialog(IGuestRepository guestRepository, IRoomRepository roomRepository, IHotelSettingsService hotelSettingsService, IBookingRepository bookingRepository, IVerificationCodeService verificationCodeService, ITransactionRepository transactionRepository, Domain.Entities.FrontDesk.Booking booking, IReservationRepository reservationRepository, IApplicationUserRoleRepository applicationUserRoleRepository)
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
                    var avalableTransferRooms = rooms.Where(r => r.Rate >= _booking.Room.Rate).ToList();

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
            LoaderOverlay.Visibility = Visibility.Visible;
            try
            {
                dtpCheckIn.SelectedDate = _booking.CheckIn;
                dtpCheckOut.SelectedDate = _booking.CheckOut;

                txtDays.Text = (dtpCheckOut.SelectedDate.Value - dtpCheckIn.SelectedDate.Value).Days.ToString();
                txtDiscount.Text = _booking.Discount.ToString();
                txtVAT.Text = _booking.VAT.ToString();
                txtServiceCharge.Text = _booking.ServiceCharge.ToString();
                cmbAccountNumber.SelectedValue = _booking.BankAccountId;
                cmbPaymentMethod.SelectedValue = _booking.PaymentMethod;
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

                if (cmbRoom.SelectedItem is not Domain.Entities.RoomSettings.Room selectedRoom)
                {
                    MessageBox.Show("Please select a room.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                decimal oldRate = _booking.Room.Rate;
                decimal newRate = selectedRoom.Rate;
                var checkIn = dtpCheckOut.SelectedDate!.Value;
                var checkOut = dtpCheckIn.SelectedDate!.Value;
                int numberOfDays = (checkIn - checkOut).Days;
                var accountNumber = ((BankAccount)cmbAccountNumber.SelectedItem).Id;
                var paymentMethod = Enum.Parse<PaymentMethod>(cmbPaymentMethod.SelectedValue.ToString()!);

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
                        ? $"The new room is ₦{differencePerDay:N2} higher per day. Total additional charge: ₦{totalDifference:N2}.\nDo you want to proceed?"
                        : $"The new room is ₦{Math.Abs(differencePerDay):N2} cheaper per day. Total refund/discount: ₦{Math.Abs(totalDifference):N2}.\nDo you want to proceed?";

                    if (MessageBox.Show(message, "Confirm Room Transfer", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.No)
                    {
                        return;
                    }
                }

                _booking.Room.Status = RoomStatus.Vacant;
                await _roomRepository.UpdateRoom(_booking.Room);

                var amount = Helper.GetPriceByRateAndTime(dtpCheckIn.SelectedDate.Value, dtpCheckOut.SelectedDate.Value, newRate);

                _booking.RoomId = selectedRoom.Id;
                _booking.Room = selectedRoom;
                _booking.Amount += amount;
                _booking.BankAccountId = accountNumber;
                _booking.PaymentMethod = paymentMethod;

                if (differencePerDay != 0)
                {
                    _booking.Amount += totalAmount;
                }

                var isRoomAvailable = await CheckIfRoomCanBeBooked(selectedRoom.Number, dtpCheckIn.SelectedDate.Value, dtpCheckOut.SelectedDate.Value);

                if (isRoomAvailable)
                {
                    await _bookingRepository.UpdateBooking(_booking);

                    _booking.Room.Status = RoomStatus.Booked;
                    await _roomRepository.UpdateRoom(_booking.Room);

                    await HandlePostBookingAsync(_booking, differencePerDay, totalAmount, amount);

                    MessageBox.Show("Guest transferred successfully.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                    this.DialogResult = true;
                }
                else
                {
                    MessageBox.Show("The selected room is not available for the specified dates.", "Room Unavailable", MessageBoxButton.OK, MessageBoxImage.Warning);
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

        private decimal CalculateCharges(decimal charge, decimal basePrice)
        {
            var total = basePrice * (charge / 100);
            return total;
        }

        private async Task HandlePostBookingAsync(Domain.Entities.FrontDesk.Booking booking, decimal differencePerDay, decimal amount, decimal basePrice)
        {

            if (differencePerDay != 0)
            {
                var bookedRoom = await _roomRepository.GetRoomById(booking.RoomId);
                var bookingAccount = await _transactionRepository.GetBankAccountById(booking.BankAccountId);
                var bookedGuest = await _guestRepository.GetGuestByIdAsync(booking.GuestId);
                var hotel = await _hotelSettingsService.GetHotelInformation();
                var activeUser = await _applicationUserRoleRepository.GetUserById(AuthSession.CurrentUser!.Id);


                var transaction = await _transactionRepository.GetByBookingIdAsync(booking.Id);

                var tax = CalculateCharges(booking.VAT, booking.Amount);
                var serviceCharge = CalculateCharges(booking.ServiceCharge, booking.Amount);
                var discount = CalculateCharges(booking.Discount, booking.Amount);

                var transactionItem = new Domain.Entities.Transaction.TransactionItem
                {
                    Amount = booking.Amount,
                    ServiceId = booking.BookingId,
                    TaxAmount = tax,
                    ServiceCharge = serviceCharge,
                    Discount = discount,
                    TotalAmount = booking.Amount + tax + serviceCharge - discount,
                    Category = Category.Accomodation,
                    Type = TransactionType.Adjustment,
                    Status = TransactionStatus.Unpaid,
                    BankAccount = $"{bookingAccount.BankAccountNumber}",
                    DateAdded = DateTime.Now,
                    ApplicationUserId = AuthSession.CurrentUser?.Id,
                    TransactionId = transaction?.Id,
                    Description = $"Booking room transfered for {booking.Guest.FullName} to {booking.Room.Number} from {booking.CheckIn} to {booking.CheckOut}"
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

                            if (_isUnpaid)
                            {
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
                                 "Booking Transfer Payment Receipt",
                                 "guest_receipt",
                                 new ReceiptVariable
                                 {
                                     accountNumber = $"{bookingAccount.BankAccountNumber} ({bookingAccount.BankName}) | {bookingAccount.BankAccountName}",
                                     amount = booking.Amount.ToString("N2"),
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
                        "Booking Room Transfer Receipt",
                        "guest_receipt",
                        new ReceiptVariable
                        {
                            accountNumber = $"{bookingAccount.BankAccountNumber} ({bookingAccount.BankName}) | {bookingAccount.BankAccountName}",
                            amount = booking.Amount.ToString("N2"),
                            guestName = bookedGuest.FullName,
                            hotelName = hotel.Name,
                            invoiceNumber = transaction.InvoiceNumber,
                            paymentMethod = booking.PaymentMethod.ToString("N2"),
                            receptionist = activeUser.FullName,
                            receptionistContact = activeUser.PhoneNumber,
                            service = booking.BookingId,
                        }
                    );
                }
                else
                {
                    booking.Status = BookingStatus.Active;

                    await _verificationCodeService.DeleteAsync(verificationCode.Id);
                }

            }
            else
            {
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

                if (_booking.Room.Rate == decimal.Parse(txtRoomRate.Text))
                {
                    amountToDisplay = 0;
                }
                else if (decimal.Parse(txtRoomRate.Text) > _booking.Room.Rate)
                {
                    amountToDisplay = newTotalAmount - _booking.Amount;
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

        private async void cmbRoom_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cmbRoom.SelectedItem is Domain.Entities.RoomSettings.Room selectedRoom)
            {
                txtRoomRate.Text = selectedRoom.Rate.ToString();
                txtRoomRate.IsEnabled = false;

                if (dtpCheckIn.SelectedDate != null && dtpCheckOut.SelectedDate != null)
                {
                    await CalculateTotal();
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
                        await CalculateTotal();
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
                        await CalculateTotal();
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
                        await CalculateTotal();
                    }
                }
            }
        }

        private async void txtServiceCharge_TextChanged(object sender, TextChangedEventArgs e)
        {
            bool isNull = Helper.AreAnyNullOrEmpty(txtServiceCharge.Text);
            if (!isNull)
            {
                if (cmbRoom.SelectedItem is Domain.Entities.RoomSettings.Room selectedRoom)
                {
                    txtRoomRate.Text = selectedRoom.Rate.ToString();
                    txtRoomRate.IsEnabled = false;

                    if (dtpCheckIn.SelectedDate != null && dtpCheckOut.SelectedDate != null)
                    {
                        await CalculateTotal();
                    }
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
                if (cmbRoom.SelectedItem is Domain.Entities.RoomSettings.Room selectedRoom)
                {
                    txtRoomRate.Text = selectedRoom.Rate.ToString();
                    txtRoomRate.IsEnabled = false;

                    if (dtpCheckIn.SelectedDate != null && dtpCheckOut.SelectedDate != null)
                    {
                        await CalculateTotal();
                    }
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
            if (!isNull)
            {
                if (cmbRoom.SelectedItem is Domain.Entities.RoomSettings.Room selectedRoom)
                {
                    txtRoomRate.Text = selectedRoom.Rate.ToString();
                    txtRoomRate.IsEnabled = false;

                    if (dtpCheckIn.SelectedDate != null && dtpCheckOut.SelectedDate != null)
                    {
                        await CalculateTotal();
                    }
                }
            }
            else
            {
                txtVAT.Text = 0.ToString();
            }
        }

        private async Task CalculateTotal()
        {
            var totalPrice = Helper.GetPriceByRateAndTime(dtpCheckIn.SelectedDate!.Value, dtpCheckOut.SelectedDate!.Value, decimal.Parse(txtRoomRate.Text));

            var currencySetting = await _hotelSettingsService.GetSettingAsync("CurrencySymbol");
            var currencySymbol = currencySetting?.Value ?? "₦";

            decimal newTotalAmount = Helper.CalculateTotal(
                totalPrice,
                decimal.Parse(txtDiscount.Text),
                decimal.Parse(txtVAT.Text),
                decimal.Parse(txtServiceCharge.Text)
            );

            decimal amountToDisplay;

            if (_booking.Room.Rate == decimal.Parse(txtRoomRate.Text))
            {
                amountToDisplay = 0;
            }
            else if (decimal.Parse(txtRoomRate.Text) > _booking.Room.Rate)
            {
                amountToDisplay = newTotalAmount - _booking.Amount;
            }
            else
            {
                amountToDisplay = 0;
            }

            txtTotalAmount.Text = currencySymbol + " " + amountToDisplay.ToString("N2");
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
                    //TransactionType = TransactionType.Debit,
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

        private void MarkAsUnPaid_Click(object sender, RoutedEventArgs e)
        {
            _isUnpaid = !_isUnpaid;
            UnpaidBtn.Background = _isUnpaid
                ? new SolidColorBrush(Colors.Red)
                : new SolidColorBrush(Colors.Black);
        }

        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            await LoadRooms();
            await LoadFinancialMetric();
            await LoadBankAccount();
            LoadDefaultSetting();
            LoadPaymentMethod();
        }
    }
}
