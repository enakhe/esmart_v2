using ESMART.Application.Common.Interface;
using ESMART.Application.Common.Utils;
using ESMART.Domain.Entities.FrontDesk;
using ESMART.Domain.Entities.Transaction;
using ESMART.Infrastructure.Repositories.Configuration;
using ESMART.Infrastructure.Services;
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
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace ESMART.Presentation.Forms.FrontDesk.Booking
{
    /// <summary>
    /// Interaction logic for EditBookingInfoDialog.xaml
    /// </summary>
    public partial class EditBookingInfoDialog : Window
    {
        private readonly RoomBooking _roomBooking;
        private readonly IHotelSettingsService _hotelSettingsService;
        private readonly IGuestRepository _guestRepository;
        private bool _suppressTextChanged = false;
        private readonly IRoomRepository _roomRepository;
        private DispatcherTimer _formatTimer;
        private readonly GuestAccountService _guestAccountService;
        public EditBookingInfoDialog(RoomBooking roomBooking, GuestAccountService guestAccountService, IRoomRepository roomRepository, IHotelSettingsService hotelSettingsService, IGuestRepository guestRepository)
        {
            _roomBooking = roomBooking;
            _roomRepository = roomRepository;
            _guestAccountService = guestAccountService;
            _hotelSettingsService = hotelSettingsService;
            _guestRepository = guestRepository;
            InitializeComponent();

            _formatTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(500)
            };
            _formatTimer.Tick += FormatTimer_Tick;
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

        public async void LoadData()
        {
            LoaderOverlay.Visibility = Visibility.Visible;
            try
            {
                var discountSetting = await _hotelSettingsService.GetSettingAsync("Discount");

                cmbRoom.SelectedValue = _roomBooking.RoomId;
                txtDiscount.Text = discountSetting!.Value;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error ocurred when getting booking info. {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                LoaderOverlay.Visibility = Visibility.Collapsed;
            }
        }

        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            await LoadRooms();
            LoadData();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
        }

        private async void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            LoaderOverlay.Visibility = Visibility.Visible;
            try
            {
                bool isNull = Helper.AreAnyNullOrEmpty(cmbRoom.Text);

                if(!isNull)
                {
                    var serviceChareSetting = await _hotelSettingsService.GetSettingAsync("ServiceCharge");
                    var vatSetting = await _hotelSettingsService.GetSettingAsync("VAT");
                    var activeUser = AuthSession.CurrentUser.Id;

                    var roomId = ((Domain.Entities.RoomSettings.Room)cmbRoom.SelectedItem).Id;
                    var room = await _roomRepository.GetRoomById(roomId);
                    var guest = _roomBooking.Booking.Guest;
                    var booking = _roomBooking.Booking;

                    var rate = room.Rate;
                    var vat = decimal.Parse(vatSetting!.Value);
                    var serviceCharge = decimal.Parse(serviceChareSetting!.Value);
                    var discount = decimal.Parse(txtDiscount.Text);

                    var (rack, discountPrce, serviceFeeAmount, tax, final) = Helper.CalculateRackAndDiscountedTotal(rate, vat, serviceCharge, discount);

                    var roomBookings = new RoomBooking
                    {
                        RoomId = room.Id,
                        OccupantName = _roomBooking.OccupantName,
                        OccupantPhoneNumber = _roomBooking.OccupantPhoneNumber,
                        CheckIn = _roomBooking.CheckIn,
                        CheckOut = _roomBooking.CheckOut!,
                        Rate = rack,
                        Date = DateTime.Now,
                        BookingId = booking.Id,
                        Tax = tax,
                        Discount = discountPrce,
                        ServiceCharge = serviceFeeAmount
                    };

                    await _guestAccountService.AssignRoomsToBookingAsync(booking.Id, activeUser, [roomBookings]);
                    await _guestAccountService.UpdateRoomBooking(_roomBooking);

                    MessageBox.Show($"Room booking information updated sucessfully", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                    this.DialogResult = true;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error ocurred when saving booking info. {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                LoaderOverlay.Visibility = Visibility.Collapsed;
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
    }
}
