using ESMART.Application.Common.Interface;
using ESMART.Application.Common.Utils;
using ESMART.Domain.Entities.FrontDesk;
using ESMART.Domain.Entities.Transaction;
using ESMART.Domain.Enum;
using ESMART.Domain.ViewModels.RoomSetting;
using ESMART.Infrastructure.Repositories.Configuration;
using ESMART.Infrastructure.Repositories.FrontDesk;
using ESMART.Infrastructure.Repositories.Transaction;
using ESMART.Presentation.Forms.Home;
using ESMART.Presentation.Forms.StockKeeping.Order;
using ESMART.Presentation.Session;
using System;
using System.Collections.Generic;
using System.Globalization;
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

namespace ESMART.Presentation.Forms.FrontDesk.Booking
{
    /// <summary>
    /// Interaction logic for AddBulkBookingDialog.xaml
    /// </summary>
    public partial class AddBulkBookingDialog : Window
    {
        private readonly IRoomRepository _roomRepository;
        private DispatcherTimer _formatTimer;
        private readonly IHotelSettingsService _hotelSettingsService;
        private readonly IGuestRepository _guestRepository;
        private readonly IBookingRepository _bookingRepository;
        private bool _suppressTextChanged = false;
        private readonly ITransactionRepository _transactionRepository;
        private readonly IReservationRepository _reservationRepository;
        private readonly IndexPageViewModel _viewModel;
        private bool _isUnpaid = false;
        public AddBulkBookingDialog(IRoomRepository roomRepository, IHotelSettingsService hotelSettingsService, IGuestRepository guestRepository, IBookingRepository bookingRepository, ITransactionRepository transactionRepository, IReservationRepository reservationRepository)
        {
            _roomRepository = roomRepository;
            _hotelSettingsService = hotelSettingsService;
            _viewModel = new IndexPageViewModel();
            _guestRepository = guestRepository;
            _bookingRepository = bookingRepository;
            _transactionRepository = transactionRepository;
            _reservationRepository = reservationRepository;
            this.DataContext = _viewModel;

            _formatTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(500)
            };
            _formatTimer.Tick += FormatTimer_Tick;

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

        public async Task LoadRoom()
        {
            try
            {
                var rooms = await _roomRepository.GetAvailableRooms();

                _viewModel.Rooms.Clear();

                foreach (var room in rooms)
                    _viewModel.Rooms.Add(new SelectableRoomViewModel(room));
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void DecimalInput_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (_suppressTextChanged) return;

            _formatTimer.Stop(); // restart timer
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

        private bool IsTextNumeric(string text)
        {
            return text.All(char.IsDigit);
        }

        private void NumberOnly_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !IsTextNumeric(e.Text);
        }

        private void RoomCard_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (sender is Border border && border.Tag is SelectableRoomViewModel room)
            {
                room.IsSelected = !room.IsSelected;

                if (room.IsSelected)
                    _viewModel.SelectedRooms.Add(room);
                else
                    _viewModel.SelectedRooms.Remove(room);

                // Recalculate total
                if (dtpCheckIn.SelectedDate.HasValue && dtpCheckOut.SelectedDate.HasValue)
                {
                    _viewModel.CalculateTotalAmount(
                        dtpCheckIn.SelectedDate.Value,
                        dtpCheckOut.SelectedDate.Value,
                        ParseDecimal(txtDiscount.Text),
                        ParseDecimal(txtVAT.Text),
                        ParseDecimal(txtServiceCharge.Text));
                }

                txtTotalAmount.Text = _viewModel.TotalAmount.ToString("N2");
            }
        }


        private decimal ParseDecimal(string text)
        {
            return decimal.TryParse(text, out var result) ? result : 0;
        }


        private void txtDays_TextChanged(object sender, TextChangedEventArgs e)
        {
            if ((int.TryParse(txtDays.Text, out int days) || days > 1) && dtpCheckIn.SelectedDate != null)
            {
                var checkOut = dtpCheckIn.SelectedDate.Value.AddDays(int.Parse(txtDays.Text));
                if (checkOut > dtpCheckIn.SelectedDate)
                {
                    dtpCheckOut.SelectedDate = checkOut;

                    // Recalculate total
                    if (dtpCheckIn.SelectedDate.HasValue && dtpCheckOut.SelectedDate.HasValue)
                    {
                        _viewModel.CalculateTotalAmount(
                            dtpCheckIn.SelectedDate.Value,
                            dtpCheckOut.SelectedDate.Value,
                            ParseDecimal(txtDiscount.Text),
                            ParseDecimal(txtVAT.Text),
                            ParseDecimal(txtServiceCharge.Text));
                    }

                    txtTotalAmount.Text = _viewModel.TotalAmount.ToString("N2");
                }
            }
        }

        private void dtpCheckOut_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            if (dtpCheckOut.SelectedDate != null && dtpCheckIn.SelectedDate != null)
            {
                if (dtpCheckOut.SelectedDate <= dtpCheckIn.SelectedDate)
                {
                    dtpCheckOut.SelectedDate = dtpCheckIn.SelectedDate.Value.AddDays(1);
                }

                var days = (dtpCheckOut.SelectedDate.Value.Date - dtpCheckIn.SelectedDate.Value.Date).Days;
                txtDays.Text = days.ToString();

                // Recalculate total
                if (dtpCheckIn.SelectedDate.HasValue && dtpCheckOut.SelectedDate.HasValue)
                {
                    _viewModel.CalculateTotalAmount(
                        dtpCheckIn.SelectedDate.Value,
                        dtpCheckOut.SelectedDate.Value,
                        ParseDecimal(txtDiscount.Text),
                        ParseDecimal(txtVAT.Text),
                        ParseDecimal(txtServiceCharge.Text));
                }

                txtTotalAmount.Text = _viewModel.TotalAmount.ToString("N2");
            }
        }

        private void dtpCheckIn_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            if (dtpCheckIn.SelectedDate != null)
            {
                dtpCheckOut.DisplayDateStart = dtpCheckIn.SelectedDate.Value.AddDays(1);

                if (dtpCheckOut.SelectedDate == null || dtpCheckOut.SelectedDate <= dtpCheckIn.SelectedDate)
                {
                    dtpCheckOut.SelectedDate = dtpCheckIn.SelectedDate.Value.AddDays(1);
                }

                // Recalculate total
                if (dtpCheckIn.SelectedDate.HasValue && dtpCheckOut.SelectedDate.HasValue)
                {
                    _viewModel.CalculateTotalAmount(
                        dtpCheckIn.SelectedDate.Value,
                        dtpCheckOut.SelectedDate.Value,
                        ParseDecimal(txtDiscount.Text),
                        ParseDecimal(txtVAT.Text),
                        ParseDecimal(txtServiceCharge.Text));
                }

                txtTotalAmount.Text = _viewModel.TotalAmount.ToString("N2");
            }
        }

        private async Task<Domain.Entities.FrontDesk.Booking> CreateBooking(string guestId, string roomId, DateTime checkIn, DateTime checkOut, PaymentMethod paymentMethod, decimal totalAmount, decimal discount, decimal vat, decimal serviceCharge, string accountNumber, decimal rate)
        {
            var createdBy = AuthSession.CurrentUser?.Id;
            var amount = Helper.GetPriceByRateAndTime(checkIn, checkOut, rate);

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

        private bool ValidateInputs(out string guestId, out string roomId, out DateTime checkIn, out DateTime checkOut, out PaymentMethod paymentMethod, out decimal totalAmount, out decimal discount, out decimal vat, out decimal serviceCharge, out string accountNumber, Domain.Entities.RoomSettings.Room room)
        {
            guestId = string.Empty;
            roomId = string.Empty;
            checkIn = checkOut = DateTime.MinValue;
            paymentMethod = default;
            totalAmount = discount = vat = serviceCharge = 0;
            accountNumber = string.Empty;

            if (cmbGuest.SelectedItem == null || _viewModel.SelectedRooms.Count < 1)
            {
                MessageBox.Show("Please select a guest and a room.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }

            guestId = ((Domain.Entities.FrontDesk.Guest)cmbGuest.SelectedItem).Id;
            roomId = room.Id;
            checkIn = dtpCheckIn.SelectedDate!.Value;
            checkOut = dtpCheckOut.SelectedDate!.Value;
            paymentMethod = Enum.Parse<PaymentMethod>(cmbPaymentMethod.SelectedValue.ToString()!);
            discount = decimal.Parse(txtDiscount.Text.Replace("%", ""));
            vat = decimal.Parse(txtVAT.Text);
            serviceCharge = decimal.Parse(txtServiceCharge.Text);
            accountNumber = ((BankAccount)cmbAccountNumber.SelectedItem).Id;
            totalAmount = Helper.CalculateTotal(room.Rate, discount, vat, serviceCharge);

            return true;
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
                    Description = "Order Payment",
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

        private async Task AddBooking()
        {
            try
            {
                var guest = (Domain.Entities.FrontDesk.Guest)cmbGuest.SelectedItem;

                foreach (var room in _viewModel.SelectedRooms)
                {
                    if (!await CheckIfRoomCanBeBooked(room.Room.Number, dtpCheckIn.SelectedDate!.Value, dtpCheckOut.SelectedDate!.Value))
                    {
                        MessageBox.Show($"Room {room.Room.Number} is already booked for the selected dates.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }

                    if (!ValidateInputs(out string guestId, out string roomId, out DateTime checkIn, out DateTime checkOut, out PaymentMethod paymentMethod, out decimal totalAmount, out decimal discount, out decimal vat, out decimal serviceCharge, out string accountNumber, room.Room))
                    {
                        return;
                    }

                    var booking = await CreateBooking(guestId, roomId, checkIn, checkOut, paymentMethod, totalAmount, discount, vat, serviceCharge, accountNumber, room.Room.Rate);

                    var bookedRoom = await _roomRepository.GetRoomById(roomId);
                    bookedRoom.Status = Domain.Entities.RoomSettings.RoomStatus.Booked;
                    await _roomRepository.UpdateRoom(bookedRoom);

                    if (await CheckIfGuestHasAccount(guest))
                    {
                        await ChargeGuestAccount(guest, _viewModel.TotalAmount);
                        await AddGuestTransaction(guest, _viewModel.TotalAmount);

                        if (await CheckIfGuestHasMoney(guest, _viewModel.TotalAmount))
                        {
                            _isUnpaid = false;
                        }
                        else
                        {
                            _isUnpaid = true;
                            MessageBox.Show("Guest does not have enough balance to pay for the booking. Payment will be flagged as pending.", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
                        }
                    }

                    await CreateTransaction(guestId, booking, _isUnpaid);
                }
                MessageBox.Show("Booking created successfully.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                this.DialogResult = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async void btnAddBooking_Click(object sender, RoutedEventArgs e)
        {
            if (_viewModel.SelectedRooms.Count < 1)
            {
                MessageBox.Show("Please select at least one room.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (cmbGuest.SelectedItem == null)
            {
                MessageBox.Show("Please select a guest.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            await AddBooking();
        }

        private void MarkAsUnPaid_Click(object sender, RoutedEventArgs e)
        {
            _isUnpaid = !_isUnpaid;
            UnpaidBtn.Background = _isUnpaid
                ? new SolidColorBrush(Colors.Red)
                : new SolidColorBrush(Colors.Black);
        }

        private async Task CreateTransaction(string guestId, Domain.Entities.FrontDesk.Booking booking, bool isUnPaid)
        {
            try
            {
                var transaction = new Domain.Entities.Transaction.Transaction
                {
                    TransactionId = $"TR{Guid.NewGuid().ToString().Split('-')[0].ToUpper().AsSpan(0, 5)}",
                    GuestId = guestId,
                    BookingId = booking.Id,
                    Date = DateTime.Now,
                    InvoiceNumber = booking.BookingId,
                    ApplicationUserId = AuthSession.CurrentUser?.Id,
                };

                await _transactionRepository.AddTransactionAsync(transaction);
                await CreateTransactionItem(guestId, isUnPaid, booking.BookingId, transaction.Id, booking);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }       

        private async Task CreateTransactionItem(string guestId, bool isUnPaid, string serviceId, string transactionId, Domain.Entities.FrontDesk.Booking booking)
        {
            try
            {
                var transactionItem = new TransactionItem()
                {
                    Id = Guid.NewGuid().ToString(),
                    TransactionId = transactionId,
                    Amount = booking.Amount,
                    Description = $"Booking for {booking.Guest?.FullName} in {booking.Room?.Number} from {booking.CheckIn.ToShortDateString()} to {booking.CheckOut.ToShortDateString()}",
                    DateAdded = DateTime.Now,
                    BankAccount = booking.AccountNumber,
                    Category = Category.Accomodation,
                    ServiceId = serviceId,
                    Type = TransactionType.Charge,
                    Status = TransactionStatus.Unpaid,
                    Discount = 0,
                    TaxAmount = 0,
                    ServiceCharge = 0,
                    TotalAmount = Helper.CalculateTotal(booking.Amount, booking.Discount, booking.VAT, booking.ServiceCharge),
                    ApplicationUserId = AuthSession.CurrentUser?.Id
                };

                if (!isUnPaid)
                {
                    transactionItem.Status = TransactionStatus.Paid;
                }

                await _transactionRepository.AddTransactionItemAsync(transactionItem);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            await LoadRoom();
            await LoadFinancialMetric();
            await LoadBankAccount();
            await LoadGuests();
            LoadPaymentMethod();
            LoadDefaultSetting();
        }

        private async void cmbGuest_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var guestId = ((Domain.Entities.FrontDesk.Guest)cmbGuest.SelectedItem).Id;

            if (guestId != null)
            {
                var guest = await _guestRepository.GetGuestByIdAsync(guestId);
                if (guest != null)
                {
                    if (await CheckIfGuestHasAccount(guest))
                    {
                        btnAddRecipe.Visibility = Visibility.Collapsed;
                    }
                    else
                    {
                        btnAddRecipe.Visibility = Visibility.Visible;
                    }
                }
            }
        }
    }
}
