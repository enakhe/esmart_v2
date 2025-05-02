using ESMART.Application.Common.Interface;
using ESMART.Domain.Enum;
using ESMART.Presentation.LockSDK;
using System.Windows;

namespace ESMART.Presentation.Forms.FrontDesk.Booking
{
    /// <summary>
    /// Interaction logic for IssueCardDialog.xaml
    /// </summary>
    public partial class IssueCardDialog : Window
    {
        private IBookingRepository _bookingRepository;
        private Domain.Entities.FrontDesk.Booking _booking;
        public IssueCardDialog(IBookingRepository bookingRepository, Domain.Entities.FrontDesk.Booking booking)
        {
            _bookingRepository = bookingRepository;
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

        private void RecycleCard_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var st = LockSDKMethods.RecycleCard();
                if (st == 1)
                {
                    MessageBox.Show("Successfully recycled card", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
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

        private void IssueButton_Click(object sender, RoutedEventArgs e)
        {
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
                    this.DialogResult = true;

                else
                    LockSDKMethods.CheckErr(st);

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;
        }

        private async void Window_Activated(object sender, EventArgs e)
        {
            await LoadBookingData();
        }
    }
}
