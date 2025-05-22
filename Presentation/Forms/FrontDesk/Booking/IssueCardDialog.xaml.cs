#nullable disable

using ESMART.Application.Common.Interface;
using ESMART.Application.Common.Utils;
using ESMART.Domain.Enum;
using ESMART.Presentation.LockSDK;
using System.Text;
using System.Windows;

namespace ESMART.Presentation.Forms.FrontDesk.Booking
{
    /// <summary>
    /// Interaction logic for IssueCardDialog.xaml
    /// </summary>
    public partial class IssueCardDialog : Window
    {
        private IBookingRepository _bookingRepository;
        private IGuestRepository _guestRepository;
        private IHotelSettingsService _hotelSettingsService;
        private Domain.Entities.FrontDesk.Booking _booking;
        public IssueCardDialog(IBookingRepository bookingRepository, IGuestRepository guestRepository, Domain.Entities.FrontDesk.Booking booking, IHotelSettingsService hotelSettingsService)
        {
            _bookingRepository = bookingRepository;
            _hotelSettingsService = hotelSettingsService;
            _guestRepository = guestRepository;
            _booking = booking;
            InitializeComponent();
        }

        private async Task LoadBookingData()
        {
            LoaderOverlay.Visibility = Visibility.Visible;
            try
            {
                var bookingDetails = await _bookingRepository.GetBookingByIdViewModel(_booking.Id);
                if (bookingDetails != null)
                {
                    this.DataContext = bookingDetails;
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

        private static int OpenPort(int port)
        {
            var st = LockSDKHeaders.LS_OpenPort(port);
            return st;
        }

        public static int CheckEncoder(LOCK_SETTING lockSetting)
        {
            Int16 locktype = (short)lockSetting;
            var st = LockSDKHeaders.TP_Configuration(locktype);
            return st;
        }

        private void RecycleCard_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                StringBuilder card_snr = new StringBuilder();
                CARD_INFO cardInfo = new CARD_INFO();
                byte[] cbuf = new byte[10000];
                cardInfo = new CARD_INFO();
                int result = LockSDKHeaders.LS_GetCardInformation(ref cardInfo, 0, 0, IntPtr.Zero);

                var cardNo = Helper.ByteArrayToString(cardInfo.CardNo);

                int st = LockSDKHeaders.TP_CancelCard(card_snr);
                if (st == 1)
                {
                    MessageBox.Show("Successfully recycled card", "", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    LockSDKMethods.CheckErr(st);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async void IssueButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var lockSetting = await _hotelSettingsService.GetSettingsByCategoryAsync("Operation Settings");
                if (lockSetting != null)
                {
                    var lockType = lockSetting.FirstOrDefault(x => x.Key == "LockType")?.Value;
                    if (lockType == "MIFI")
                    {
                        await IssueCardForMIFI();
                    }
                    else if (lockType == "RFID")
                    {
                        MessageBox.Show("RFID card issuing is not implemented yet", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                    else if (lockType == "PULMOS")
                    {
                        await IssueCardForMIFI();
                    }
                    else
                    {
                        MessageBox.Show("Invalid lock type selected", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
                else
                {
                    MessageBox.Show("Lock settings not found", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async Task IssueCardForMIFI()
        {
            LoaderOverlay.Visibility = Visibility.Visible;
            try
            {
                char[] card_snr = new char[100];
                string roomno = $"{_booking.Room?.Building?.Number}.{_booking.Room?.Floor?.Number}.{_booking.Room?.Number}";
                string intime = _booking.CheckIn.ToString("yyyy-MM-dd HH:mm:ss");
                String outtime = _booking.CheckOut.ToString("yyyy-MM-dd HH:mm:ss");

                CARD_FLAGS iflags = CARD_FLAGS.CF_CHECK_TIMESTAMP;

                if (LockSDKHeaders.PreparedIssue(card_snr) == false)
                    return;
                var st = LockSDKMethods.MakeGuestCard(card_snr, roomno, _booking.Room.Area.Number, "", intime, outtime, iflags);

                if (st == 1)
                {
                    _booking.Guest.Status = "Active";
                    await _guestRepository.UpdateGuestAsync(_booking.Guest);
                    string cardSnr = new string(card_snr);
                    MessageBox.Show($"Successfully issued card: {cardSnr}", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                    this.DialogResult = true;
                }
                else
                {
                    LockSDKMethods.CheckErr(st);
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

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;
        }

        private async void Window_Activated(object sender, EventArgs e)
        {
            await LoadBookingData();
            int checkEncoder = CheckEncoder(LOCK_SETTING.LOCK_TYPE_PULMOS);
            if (checkEncoder != 1)
            {
                LockSDKMethods.CheckErr(checkEncoder);
            }
        }
    }
}
