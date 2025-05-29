using ESMART.Application.Common.Dtos;
using ESMART.Application.Common.Interface;
using ESMART.Application.Common.Utils;
using ESMART.Domain.Entities.FrontDesk;
using ESMART.Domain.Entities.RoomSettings;
using ESMART.Domain.Entities.Transaction;
using ESMART.Domain.Enum;
using ESMART.Domain.ViewModels.FrontDesk;
using ESMART.Domain.ViewModels.RoomSetting;
using ESMART.Infrastructure.Repositories.Configuration;
using ESMART.Infrastructure.Repositories.FrontDesk;
using ESMART.Infrastructure.Repositories.Transaction;
using ESMART.Infrastructure.Services;
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
        private readonly IHotelSettingsService _hotelSettingsService;
        private readonly IGuestRepository _guestRepository;
        private readonly ITransactionRepository _transactionRepository;
        private readonly GuestAccountService _guestAccountService;
        private readonly IndexPageViewModel _viewModel;
        private bool _suppressTextChanged = false;
        private readonly DispatcherTimer _formatTimer;
        public AddBulkBookingDialog(IRoomRepository roomRepository, IHotelSettingsService hotelSettingsService, IGuestRepository guestRepository, ITransactionRepository transactionRepository, GuestAccountService guestAccountService, IndexPageViewModel indexPageViewModel)
        {
            _roomRepository = roomRepository;
            _hotelSettingsService = hotelSettingsService;
            _viewModel = new IndexPageViewModel();
            _guestRepository = guestRepository;
            _transactionRepository = transactionRepository;
            _guestAccountService = guestAccountService;
            _viewModel = indexPageViewModel;
            this.DataContext = _viewModel;

            _formatTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(500)
            };
            _formatTimer.Tick += FormatTimer_Tick!;

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
                var allVacantRooms = await _roomRepository.GetAvailableRooms();

                var alreadySelectedIds = _viewModel.SelectedRooms.Select(r => r.Room.Id).ToHashSet();

                var selectable = allVacantRooms
                    .Where(r => !alreadySelectedIds.Contains(r.Id))
                    .Select(r => new SelectableRoomViewModel(r));

                foreach (var item in selectable)
                {
                    _viewModel.Rooms.Add(item);
                }
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

        private void RoomCard_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (sender is Border border && border.Tag is SelectableRoomViewModel room)
            {
                room.IsSelected = !room.IsSelected;

                if (room.IsSelected)
                {
                    // Ensure there's an occupant object (just in case)
                    if (room.Occupant == null)
                        room.Occupant = new RoomOccupantViewModel();

                    if (!_viewModel.SelectedRooms.Contains(room))
                    {
                        _viewModel.SelectedRooms.Add(room);
                        room.Occupant.OccupantName = _viewModel.MainGuestName;
                        room.Occupant.PhoneNumber = _viewModel.MainGuestPhoneNumber;
                    }
                }
                else
                {
                    if (_viewModel.SelectedRooms.Contains(room))
                        _viewModel.SelectedRooms.Remove(room);
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

            if (chkUseSameCheckout.IsChecked == true && dtpCheckOut.SelectedDate.HasValue)
            {
                var selectedTime = dtpCheckOut.SelectedDate.Value;
                foreach (var occupant in _viewModel.RoomOccupants)
                {
                    occupant.CheckoutTime = selectedTime;
                }
            }
        }

        private void chkUseSameCheckout_Unchecked(object sender, RoutedEventArgs e)
        {
            foreach (var room in _viewModel.SelectedRooms)
            {
                room.CheckoutTime = null; // or restore previous if you stored them
            }
        }

        private void chkUseSameCheckout_Checked(object sender, RoutedEventArgs e)
        {
            if (dtpCheckOut.SelectedDate == null)
                return;

            var sameTime = dtpCheckOut.SelectedDate.Value;

            foreach (var room in _viewModel.SelectedRooms)
            {
                room.CheckoutTime = sameTime;
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

        private bool ValidateInputs(out string guestId, out string roomId, out DateTime checkIn, out DateTime checkOut, out PaymentMethod paymentMethod, out decimal totalAmount, out decimal discount, out decimal vat, out decimal serviceCharge, out string accountNumber)
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
            checkIn = dtpCheckIn.SelectedDate!.Value;
            checkOut = dtpCheckOut.SelectedDate!.Value;
            paymentMethod = Enum.Parse<PaymentMethod>(cmbPaymentMethod.SelectedValue.ToString()!);
            discount = decimal.Parse(txtDiscount.Text.Replace("%", ""));
            vat = decimal.Parse(txtVAT.Text);
            serviceCharge = decimal.Parse(txtServiceCharge.Text);
            accountNumber = ((BankAccount)cmbAccountNumber.SelectedItem).Id;

            return true;
        }

        private async Task AddBooking()
        {
            try
            {
                var guest = (Domain.Entities.FrontDesk.Guest)cmbGuest.SelectedItem;

                if (!ValidateInputs(out string guestId, out string roomId, out DateTime checkIn, out DateTime checkOut, out PaymentMethod paymentMethod, out decimal totalAmount, out decimal discount, out decimal vat, out decimal serviceCharge, out string accountNumber))
                {
                    return;
                }

                var guestAccount = await _guestAccountService.GetAccountAsync(guestId);
                var activeUser = AuthSession.CurrentUser.Id;

                var multiRoomBooking = new MultiRoomBookingDto()
                {
                    ApplicationUserId = activeUser,
                    CheckIn = checkIn,
                    CheckOut = checkOut,
                    AccountNumber = accountNumber,
                    Discount = discount,
                    GuestAccountId = guestAccount.Id,
                    GuestId = guestId,
                    PaymentMethod = paymentMethod,
                };

                string bookingId = await _guestAccountService.CreateGuestBookingAsync(multiRoomBooking);

                var roomBookings = _viewModel.SelectedRooms.Select(roomVm => new RoomBooking
                {
                    RoomId = roomVm.Room.Id,
                    OccupantName = roomVm.Occupant.OccupantName,
                    OccupantPhoneNumber = roomVm.Occupant.PhoneNumber,
                    CheckIn = checkIn,
                    CheckOut = (DateTime)roomVm.CheckoutTime!,
                    Rate = roomVm.RackRate,
                    Date = DateTime.Now,
                    BookingId = bookingId,
                    Tax = roomVm.TaxRate,
                    Discount = roomVm.DiscountRate,
                    ServiceCharge = roomVm.ServiceChargeRate
                }).ToList();

                await _guestAccountService.AssignRoomsToBookingAsync(bookingId, activeUser, roomBookings);

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

        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            await LoadRoom();
            await LoadFinancialMetric();
            await LoadBankAccount();
            await LoadGuests();
            LoadPaymentMethod();
            LoadDefaultSetting();
        }
    }
}
