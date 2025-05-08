using ESMART.Application.Common.Interface;
using ESMART.Domain.Entities.FrontDesk;
using ESMART.Domain.ViewModels.FrontDesk;
using ESMART.Infrastructure.Repositories.Configuration;
using ESMART.Infrastructure.Repositories.FrontDesk;
using ESMART.Infrastructure.Repositories.RoomSetting;
using ESMART.Infrastructure.Repositories.Transaction;
using ESMART.Infrastructure.Repositories.Verification;
using ESMART.Presentation.Forms.Export;
using ESMART.Presentation.Forms.FrontDesk.Booking;
using ESMART.Presentation.Utils;
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

namespace ESMART.Presentation.Forms.FrontDesk.Reservation
{
    /// <summary>
    /// Interaction logic for ReservationPage.xaml
    /// </summary>
    public partial class ReservationPage : Page
    {
        private readonly IReservationRepository _reservationRepository;
        private readonly IHotelSettingsService _hotelSettingsService;
        private readonly IGuestRepository _guestRepository;
        private readonly IRoomRepository _roomRepository;
        private readonly IBookingRepository _bookingRepository;
        private readonly IVerificationCodeService _verificationCodeService;
        private readonly ITransactionRepository _transactionRepository;

        public ReservationPage(IReservationRepository reservationRepository, IHotelSettingsService hotelSettingsService, IGuestRepository guestRepository, IRoomRepository roomRepository, IBookingRepository bookingRepository, IVerificationCodeService verificationCodeService, ITransactionRepository transactionRepository)
        {
            _reservationRepository = reservationRepository;
            _hotelSettingsService = hotelSettingsService;
            _guestRepository = guestRepository;
            _roomRepository = roomRepository;
            _bookingRepository = bookingRepository;
            _verificationCodeService = verificationCodeService;
            _transactionRepository = transactionRepository;
            InitializeComponent();
        }

        private async Task LoadReservation()
        {
            LoaderOverlay.Visibility = Visibility.Visible;
            try
            {
                var reservations = await _reservationRepository.GetAllReservationsAsync();
                if (reservations != null)
                {
                    ReservationDataGrid.ItemsSource = reservations;
                    txtReservationCount.Text = reservations.Count().ToString();
                }
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
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
                var columnNames = ReservationDataGrid.Columns
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
                            ExportHelper.ExportAndPrint(ReservationDataGrid, exportResult.SelectedColumns, exportResult.ExportFormat, exportResult.FileName, hotel.LogoUrl!, hotel.Name, hotel.Email, hotel.PhoneNumber, hotel.Address);
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

        // Add reservation
        private async void AddReservation_Click(object sender, RoutedEventArgs e)
        {
            var services = new ServiceCollection();
            DependencyInjection.ConfigureServices(services);
            var serviceProvider = services.BuildServiceProvider();

            AddReservationDialog reservationForm = serviceProvider.GetRequiredService<AddReservationDialog>();
            if (reservationForm.ShowDialog() == true)
            {
                await LoadReservation();
            }
        }

        //Delete Reservation
        private async void DeleteReservation_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (sender is Button button && button.Tag is string Id)
                {
                    var selectedReservation = (ReservationViewModel)ReservationDataGrid.SelectedItem;

                    if (selectedReservation.Id != null)
                    {
                        MessageBoxResult messageResult = MessageBox.Show("Are you sure you want to delete this reservation?", "Confirmation", MessageBoxButton.YesNo, MessageBoxImage.Warning);
                        if (messageResult == MessageBoxResult.Yes)
                        {
                            LoaderOverlay.Visibility = Visibility.Visible;
                            await _reservationRepository.DeleteReservationAsync(selectedReservation.Id);
                            await LoadReservation();
                        }
                    }
                    else
                    {
                        MessageBox.Show("Please select a reservation before deleting.", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                    }
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

        // Extend Reservation
        private async void ExtendReservation_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (sender is Button button && button.Tag is string Id)
                {
                    var selectedReservation = (ReservationViewModel)ReservationDataGrid.SelectedItem;

                    if (selectedReservation.Id != null)
                    {
                        var reservation = await _reservationRepository.GetReservationByIdAsync(selectedReservation.Id);
                        ExtendReservationStayDialog extendStayDialog = new ExtendReservationStayDialog(_guestRepository, _roomRepository, _hotelSettingsService, _reservationRepository, _verificationCodeService, _transactionRepository, reservation, _bookingRepository);
                        if (extendStayDialog.ShowDialog() == true)
                        {
                            await LoadReservation();
                        }
                    }
                    else
                    {
                        MessageBox.Show("Please select a reservation before editing.", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                    }
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

        //Transfer Reservation
        private async void TransferReservation_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (sender is Button button && button.Tag is string Id)
                {
                    var selectedReservation = (ReservationViewModel)ReservationDataGrid.SelectedItem;
                    if (selectedReservation.Id != null)
                    {
                        var reservation = await _reservationRepository.GetReservationByIdAsync(selectedReservation.Id);
                        TransferGuestReservation transferDialog = new TransferGuestReservation(_guestRepository, _roomRepository, _hotelSettingsService, _reservationRepository, _verificationCodeService, _transactionRepository, reservation, _bookingRepository);
                        if (transferDialog.ShowDialog() == true)
                        {
                            await LoadReservation();
                        }
                    }
                    else
                    {
                        MessageBox.Show("Please select a reservation before editing.", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                    }
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

        private async void Page_Loaded(object sender, RoutedEventArgs e)
        {
            await LoadReservation();
        }
    }
}
