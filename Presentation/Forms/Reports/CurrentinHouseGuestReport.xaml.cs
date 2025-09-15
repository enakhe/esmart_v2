using ESMART.Application.Common.Dtos;
using ESMART.Application.Common.Interface;
using ESMART.Domain.Entities.FrontDesk;
using ESMART.Domain.ViewModels.FrontDesk;
using ESMART.Infrastructure.Services;
using ESMART.Presentation.Forms.Export;
using ESMART.Presentation.Forms.FrontDesk.Booking;
using ESMART.Presentation.Utils;
using System.Windows;
using System.Windows.Controls;

namespace ESMART.Presentation.Forms.Reports
{
    /// <summary>
    /// Interaction logic for CurrentinHouseGuestReport.xaml
    /// </summary>
    public partial class CurrentinHouseGuestReport : Page
    {
        private readonly ReportService _reportService;
        private readonly IHotelSettingsService _hotelSettingsService;

        public CurrentinHouseGuestReport(ReportService reportService, IHotelSettingsService hotelSettingsService)
        {
            InitializeComponent();
            _reportService = reportService;
            _hotelSettingsService = hotelSettingsService;
            this.DataContext = new HouseListDto();
        }

        private async Task<HouseListDto?> LoadData()
        {
            LoaderOverlay.Visibility = Visibility.Visible;
            try
            {
                var expectedDepartureBooking = await _reportService.GetHouseListReportAsync();

                if (expectedDepartureBooking != null)
                {
                    this.BookingDataGrid.ItemsSource = expectedDepartureBooking?.InHouseGuests;
                    this.DataContext = expectedDepartureBooking; // Ensure it updates correctly
                }

                return expectedDepartureBooking;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, ex.Source, MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                LoaderOverlay.Visibility = Visibility.Collapsed;
            }

            return null;
        }


        private async void Page_Loaded(object sender, RoutedEventArgs e)
        {
            await LoadData();
        }

        private async void PrintButton_Click(object sender, RoutedEventArgs e)
        {
            LoaderOverlay.Visibility = Visibility.Visible;
            try
            {
                var expectedDepartureBooking = await _reportService.GetHouseListReportAsync();
                var hotel = await _hotelSettingsService.GetHotelInformation();

                var printer = new PrintHelper();
                var doc = printer.GenerateHouseListReport(expectedDepartureBooking, hotel);

                string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmm");
                PrintHelper.PrintFlowDocument(doc, System.Printing.PageOrientation.Portrait);
                PrintHelper.SaveFlowDocumentToFile(doc, $"House List-{timestamp}");

            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occured when getting house list", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                LoaderOverlay.Visibility = Visibility.Collapsed;
            }
        }
    }
}
