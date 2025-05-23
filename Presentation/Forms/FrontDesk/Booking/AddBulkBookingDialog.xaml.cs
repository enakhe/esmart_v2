using ESMART.Application.Common.Interface;
using ESMART.Application.Common.Utils;
using ESMART.Domain.Enum;
using ESMART.Domain.ViewModels.RoomSetting;
using ESMART.Infrastructure.Repositories.Configuration;
using ESMART.Infrastructure.Repositories.FrontDesk;
using ESMART.Infrastructure.Repositories.Transaction;
using ESMART.Presentation.Forms.Home;
using System;
using System.Collections.Generic;
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
        private readonly IBookingRepository _bookingRepository;
        private readonly ITransactionRepository _transactionRepository;
        private readonly IndexPageViewModel _viewModel;
        public AddBulkBookingDialog(IRoomRepository roomRepository, IHotelSettingsService hotelSettingsService, IGuestRepository guestRepository, IBookingRepository bookingRepository, ITransactionRepository transactionRepository)
        {
            _roomRepository = roomRepository;
            _hotelSettingsService = hotelSettingsService;
            _viewModel = new IndexPageViewModel();
            _guestRepository = guestRepository;
            _bookingRepository = bookingRepository;
            _transactionRepository = transactionRepository;
            this.DataContext = _viewModel;
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
                                txtVAT.Text = decimal.Parse(setting.Value).ToString("N0");
                                break;
                            case "ServiceCharge":
                                txtServiceCharge.Text = decimal.Parse(setting.Value).ToString("N0");
                                break;
                            case "Discount":
                                txtDiscount.Text = decimal.Parse(setting.Value).ToString("N0");
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


        private bool IsTextNumeric(string text)
        {
            return text.All(char.IsDigit);
        }

        private void NumberOnly_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !IsTextNumeric(e.Text);
        }

        private void EditVATButton_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            txtVAT.IsEnabled = true;
        }

        private void EditServiceButton_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            txtServiceCharge.IsEnabled = true;
        }

        private void EditDiscountButton_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            txtDiscount.IsEnabled = true;
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
