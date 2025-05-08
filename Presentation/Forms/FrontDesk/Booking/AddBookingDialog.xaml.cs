using DocumentFormat.OpenXml.Wordprocessing;
using ESMART.Application.Common.Interface;
using ESMART.Application.Common.Utils;
using ESMART.Domain.Entities.RoomSettings;
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
        private bool _suppressTextChanged = false;

        public AddBookingDialog(IGuestRepository guestRepository, IRoomRepository roomRepository, IHotelSettingsService hotelSettingsService, IBookingRepository bookingRepository, IVerificationCodeService verificationCodeService, ITransactionRepository transactionRepository, IReservationRepository reservationRepository)
        {
            _guestRepository = guestRepository;
            _roomRepository = roomRepository;
            _hotelSettingsService = hotelSettingsService;
            _verificationCodeService = verificationCodeService;
            _bookingRepository = bookingRepository;
            _transactionRepository = transactionRepository;
            _reservationRepository = reservationRepository;
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

                var isBookingAllowed = await CheckIfRoomCanBeBooked(((Room)cmbRoom.SelectedItem).Number, checkIn, checkOut);

                if(isBookingAllowed)
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
                Receivables = 0,
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
            guestId = String.Empty;
            roomId = String.Empty;
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
            roomId = ((Room)cmbRoom.SelectedItem).Id;
            checkIn = dtpCheckIn.SelectedDate!.Value;
            checkOut = dtpCheckOut.SelectedDate!.Value;
            paymentMethod = Enum.Parse<PaymentMethod>(cmbPaymentMethod.SelectedValue.ToString()!);
            totalAmount = decimal.Parse(txtTotalAmount.Text.Replace("NGN", ""));
            discount = decimal.Parse(txtDiscount.Text);
            vat = decimal.Parse(txtVAT.Text);
            serviceCharge = decimal.Parse(txtServiceCharge.Text);
            accountNumber = cmbAccountNumber.Text;

            return true;
        }

        private async Task HandlePostBookingAsync(Domain.Entities.FrontDesk.Booking booking)
        {
            var bookedRoom = await _roomRepository.GetRoomById(booking.RoomId);
            var bookedGuest = await _guestRepository.GetGuestByIdAsync(booking.GuestId);
            var hotel = await _hotelSettingsService.GetHotelInformation();

            var transaction = new Domain.Entities.Transaction.Transaction
            {
                TransactionId = $"TR{Guid.NewGuid().ToString().Split('-')[0].ToUpper().AsSpan(0, 5)}",
                GuestId = booking.GuestId,
                BookingId = booking.Id,
                Date = DateTime.Now,
                InvoiceNumber = booking.BookingId,
                ApplicationUserId = AuthSession.CurrentUser?.Id,
            };

            await _transactionRepository.AddTransactionAsync(transaction);

            if (bookedRoom != null)
            {
                bookedRoom.Status = RoomStatus.Booked;
                await _roomRepository.UpdateRoom(bookedRoom);
            }

            if (hotel != null)
            {
                var verificationCode = new VerificationCode
                {
                    Code = string.Concat("BK", Guid.NewGuid().ToString().Split("-")[0].ToUpper().AsSpan(0, 5)),
                    ServiceId = booking.BookingId,
                    ApplicationUserId = AuthSession.CurrentUser?.Id
                };

                await _verificationCodeService.AddCode(verificationCode);

                var response = await SenderHelper.SendOtp(hotel.PhoneNumber, booking.AccountNumber, bookedGuest.FullName, "Booking", verificationCode.Code, booking.TotalAmount);
                if (response.IsSuccessStatusCode)
                {
                    var verifyPaymentWindow = new VerifyPaymentWindow(_verificationCodeService, _hotelSettingsService, _bookingRepository, _transactionRepository, booking.BookingId, booking.TotalAmount);
                    if (verifyPaymentWindow.ShowDialog() == true)
                    {
                        booking.Status = BookingStatus.Completed;
                    }
                    else
                    {
                        booking.Receivables += booking.TotalAmount;
                        await _verificationCodeService.DeleteAsync(verificationCode.Id);
                    }

                    await _bookingRepository.UpdateBooking(booking);
                }
                else
                {
                    MessageBox.Show("Booking added successfully but could not verify payment. Payment will be flagged as pending.",
                        "Info", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }

            var transactionItem = new Domain.Entities.Transaction.TransactionItem
            {
                Amount = booking.Amount,
                ServiceId = booking.BookingId,
                TaxAmount = booking.VAT,
                ServiceCharge = booking.ServiceCharge,
                Discount = booking.Discount,
                Category = Category.Accomodation,
                Type = TransactionType.Charge,
                BankAccount = booking.AccountNumber,
                DateAdded = DateTime.Now,
                ApplicationUserId = AuthSession.CurrentUser?.Id,
                TransactionId = transaction.Id,
                Description = $"Booking for {bookedGuest.FullName} in {bookedRoom?.Number} from {booking.CheckIn.ToShortDateString()} to {booking.CheckOut.ToShortDateString()}",
            };

            if (booking.Status == BookingStatus.Completed)
            {
                transactionItem.Status = TransactionStatus.Paid;
            }
            else
            {
                transactionItem.Status = TransactionStatus.Unpaid;
            }

            await _transactionRepository.AddTransactionItemAsync(transactionItem);
        }


        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
        }

        private void DecimalInput_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !Regex.IsMatch(e.Text, @"[\d.]");
        }

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
                    txtTotalAmount.Text = currencySetting?.Value + " " + Helper.CalculateTotal(totalPrice, decimal.Parse(txtDiscount.Text), decimal.Parse(txtVAT.Text), decimal.Parse(txtServiceCharge.Text)).ToString("N2");
                else
                    txtTotalAmount.Text = "₦" + " " + Helper.CalculateTotal(totalPrice, decimal.Parse(txtDiscount.Text), decimal.Parse(txtVAT.Text), decimal.Parse(txtServiceCharge.Text)).ToString("N2");
            }
        }

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
                        txtTotalAmount.Text = currencySetting?.Value + " " + Helper.CalculateTotal(totalPrice, decimal.Parse(txtDiscount.Text), decimal.Parse(txtVAT.Text), decimal.Parse(txtServiceCharge.Text)).ToString("N2");
                    else
                        txtTotalAmount.Text = "₦" + " " + Helper.CalculateTotal(totalPrice, decimal.Parse(txtDiscount.Text), decimal.Parse(txtVAT.Text), decimal.Parse(txtServiceCharge.Text)).ToString("N2");
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
                            txtTotalAmount.Text = currencySetting?.Value + " " + Helper.CalculateTotal(totalPrice, decimal.Parse(txtDiscount.Text), decimal.Parse(txtVAT.Text), decimal.Parse(txtServiceCharge.Text)).ToString("N2");
                        else
                            txtTotalAmount.Text = "₦" + " " + Helper.CalculateTotal(totalPrice, decimal.Parse(txtDiscount.Text), decimal.Parse(txtVAT.Text), decimal.Parse(txtServiceCharge.Text)).ToString("N2");
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
