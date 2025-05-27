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
using System.Windows.Threading;
using static System.Runtime.InteropServices.JavaScript.JSType;
using Category = ESMART.Domain.Enum.Category;

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
        private readonly IReservationRepository _reservationRepository;
        private readonly IVerificationCodeService _verificationCodeService;
        private readonly ITransactionRepository _transactionRepository;
        private readonly IApplicationUserRoleRepository _applicationUserRoleRepository;
        private bool _suppressTextChanged = false;
        private DispatcherTimer _formatTimer;

        public AddBookingDialog(IGuestRepository guestRepository, IRoomRepository roomRepository, IHotelSettingsService hotelSettingsService, IBookingRepository bookingRepository, IVerificationCodeService verificationCodeService, ITransactionRepository transactionRepository, IReservationRepository reservationRepository, IApplicationUserRoleRepository applicationUserRoleRepository)
        {
            _guestRepository = guestRepository;
            _roomRepository = roomRepository;
            _hotelSettingsService = hotelSettingsService;
            _verificationCodeService = verificationCodeService;
            _bookingRepository = bookingRepository;
            _transactionRepository = transactionRepository;
            _reservationRepository = reservationRepository;
            _applicationUserRoleRepository = applicationUserRoleRepository;
            InitializeComponent();

            _formatTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(500)
            };
            _formatTimer.Tick += FormatTimer_Tick;
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
            dtpCheckIn.SelectedDate = DateTime.Now;
            dtpCheckOut.SelectedDate = DateTime.Now.AddDays(1);
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

                var isBookingAllowed = await CheckIfRoomCanBeBooked(((Domain.Entities.RoomSettings.Room)cmbRoom.SelectedItem).Number, checkIn, checkOut);

                if (isBookingAllowed)
                {
                    var booking = await CreateBooking(guestId, roomId, checkIn, checkOut, paymentMethod, totalAmount, discount, vat, serviceCharge, accountNumber);

                    await HandlePostBookingAsync(booking);

                    MessageBox.Show("Booking added successfully", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                    this.DialogResult = true;
                }
                else
                {
                    MessageBox.Show("Room is already booked for the selected dates.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
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

        private async Task<Domain.Entities.FrontDesk.Booking> CreateBooking(string guestId, string roomId, DateTime checkIn, DateTime checkOut, PaymentMethod paymentMethod, decimal totalAmount, decimal discount, decimal vat, decimal serviceCharge, string accountNumber)
        {
            var createdBy = AuthSession.CurrentUser?.Id;
            var amount = Helper.GetPriceByRateAndTime(checkIn, checkOut, decimal.Parse(txtRoomRate.Text));

            var booking = new Domain.Entities.FrontDesk.Booking
            {
                BookingId = $"BK{Guid.NewGuid().ToString().Split('-')[0].ToUpper().AsSpan(0, 5)}",
                CheckIn = checkIn,
                CheckOut = new DateTime(checkOut.Year, checkOut.Month, checkOut.Day, 12, 0, 0),
                Amount = amount,
                Status = BookingStatus.Pending,
                AccountNumber = accountNumber,
                Discount = discount,
                VAT = vat,
                ServiceCharge = serviceCharge,
                TotalAmount = totalAmount,
                Balance = 0,
                PaymentMethod = paymentMethod,
                GuestId = guestId,
                RoomId = roomId,
                ApplicationUserId = createdBy,
                UpdatedBy = createdBy,
                DateCreated = DateTime.Now,
                DateModified = DateTime.Now,
            };

            await _bookingRepository.AddBooking(booking);
            return booking;
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
            guestId = string.Empty;
            roomId = string.Empty;
            checkIn = checkOut = DateTime.MinValue;
            paymentMethod = default;
            totalAmount = discount = vat = serviceCharge = 0;
            accountNumber = string.Empty;

            if (cmbGuest.SelectedItem == null || cmbRoom.SelectedItem == null)
            {
                MessageBox.Show("Please select a guest and a room.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }

            guestId = ((Domain.Entities.FrontDesk.Guest)cmbGuest.SelectedItem).Id;
            roomId = ((Domain.Entities.RoomSettings.Room)cmbRoom.SelectedItem).Id;
            checkIn = dtpCheckIn.SelectedDate!.Value;
            checkOut = dtpCheckOut.SelectedDate!.Value;
            paymentMethod = Enum.Parse<PaymentMethod>(cmbPaymentMethod.SelectedValue.ToString()!);
            totalAmount = decimal.Parse(txtTotalAmount.Text.Replace("NGN", ""));
            discount = decimal.Parse(txtDiscount.Text.Replace("%", ""));
            vat = decimal.Parse(txtVAT.Text);
            serviceCharge = decimal.Parse(txtServiceCharge.Text);
            accountNumber = ((BankAccount)cmbAccountNumber.SelectedItem).Id;

            return true;
        }

        private async Task HandlePostBookingAsync(Domain.Entities.FrontDesk.Booking booking)
        {
            var bookedRoom = await _roomRepository.GetRoomById(booking.RoomId);
            var bookedGuest = await _guestRepository.GetGuestByIdAsync(booking.GuestId);
            var bookingAccount = await _transactionRepository.GetBankAccountById(booking.AccountNumber);
            var hotel = await _hotelSettingsService.GetHotelInformation();
            var activeUser = await _applicationUserRoleRepository.GetUserById(AuthSession.CurrentUser!.Id);

            var result = CreateTransaction(
                booking, 
                decimal.Parse(txtRoomRate.Text), 
                decimal.Parse(txtVAT.Text), 
                decimal.Parse(txtServiceCharge.Text), 
                decimal.Parse(txtDiscount.Text), 
                bookingAccount.BankAccountNumber);

            if (bookedRoom != null)
            {
                bookedRoom.Status = RoomStatus.Booked;
                await _roomRepository.UpdateRoom(bookedRoom);
            }

            var isVerifyPayment = await _hotelSettingsService.GetSettingAsync("VerifyTransaction");
            if (isVerifyPayment != null)
            {
                var value = isVerifyPayment.Value;
                if (value != null && value.Equals("true", StringComparison.CurrentCultureIgnoreCase))
                {
                    await VerifyPayment(hotel, booking, bookedGuest, activeUser, result.Result.Item1, result.Result.Item2, bookingAccount);
                }
                else
                {
                    booking.Status = BookingStatus.Completed;

                    var isGuestAccount = await CheckIfGuestHasAccount(bookedGuest);

                    if (isGuestAccount)
                    {
                        await ChargeGuestAccount(bookedGuest, booking.TotalAmount);
                        await AddGuestTransaction(bookedGuest, booking.TotalAmount);

                        if (await CheckIfGuestHasMoney(bookedGuest, booking.TotalAmount))
                        {
                            result.Result.Item2.Status = TransactionStatus.Paid;
                        }
                        else
                        {
                            result.Result.Item2.Status = TransactionStatus.Unpaid;
                            MessageBox.Show("Guest does not have enough balance to pay for the booking. Payment will be flagged as pending.", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
                        }
                    }
                    else
                    {
                        result.Result.Item2.Status = TransactionStatus.Paid;
                    }

                    await _transactionRepository.UpdateTransactionItemAsync(result.Result.Item2);
                    await _bookingRepository.UpdateBooking(booking);
                }
            }
        }

        private async Task VerifyPayment(Hotel hotel, Domain.Entities.FrontDesk.Booking booking, Domain.Entities.FrontDesk.Guest bookedGuest, ApplicationUser activeUser, Transaction transaction, TransactionItem transactionItem, BankAccount bookingAccount)
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
                booking.TotalAmount,
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
                    booking.TotalAmount,
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
                    booking.Balance += booking.TotalAmount;
                    await _verificationCodeService.DeleteAsync(verificationCode.Id);
                }
            }
            else
            {
                booking.Balance += booking.TotalAmount;
                MessageBox.Show(
                    "Booking added successfully but could not verify payment. Payment will be flagged as pending.",
                    "Info",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information
                );
            }

            await _bookingRepository.UpdateBooking(booking);
        }

        private async Task<(Transaction, TransactionItem)> CreateTransaction(
            Domain.Entities.FrontDesk.Booking booking,
            decimal roomRate,
            decimal vat,
            decimal serviceCharge,
            decimal discount,
            string account)
        {
            var transactionId = $"TR{Guid.NewGuid().ToString().Split('-')[0].ToUpper().AsSpan(0, 5)}";
            var transaction = new Domain.Entities.Transaction.Transaction
            {
                TransactionId = transactionId,
                GuestId = booking.GuestId,
                BookingId = booking.Id,
                Date = DateTime.Now,
                InvoiceNumber = transactionId,
                ApplicationUserId = AuthSession.CurrentUser?.Id,
            };

            await _transactionRepository.AddTransactionAsync(transaction);

            var transactionItem = await CreateTransactionItem(booking, transaction, roomRate, vat, serviceCharge, discount, account);

            return (transaction, transactionItem);
        }

        private async Task<TransactionItem> CreateTransactionItem(
            Domain.Entities.FrontDesk.Booking booking, 
            Transaction transaction, 
            decimal roomRate, 
            decimal vat, 
            decimal serviceCharge, 
            decimal discount,
            string account)
        {
            var (RackRate, FinalTotal) = Helper.CalculateRackAndDiscountedTotal(
                roomRate,
                vat,
                serviceCharge,
                discount
            );

            var transactionItem = new TransactionItem
            {
                Amount = RackRate,
                ServiceId = booking.BookingId,
                TaxAmount = vat,
                ServiceCharge = serviceCharge,
                Discount = discount,
                BookingId = booking.Id,
                TotalAmount = FinalTotal,
                Category = Category.Accomodation,
                Invoice = transaction.InvoiceNumber,
                Type = TransactionType.Charge,
                Status = TransactionStatus.Unpaid,
                BankAccount = account,
                DateAdded = DateTime.Now,
                ApplicationUserId = AuthSession.CurrentUser?.Id,
                TransactionId = transaction.Id,
                Description = $"Room Charge (Inclusive of Inclusion)",
            };

            await _transactionRepository.AddTransactionItemAsync(transactionItem);

            return transactionItem;
        }


        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
        }

        private void DecimalInput_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !Regex.IsMatch(e.Text, @"[\d.]");
        }

        private void NumberOnly_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !IsTextNumeric(e.Text);
        }

        private bool IsTextNumeric(string text)
        {
            return text.All(char.IsDigit);
        }

        private async void cmbRoom_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cmbRoom.SelectedItem is Domain.Entities.RoomSettings.Room selectedRoom)
            {
                txtRoomRate.Text = selectedRoom.Rate.ToString();
                txtRoomRate.IsEnabled = false;

                if (dtpCheckIn.SelectedDate != null && dtpCheckOut.SelectedDate != null)
                {
                    await CalculateTotalPrice(decimal.Parse(txtRoomRate.Text));
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
                        await CalculateTotalPrice(decimal.Parse(txtRoomRate.Text));
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
                    await CalculateTotalPrice(decimal.Parse(txtRoomRate.Text));
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
                    await CalculateTotalPrice(decimal.Parse(txtRoomRate.Text));
                }
            }
        }

        private async void txtServiceCharge_TextChanged(object sender, TextChangedEventArgs e)
        {
            bool isNull = Helper.AreAnyNullOrEmpty(txtServiceCharge.Text);
            if(!isNull)
            {
                if (cmbRoom.SelectedItem is Domain.Entities.RoomSettings.Room selectedRoom)
                {
                    txtRoomRate.Text = selectedRoom.Rate.ToString();
                    txtRoomRate.IsEnabled = false;

                    if (dtpCheckIn.SelectedDate != null && dtpCheckOut.SelectedDate != null)
                    {
                        await CalculateTotalPrice(selectedRoom.Rate);
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
                        await CalculateTotalPrice(selectedRoom.Rate);
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
            if(!isNull)
            {
                if (cmbRoom.SelectedItem is Domain.Entities.RoomSettings.Room selectedRoom)
                {
                    txtRoomRate.Text = selectedRoom.Rate.ToString();
                    txtRoomRate.IsEnabled = false;

                    if (dtpCheckIn.SelectedDate != null && dtpCheckOut.SelectedDate != null)
                    {
                        await CalculateTotalPrice(selectedRoom.Rate);
                    }
                }
            }
            else
            {
                txtVAT.Text = 0.ToString();
            }
        }

        private void DecimalInput_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (_suppressTextChanged) return;

            _formatTimer.Stop();
            _formatTimer.Tag = sender;
            _formatTimer.Start();
        }

        private void FormatTimer_Tick(object sender, EventArgs e)
        {
            _formatTimer.Stop();

            var textBox = _formatTimer.Tag as TextBox;
            if (textBox == null || string.IsNullOrWhiteSpace(textBox.Text)) return;

            int caretIndex = textBox.CaretIndex;
            string unformatted = textBox.Text.Replace(",", "");

            if (decimal.TryParse(unformatted, out decimal value))
            {
                _suppressTextChanged = true;

                textBox.Text = string.Format(CultureInfo.InvariantCulture, "{0:N}", value);
                textBox.CaretIndex = Math.Min(caretIndex, textBox.Text.Length);

                _suppressTextChanged = false;
            }
        }

        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            await LoadGuests();
            await LoadRooms();
            await LoadFinancialMetric();
            await LoadBankAccount();
            LoadPaymentMethod();
            LoadDefaultSetting();
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

        private async Task CalculateTotalPrice(decimal rate)
        {
            var totalPrice = Helper.GetPriceByRateAndTime(dtpCheckIn.SelectedDate!.Value, dtpCheckOut.SelectedDate!.Value, rate);

            var currencySetting = await _hotelSettingsService.GetSettingAsync("CurrencySymbol");

            if (currencySetting != null)
                txtTotalAmount.Text = currencySetting?.Value + " " + Helper.CalculateTotal(totalPrice, decimal.Parse(txtDiscount.Text.Replace("%", "")), decimal.Parse(txtVAT.Text.Replace("%", "")), decimal.Parse(txtServiceCharge.Text.Replace("%", ""))).ToString("N2");
            else
                txtTotalAmount.Text = "₦" + " " + Helper.CalculateTotal(totalPrice, decimal.Parse(txtDiscount.Text.Replace("%", "")), decimal.Parse(txtVAT.Text.Replace("%", "")), decimal.Parse(txtServiceCharge.Text.Replace("%", ""))).ToString("N2");
        }

        private async Task<bool> CheckIfGuestHasAccount(Domain.Entities.FrontDesk.Guest guest)
        {
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
    }
}
