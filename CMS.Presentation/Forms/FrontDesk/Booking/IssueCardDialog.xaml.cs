using ESMART.Application.Common.Interface;
using ESMART.Domain.Entities.FrontDesk;
using ESMART.Presentation.LockSDK;
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

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
        }

        private async void Window_Activated(object sender, EventArgs e)
        {
            await LoadBookingData();
        }
    }
}
