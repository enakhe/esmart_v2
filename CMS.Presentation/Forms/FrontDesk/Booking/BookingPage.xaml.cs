using ESMART.Application.Common.Interface;
using Microsoft.Extensions.DependencyInjection;
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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace ESMART.Presentation.Forms.FrontDesk.Booking
{
    /// <summary>
    /// Interaction logic for BookingPage.xaml
    /// </summary>
    public partial class BookingPage : Page
    {
        private readonly IBookingRepository _bookingRepository;
        public BookingPage(IBookingRepository bookingRepository)
        {
            _bookingRepository = bookingRepository;
            InitializeComponent();
        }

        private async Task LoadBooking()
        {
            try
            {
                LoaderOverlay.Visibility = Visibility.Visible;
                var allBooking = await _bookingRepository.GetAllBookingsAsync();

                if (allBooking != null)
                {
                    BookingDataGrid.ItemsSource = allBooking;
                    txtBookingCount.Text = allBooking.Count.ToString();
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

        private async void AddSingleBooking_Click(object sender, RoutedEventArgs e)
        {
            var services = new ServiceCollection();
            DependencyInjection.ConfigureServices(services);
            var serviceProvider = services.BuildServiceProvider();

            AddBookingDialog addBookingDialog = serviceProvider.GetRequiredService<AddBookingDialog>();
            if (addBookingDialog.ShowDialog() == true)
            {
                await LoadBooking();
            }
        }

        private async void Page_Loaded(object sender, RoutedEventArgs e)
        {
            await LoadBooking();
        }
    }
}
