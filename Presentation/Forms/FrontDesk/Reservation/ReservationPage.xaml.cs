using ESMART.Application.Common.Interface;
using ESMART.Domain.Entities.FrontDesk;
using ESMART.Domain.Entities.RoomSettings;
using ESMART.Domain.Enum;
using ESMART.Domain.ViewModels.FrontDesk;
using ESMART.Presentation.Forms.Export;
using ESMART.Presentation.Session;
using ESMART.Presentation.Utils;
using Google.Apis.Drive.v3.Data;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.IO;
using System.Windows;
using System.Windows.Controls;

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
        private readonly IApplicationUserRoleRepository _applicationUserRoleRepository;
        private IServiceProvider _serviceProvider;

        public ReservationPage(IReservationRepository reservationRepository, IHotelSettingsService hotelSettingsService, IGuestRepository guestRepository, IRoomRepository roomRepository, IBookingRepository bookingRepository, IVerificationCodeService verificationCodeService, ITransactionRepository transactionRepository, IApplicationUserRoleRepository applicationUserRoleRepository)
        {
            _reservationRepository = reservationRepository;
            _hotelSettingsService = hotelSettingsService;
            _guestRepository = guestRepository;
            _roomRepository = roomRepository;
            _bookingRepository = bookingRepository;
            _verificationCodeService = verificationCodeService;
            _transactionRepository = transactionRepository;
            _applicationUserRoleRepository = applicationUserRoleRepository;
            InitializeComponent();
        }

        private void InitializeServices()
        {
            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .Build();

            var services = new ServiceCollection();
            DependencyInjection.ConfigureServices(services, configuration);
            _serviceProvider = services.BuildServiceProvider();
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
            catch (Exception ex)
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
            InitializeServices();

            AddReservationDialog reservationForm = _serviceProvider.GetRequiredService<AddReservationDialog>();
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
                        ExtendReservationStayDialog extendStayDialog = new ExtendReservationStayDialog(_guestRepository, _roomRepository, _hotelSettingsService, _reservationRepository, _verificationCodeService, _transactionRepository, reservation, _bookingRepository, _applicationUserRoleRepository);
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
                        TransferGuestReservation transferDialog = new TransferGuestReservation(_guestRepository, _roomRepository, _hotelSettingsService, _reservationRepository, _verificationCodeService, _transactionRepository, reservation, _bookingRepository, _applicationUserRoleRepository);
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

        // Book Reservation
        private async void BookReservation_Click(object sender, RoutedEventArgs e)
        {
            LoaderOverlay.Visibility = Visibility.Visible;
            try
            {
                if (sender is Button button && button.Tag is string Id)
                {
                    var selectedReservation = (ReservationViewModel)ReservationDataGrid.SelectedItem;
                    if (selectedReservation.Id != null)
                    {
                        var reservation = await _reservationRepository.GetReservationByIdAsync(selectedReservation.Id);
                        var reservedAccount = await _transactionRepository.GetBankAccountById(reservation.AccountNumber);
                        var transaction = await _transactionRepository.GetByInvoiceNumberAsync(reservation.ReservationId);

                        var booking = new Domain.Entities.FrontDesk.Booking()
                        {
                            BookingId = $"BK{Guid.NewGuid().ToString().Split('-')[0].ToUpper().AsSpan(0, 5)}",
                            CheckIn = reservation.ArrivateDate,
                            CheckOut = reservation.DepartureDate,
                            Amount = reservation.Amount,
                            AccountNumber = reservation.AccountNumber,
                            Discount = reservation.Discount,
                            VAT = reservation.VAT,
                            ServiceCharge = reservation.ServiceCharge,
                            TotalAmount = reservation.TotalAmount,
                            PaymentMethod = reservation.PaymentMethod,
                            GuestId = reservation.GuestId,
                            RoomId = reservation.RoomId,
                            Status = BookingStatus.Completed,
                            ApplicationUserId = reservation.ApplicationUserId,
                            UpdatedBy = AuthSession.CurrentUser?.Id,
                            DateCreated = DateTime.Now,
                            DateModified = DateTime.Now,
                        };

                        await _bookingRepository.AddBooking(booking);

                        var transactionItem = new Domain.Entities.Transaction.TransactionItem
                        {
                            Amount = booking.Amount,
                            TaxAmount = booking.VAT,
                            ServiceId = booking.BookingId,
                            ServiceCharge = booking.ServiceCharge,
                            Discount = booking.Discount,
                            Category = Category.Accomodation,
                            Type = TransactionType.Adjustment,
                            Status = TransactionStatus.Paid,
                            BankAccount = $"{reservedAccount.BankAccountNumber} ({reservedAccount.BankName}) | {reservedAccount.BankAccountName}",
                            DateAdded = DateTime.Now,
                            ApplicationUserId = AuthSession.CurrentUser?.Id,
                            TransactionId = transaction.Id,
                            Description = $"Reservation converted to booking for {reservation.Guest.FullName} to {reservation.Room.Number} from {reservation.ArrivateDate} to {reservation.DepartureDate}"
                        };

                        await _transactionRepository.AddTransactionItemAsync(transactionItem);

                        transaction.BookingId = booking.Id;
                        await _transactionRepository.UpdateTransactionAsync(transaction);
                        
                        reservation.Status = ReservationStatus.CheckedIn;
                        await _reservationRepository.UpdateReservationAsync(reservation);

                        var room = await _roomRepository.GetRoomById(reservation.Room.Id);
                        room.Status = RoomStatus.Booked;
                        await _roomRepository.UpdateRoom(room);

                        MessageBox.Show("Successfully booked reservation", "Success", MessageBoxButton.OK, MessageBoxImage.Information);


                        await LoadReservation();
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

        // Cancel Reservation
        private async void CancelReservation_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (sender is Button button && button.Tag is string Id)
                {
                    var selectedReservation = (ReservationViewModel)ReservationDataGrid.SelectedItem;
                    if (selectedReservation.Id != null)
                    {
                        MessageBoxResult messageResult = MessageBox.Show("Are you sure you want to cancel this reservation?", "Confirmation", MessageBoxButton.YesNo, MessageBoxImage.Warning);
                        if (messageResult == MessageBoxResult.Yes)
                        {
                            LoaderOverlay.Visibility = Visibility.Visible;

                            var reservation = await _reservationRepository.GetReservationByIdAsync(selectedReservation.Id);
                            var refundSetting = await _hotelSettingsService.GetSettingAsync("RefundPercent");

                            if (reservation != null && refundSetting != null)
                            {
                                CancelReservationDialog cancelReservationDialog = new CancelReservationDialog(reservation, reservation.TotalAmount, decimal.Parse(refundSetting.Value), _reservationRepository, _transactionRepository, _hotelSettingsService, _verificationCodeService, _applicationUserRoleRepository, _bookingRepository, _guestRepository);
                                if (cancelReservationDialog.ShowDialog() == true)
                                {
                                    var transaction = await _transactionRepository.GetByInvoiceNumberAsync(reservation.ReservationId);
                                    if (transaction != null)
                                    {
                                        var transactionItem = new Domain.Entities.Transaction.TransactionItem
                                        {
                                            Amount = reservation.TotalAmount,
                                            ServiceId = reservation.ReservationId,
                                            Status = TransactionStatus.Refunded,
                                            TaxAmount = reservation.VAT,
                                            ServiceCharge = reservation.ServiceCharge,
                                            Discount = reservation.Discount,
                                            Category = Category.Accomodation,
                                            Type = TransactionType.Refund,
                                            BankAccount = reservation.AccountNumber,
                                            DateAdded = DateTime.Now,
                                            ApplicationUserId = AuthSession.CurrentUser?.Id,
                                            TransactionId = transaction.Id,
                                            Description = $"Reservation cancelled for {reservation.Guest.FullName} to {reservation.Room.Number} from {reservation.ArrivateDate} to {reservation.DepartureDate}"
                                        };
                                        await _transactionRepository.AddTransactionItemAsync(transactionItem);
                                    }

                                    var room = await _roomRepository.GetRoomById(reservation.RoomId);
                                    if (room != null)
                                    {
                                        room.Status = Domain.Entities.RoomSettings.RoomStatus.Vacant;
                                        await _roomRepository.UpdateRoom(room);
                                    }

                                    reservation.Status = ReservationStatus.Cancelled;
                                    await _reservationRepository.UpdateReservationAsync(reservation);
                                    await LoadReservation();

                                }
                            }
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

        private async void Page_Loaded(object sender, RoutedEventArgs e)
        {
            await LoadReservation();
        }
    }
}
