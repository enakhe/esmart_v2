using ESMART.Application.Common.Interface;
using ESMART.Domain.Entities.Transaction;
using ESMART.Domain.Entities.Verification;
using ESMART.Domain.ViewModels.Transaction;
using ESMART.Presentation.Forms.Export;
using ESMART.Presentation.Forms.FrontDesk.Guest;
using ESMART.Presentation.Forms.Verification;
using ESMART.Presentation.Session;
using ESMART.Presentation.Utils;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace ESMART.Presentation.Forms.FrontDesk.Booking
{
    /// <summary>
    /// Interaction logic for CheckOutBooking.xaml
    /// </summary>
    public partial class CheckOutBooking : Window
    {
        private readonly ITransactionRepository _transactionRepository;
        private readonly IHotelSettingsService _hotelSettingsService;
        private readonly IVerificationCodeService _verificationCodeService;
        private readonly IApplicationUserRoleRepository _applicationUserRoleRepository;
        private readonly IBookingRepository _bookingRepository;
        private readonly IGuestRepository _guestRepository;
        private readonly IRoomRepository _roomRepository;
        private readonly Domain.Entities.FrontDesk.Booking _booking;
        private readonly Domain.Entities.FrontDesk.Guest _guest;
        private readonly decimal _amount;

        public CheckOutBooking(Domain.Entities.FrontDesk.Booking booking, Domain.Entities.FrontDesk.Guest guest, decimal amount, IReservationRepository reservationRepository, ITransactionRepository transactionRepository, IHotelSettingsService hotelSettingsService, IVerificationCodeService verificationCodeService, IApplicationUserRoleRepository applicationUserRoleRepository, IBookingRepository bookingRepository, IGuestRepository guestRepository, IRoomRepository roomRepository)
        {
            _amount = amount;
            _booking = booking;
            _guest = guest;
            _bookingRepository = bookingRepository;
            _guestRepository = guestRepository;
            _transactionRepository = transactionRepository;
            _transactionRepository = transactionRepository;
            _roomRepository = roomRepository;
            _hotelSettingsService = hotelSettingsService;
            _verificationCodeService = verificationCodeService;
            _applicationUserRoleRepository = applicationUserRoleRepository;
            InitializeComponent();

            txtExpectedAmount.Text = _amount.ToString("N2");
        }

        private async Task LoadTransactions()
        {
            LoaderOverlay.Visibility = Visibility.Visible;
            try
            {
                var transactions = await _transactionRepository.GetGroupedTransactionsByGuestIdAsync(_booking.Id);
                AccountTransactionStatement.ItemsSource = transactions;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error Loading Transactions", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                LoaderOverlay.Visibility = Visibility.Collapsed;
            }
        }


        private async void VerifyButton_Click(Object sender, RoutedEventArgs e)
        {
            LoaderOverlay.Visibility = Visibility.Visible;
            try
            {
                var unpaidTransaction = await _transactionRepository.GetGroupedTransactionsByGuestIdAsync(_booking.Id);
                var unpaidTransactionitem = unpaidTransaction
                    .SelectMany(t => t.GroupedTransactionItems)
                    .Where(ti => ti.Value.Any(ti => ti.Status == Domain.Enum.TransactionStatus.Unpaid))
                    .ToList();

                foreach (var transactionViewModel in unpaidTransactionitem)
                {
                    var transaction = await _transactionRepository.GetTransactionItemsByIdAsync("");

                    transaction.Status = Domain.Enum.TransactionStatus.Paid;
                    transaction.ApplicationUserId = AuthSession.CurrentUser?.Id;
                    
                    await _transactionRepository.UpdateTransactionItemAsync(transaction);
                }

                _booking.Status = Domain.Enum.BookingStatus.Active;
                _booking.IsTrashed = true;
                _booking.UpdatedBy = AuthSession.CurrentUser?.Id;

                await _bookingRepository.UpdateBooking(_booking);

                _booking.Room.Status = Domain.Entities.RoomSettings.RoomStatus.Dirty;
                await _roomRepository.UpdateRoom(_booking.Room);

                var guest = await _guestRepository.GetGuestByIdAsync(_booking.GuestId);
                var guestAccount = await _guestRepository.GetGuestAccountByGuestIdAsync(_booking.GuestId);

                if (guest != null)
                {
                    guest.Status = "Inactive";
                    await _guestRepository.UpdateGuestAsync(guest);
                }

                if (guestAccount != null)
                {
                    guestAccount.IsClosed = true;
                    await _guestRepository.UpdateGuestAccountAsync(guestAccount);
                }

                this.DialogResult = true;
                MessageBox.Show("Booking checked out successfully.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
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

        private async void ViewGuest_Click(Object sender, RoutedEventArgs e)
        {
            LoaderOverlay.Visibility = Visibility.Visible;
            try
            {
                var selectedColumns = AccountTransactionStatement.Columns
                    .OfType<DataGridTextColumn>()
                    .Select(c => (c.Binding as Binding)?.Path?.Path ?? c.Header.ToString())
                    .ToList();

                // For nested columns from TransactionItem
                var nestedColumns = new List<string>
                {
                    "Date",
                    "Description",
                    "Invoice",
                    "Discount",
                    "BillPost",
                    "Amount",
                    "Payment"
                };

                // Flatten the grouped transaction items
                foreach (var tx in AccountTransactionStatement.ItemsSource.Cast<TransactionViewModel>())
                {
                    // Combine all grouped items into a single list for export
                    tx.FlatTransactionItems = tx.GroupedTransactionItems?
                        .SelectMany(kvp => kvp.Value)
                        .Where(kvp => kvp.Category.ToString() == "Accomodation")
                        .ToList();
                }

                // get all transaction that are not under the category of accomodation
                var nonAccommodationTransactions = AccountTransactionStatement.ItemsSource.Cast<TransactionViewModel>()
                    .SelectMany(tx => tx.GroupedTransactionItems)
                    .SelectMany(kvp => kvp.Value)
                    .Where(item => item.Category.ToString() != "Accomodation" && item.Category.ToString() != "Deposit")
                    .ToList();

                // create a new datagrid with nonAccommodationTransactions
                var nonAccommodationDataGrid = new DataGrid
                {
                    AutoGenerateColumns = false,
                    ItemsSource = nonAccommodationTransactions
                };

                var depositTransaction = AccountTransactionStatement.ItemsSource.Cast<TransactionViewModel>()
                    .SelectMany(tx => tx.GroupedTransactionItems)
                    .SelectMany(kvp => kvp.Value)
                    .Where(item => item.Category.ToString() == "Deposit")
                    .ToList();

                var depositTransactionDataGrid = new DataGrid
                {
                    AutoGenerateColumns = false,
                    ItemsSource = depositTransaction
                };

                var transactions = await _transactionRepository.GetGroupedTransactionsByGuestIdAsync(_booking.Id);

                var totalVAT = transactions.Sum(t => t.GroupedTransactionItems
                    .SelectMany(g => g.Value)
                    .Sum(v => v.Tax));

                //calculate total service
                var totalServiceCharge = transactions.Sum(t => t.GroupedTransactionItems
                    .SelectMany(g => g.Value)
                    .Sum(v => v.Service)) + totalVAT;

                //calculate total discount
                var totalDiscount = transactions.Sum(t => t.GroupedTransactionItems
                    .SelectMany(g => g.Value)
                    .Sum(v => v.Discount));

                var totalAmount = transactions.Sum(t => t.Booking.Amount);
                var amountPaid = transactions.Sum(t => t.Paid);

                // sum the total amount for nonAccomodation
                var totalNonAccommodationAmount = nonAccommodationTransactions.Sum(item => item.BillPost);

                //calculate total amount totalamount + totalServiceCharge - totalDiscount
                var totalTAmount = totalAmount + totalServiceCharge - totalDiscount + totalNonAccommodationAmount;

                var optionsWindow = new ExportBillDialog(selectedColumns!, AccountTransactionStatement, _hotelSettingsService, _booking, "FlatTransactionItems", nestedColumns, "Booking Bill List (Invoices Settled)", totalAmount, totalVAT:totalVAT, totalDiscount:totalDiscount, totalTAmount:totalTAmount, totalService:totalServiceCharge, serviceTable:nonAccommodationDataGrid, totalServiceCharge: totalNonAccommodationAmount, amountPaid:amountPaid, paymentTable: depositTransactionDataGrid);
                var result = optionsWindow.ShowDialog();

                if (result == true)
                {
                    var exportResult = optionsWindow.GetResult();
                    var hotel = await _hotelSettingsService.GetHotelInformation();

                    if (exportResult.SelectedColumns?.Count == 0)
                    {
                        MessageBox.Show("Please select at least one column to export.", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
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

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
        }

        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            await LoadTransactions();
        }
    }
}
