using ESMART.Application.Common.Interface;
using ESMART.Infrastructure.Repositories.Configuration;
using ESMART.Infrastructure.Repositories.Transaction;
using ESMART.Presentation.Forms.Export;
using ESMART.Presentation.Utils;
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

namespace ESMART.Presentation.Forms.Reports
{
    /// <summary>
    /// Interaction logic for ExpectedDepartureReport.xaml
    /// </summary>
    public partial class ExpectedDepartureReport : Page
    {
        private readonly IBookingRepository _bookingRepository;
        private readonly IHotelSettingsService _hotelSettingsService;
        public ExpectedDepartureReport(IBookingRepository bookingRepository, IHotelSettingsService hotelSettingsService)
        {
            _bookingRepository = bookingRepository;
            _hotelSettingsService = hotelSettingsService;
            InitializeComponent();
        }

        private void LoadDefaultSetting()
        {
            txtFrom.SelectedDate = DateTime.Now;
            txtTo.SelectedDate = DateTime.Now.AddDays(1);
        }

        private async Task LoadData()
        {
            LoaderOverlay.Visibility = Visibility.Visible;
            try
            {
                var expectedDepartureBooking = await _bookingRepository.GetExpectedDepartureBooking(DateTime.Now, DateTime.Now.AddDays(1));
                if (expectedDepartureBooking != null)
                {
                    this.BookingDataGrid.ItemsSource = expectedDepartureBooking;
                }

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, ex.Source, MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
            finally
            {
                LoaderOverlay.Visibility = Visibility.Collapsed;
            }
        }

        private async void ExportButton_Click(object sender, RoutedEventArgs e)
        {
            LoaderOverlay.Visibility = Visibility.Visible;
            try
            {
                var columnNames = BookingDataGrid.Columns
                    .Where(c => c.Header != null)
                    .Select(c => c.Header.ToString())
                    .Where(name => !string.IsNullOrWhiteSpace(name) && name != "Operation")
                    .ToList();

                var optionsWindow = new ExportDialog(columnNames);
                var result = optionsWindow.ShowDialog();

                if (result == true)
                {
                    var exportResult = optionsWindow.GetResult();
                    var hotel = await _hotelSettingsService.GetHotelInformation();

                    if (exportResult.SelectedColumns.Count == 0)
                    {
                        MessageBox.Show("Please select at least one column to export.", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                    }
                    else
                    {
                        if (hotel != null)
                        {
                            ExportHelper.ExportAndPrint(BookingDataGrid, exportResult.SelectedColumns, exportResult.ExportFormat, exportResult.FileName, hotel.LogoUrl!, hotel.Name, hotel.Email, hotel.PhoneNumber, hotel.Address);
                        }
                    }

                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, ex.Source, MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
            finally
            {
                LoaderOverlay.Visibility = Visibility.Collapsed;
            }
        }

        private async Task LoadExpectedDepartureBookingByDate()
        {
            LoaderOverlay.Visibility = Visibility.Visible;
            try
            {
                var fromDate = txtFrom.SelectedDate.Value!;
                var toDate = txtTo.SelectedDate.Value!;

                if (fromDate > toDate)
                {
                    MessageBox.Show("From date cannot be greater than To date", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                var expectedDepartureBooking = await _bookingRepository.GetExpectedDepartureBooking(fromDate, toDate);

                if (expectedDepartureBooking != null)
                {
                    this.BookingDataGrid.ItemsSource = expectedDepartureBooking;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, ex.Source, MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
            finally
            {
                LoaderOverlay.Visibility = Visibility.Collapsed;
            }
        }

        private async void FilterButton_Click(object sender, RoutedEventArgs e)
        {
            await LoadExpectedDepartureBookingByDate();
        }

        private async void Page_Loaded(object sender, RoutedEventArgs e)
        {
            LoadDefaultSetting();
            await LoadData();
        }
    }
}
