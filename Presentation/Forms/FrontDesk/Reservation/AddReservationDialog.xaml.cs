﻿using ESMART.Application.Common.Interface;
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
    /// Interaction logic for AddReservationDialog.xaml
    /// </summary>
    public partial class AddReservationDialog : Window
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

        public AddReservationDialog(IGuestRepository guestRepository, IRoomRepository roomRepository, IHotelSettingsService hotelSettingsService, IBookingRepository bookingRepository, IVerificationCodeService verificationCodeService, ITransactionRepository transactionRepository, IReservationRepository reservationRepository, IApplicationUserRoleRepository applicationUserRoleRepository)
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
            dtpArrivalDate.DisplayDateStart = DateTime.Today;
            dtpArrivalDate.SelectedDate = DateTime.Now;
            dtpDepartureDate.SelectedDate = DateTime.Now.AddDays(1);
        }

        private async void Save_Click(object sender, RoutedEventArgs e)
        {
            LoaderOverlay.Visibility = Visibility.Visible;
            try
            {
                if (!ValidateInputs(out var guestId, out var roomId, out var checkIn, out var checkOut, out var paymentMethod, out var totalAmount, out var amountPaid, out var discount, out var vat, out var serviceCharge, out var accountNumber))
                {
                    MessageBox.Show("Please fill in all required fields.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                var isRoomAvailable = await CheckIfRoomCanBeBooked(((Domain.Entities.RoomSettings.Room)cmbRoom.SelectedItem).Number, checkIn, checkOut);

                if (isRoomAvailable)
                {
                    var reservation = await CreateReservation(
                        guestId, roomId, checkIn, checkOut, paymentMethod, totalAmount, discount, vat, serviceCharge, accountNumber, amountPaid
                        );

                    await HandlePostBookingAsync(reservation);

                    MessageBox.Show("Reservation added successfully", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                    this.DialogResult = true;
                }
                else
                {
                    MessageBox.Show("Room is not available for the selected dates.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
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

        private async Task<Domain.Entities.FrontDesk.Reservation> CreateReservation(string guestId, string roomId, DateTime checkIn, DateTime checkOut, PaymentMethod paymentMethod, decimal totalAmount, decimal discount, decimal vat, decimal serviceCharge, string accountNumber, decimal amountPaid)
        {
            var createdBy = AuthSession.CurrentUser?.Id;
            var amount = Helper.GetPriceByRateAndTime(checkIn, checkOut, decimal.Parse(txtRoomRate.Text));

            var reservation = new Domain.Entities.FrontDesk.Reservation
            {
                ReservationId = $"BK{Guid.NewGuid().ToString().Split('-')[0].ToUpper().AsSpan(0, 5)}",
                ArrivateDate = checkIn,
                DepartureDate = new DateTime(checkOut.Year, checkOut.Month, checkOut.Day, 12, 0, 0),
                Amount = amount,
                Status = ReservationStatus.Tentative,
                TransactionStatus = TransactionStatus.Pending,
                AccountNumber = accountNumber,
                Discount = discount,
                VAT = vat,
                ServiceCharge = serviceCharge,
                TotalAmount = totalAmount,
                PaymentMethod = paymentMethod,
                GuestId = guestId,
                RoomId = roomId,
                ApplicationUserId = createdBy,
                DateAdded = DateTime.Now,
                DateModified = DateTime.Now,
            };

            await _reservationRepository.AddReservationAsync(reservation);
            return reservation;
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

        private bool ValidateInputs(out string guestId, out string roomId, out DateTime checkIn, out DateTime checkOut, out PaymentMethod paymentMethod, out decimal totalAmount, out decimal amountPaid, out decimal discount, out decimal vat, out decimal serviceCharge, out string accountNumber)
        {
            guestId = String.Empty;
            roomId = String.Empty;
            checkIn = checkOut = DateTime.MinValue;
            paymentMethod = default;
            totalAmount = amountPaid = discount = vat = serviceCharge = 0;
            accountNumber = string.Empty;

            if (cmbGuest.SelectedItem == null || cmbRoom.SelectedItem == null)
            {
                MessageBox.Show("Please select a guest and a room.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }

            guestId = ((Domain.Entities.FrontDesk.Guest)cmbGuest.SelectedItem).Id;
            roomId = ((Domain.Entities.RoomSettings.Room)cmbRoom.SelectedItem).Id;
            checkIn = dtpArrivalDate.SelectedDate!.Value;
            checkOut = dtpDepartureDate.SelectedDate!.Value;
            paymentMethod = Enum.Parse<PaymentMethod>(cmbPaymentMethod.SelectedValue.ToString()!);
            totalAmount = decimal.Parse(txtTotalAmount.Text.Replace("NGN", ""));
            discount = decimal.Parse(txtDiscount.Text);
            vat = decimal.Parse(txtVAT.Text);
            serviceCharge = decimal.Parse(txtServiceCharge.Text);
            accountNumber = ((BankAccount)cmbAccountNumber.SelectedItem).Id;

            return true;
        }

        private async Task HandlePostBookingAsync(Domain.Entities.FrontDesk.Reservation reservation)
        {
            var reservedRoom = await _roomRepository.GetRoomById(reservation.RoomId);
            var reservedAccount = await _transactionRepository.GetBankAccountById(reservation.AccountNumber);
            var reservedGuest = await _guestRepository.GetGuestByIdAsync(reservation.GuestId);
            var hotel = await _hotelSettingsService.GetHotelInformation();
            var activeUser = await _applicationUserRoleRepository.GetUserById(AuthSession.CurrentUser!.Id);

            var transaction = new Domain.Entities.Transaction.Transaction
            {
                TransactionId = $"TR{Guid.NewGuid().ToString().Split('-')[0].ToUpper().AsSpan(0, 5)}",
                GuestId = reservation.GuestId,
                Date = DateTime.Now,
                InvoiceNumber = reservation.ReservationId,
                ApplicationUserId = AuthSession.CurrentUser?.Id,
            };

            await _transactionRepository.AddTransactionAsync(transaction);

            var transactionItem = new Domain.Entities.Transaction.TransactionItem
            {
                Amount = reservation.Amount,
                ServiceId = reservation.ReservationId,
                TaxAmount = reservation.VAT,
                ServiceCharge = reservation.ServiceCharge,
                Discount = reservation.Discount,
                Category = Category.Reservation,
                //Type = TransactionType.Charge,
                BankAccount = $"{reservedAccount.BankAccountNumber} ({reservedAccount.BankName}) | {reservedAccount.BankAccountName}",
                Status = TransactionStatus.Paid,
                DateAdded = DateTime.Now,
                ApplicationUserId = AuthSession.CurrentUser?.Id,
                TransactionId = transaction.Id,
                Description = $"Reservation for {reservedGuest.FullName} in {reservedRoom?.Number} from {reservation.ArrivateDate.ToShortDateString()} to {reservation.DepartureDate.ToShortDateString()}",
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
                             reservedGuest.Email,
                             "Reservation Payment Receipt",
                             "guest_receipt",
                             new ReceiptVariable
                             {
                                 accountNumber = $"{reservedAccount.BankAccountNumber} ({reservedAccount.BankName}) | {reservedAccount.BankAccountName}",
                                 amount = reservation.TotalAmount.ToString("N2"),
                                 guestName = reservedGuest.FullName,
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

            if (dtpArrivalDate.SelectedDate != null && dtpDepartureDate.SelectedDate != null)
            {
                var totalPrice = Helper.GetPriceByRateAndTime(dtpArrivalDate.SelectedDate.Value, dtpDepartureDate.SelectedDate.Value, decimal.Parse(txtRoomRate.Text));

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

        private async void cmbRoom_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cmbRoom.SelectedItem is Domain.Entities.RoomSettings.Room selectedRoom)
            {
                txtRoomRate.Text = selectedRoom.Rate.ToString();
                txtRoomRate.IsEnabled = false;

                if (dtpArrivalDate.SelectedDate != null && dtpDepartureDate.SelectedDate != null)
                {
                    var totalPrice = Helper.GetPriceByRateAndTime(dtpArrivalDate.SelectedDate.Value, dtpDepartureDate.SelectedDate.Value, selectedRoom.Rate);

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
            if ((int.TryParse(txtDays.Text, out int days) || days > 1) && dtpArrivalDate.SelectedDate != null)
            {
                var checkOut = dtpArrivalDate.SelectedDate.Value.AddDays(int.Parse(txtDays.Text));
                if (checkOut > dtpArrivalDate.SelectedDate)
                {
                    dtpDepartureDate.SelectedDate = checkOut;

                    bool isNull = Helper.AreAnyNullOrEmpty(txtRoomRate.Text);

                    if (!isNull)
                    {
                        var totalPrice = Helper.GetPriceByRateAndTime(dtpArrivalDate.SelectedDate.Value, dtpDepartureDate.SelectedDate.Value, decimal.Parse(txtRoomRate.Text));

                        var currencySetting = await _hotelSettingsService.GetSettingAsync("CurrencySymbol");

                        if (currencySetting != null)
                            txtTotalAmount.Text = currencySetting?.Value + " " + Helper.CalculateTotal(totalPrice, decimal.Parse(txtDiscount.Text), decimal.Parse(txtVAT.Text), decimal.Parse(txtServiceCharge.Text)).ToString("N2");
                        else
                            txtTotalAmount.Text = "₦" + " " + Helper.CalculateTotal(totalPrice, decimal.Parse(txtDiscount.Text), decimal.Parse(txtVAT.Text), decimal.Parse(txtServiceCharge.Text)).ToString("N2");
                    }
                }
            }
        }

        private async void dtpDepartureDate_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            if (dtpDepartureDate.SelectedDate != null && dtpArrivalDate.SelectedDate != null)
            {
                if (dtpDepartureDate.SelectedDate <= dtpArrivalDate.SelectedDate)
                {
                    dtpDepartureDate.SelectedDate = dtpArrivalDate.SelectedDate.Value.AddDays(1);
                }

                var days = (dtpDepartureDate.SelectedDate.Value.Date - dtpArrivalDate.SelectedDate.Value.Date).Days;
                txtDays.Text = days.ToString();

                bool isNull = Helper.AreAnyNullOrEmpty(txtRoomRate.Text);

                if (!isNull)
                {
                    var totalPrice = Helper.GetPriceByRateAndTime(
                        dtpArrivalDate.SelectedDate.Value,
                        dtpDepartureDate.SelectedDate.Value,
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


        private async void dtpArrivalDate_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            if (dtpArrivalDate.SelectedDate != null)
            {
                dtpDepartureDate.DisplayDateStart = dtpArrivalDate.SelectedDate.Value.AddDays(1);

                if (dtpDepartureDate.SelectedDate == null || dtpDepartureDate.SelectedDate <= dtpArrivalDate.SelectedDate)
                {
                    dtpDepartureDate.SelectedDate = dtpArrivalDate.SelectedDate.Value.AddDays(1);
                }

                var days = (dtpDepartureDate.SelectedDate.Value.Date - dtpArrivalDate.SelectedDate.Value.Date).Days;
                txtDays.Text = days.ToString();

                bool isNull = Helper.AreAnyNullOrEmpty(txtRoomRate.Text);

                if (!isNull)
                {
                    var totalPrice = Helper.GetPriceByRateAndTime(dtpArrivalDate.SelectedDate.Value, dtpDepartureDate.SelectedDate.Value, decimal.Parse(txtRoomRate.Text));

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

        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            await LoadGuests();
            await LoadRooms();
            await LoadFinancialMetric();
            await LoadBankAccount();
            LoadPaymentMethod();
            LoadDefaultSetting();
        }
    }
}