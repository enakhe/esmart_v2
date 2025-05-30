#nullable disable

using ESMART.Application.Common.Dtos;
using ESMART.Application.Common.Interface;
using ESMART.Application.Common.Utils;
using ESMART.Domain.Entities.FrontDesk;
using ESMART.Domain.Entities.Transaction;
using ESMART.Domain.Enum;
using ESMART.Domain.ViewModels.FrontDesk;
using ESMART.Domain.ViewModels.Transaction;
using ESMART.Infrastructure.Services;
using ESMART.Presentation.Forms.Export;
using ESMART.Presentation.Forms.Receipt;
using ESMART.Presentation.Utils;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interop;

namespace ESMART.Presentation.Forms.FrontDesk.Guest
{
    /// <summary>
    /// Interaction logic for GuestDetailsDialog.xaml
    /// </summary>
    public partial class GuestDetailsDialog : Window
    {
        private readonly string _id;
        private readonly IGuestRepository _guestRepository;
        private readonly ITransactionRepository _transactionRepository;
        private readonly IHotelSettingsService _hotelSettingsService;
        private readonly IBookingRepository _bookingRepository;
        private readonly GuestAccountService _guestAccountService;

        public GuestDetailsDialog(string id, IGuestRepository guestRepository, ITransactionRepository transactionRepository, IHotelSettingsService hotelSettingsService, IBookingRepository bookingRepository, GuestAccountService guestAccountService)
        {
            _id = id;
            _guestRepository = guestRepository;
            _transactionRepository = transactionRepository;
            _hotelSettingsService = hotelSettingsService;
            _bookingRepository = bookingRepository;
            _guestAccountService = guestAccountService;
            InitializeComponent();

            Loaded += DisableMinimizeButton;
        }

        private async Task LoadGuestDetails()
        {
            LoaderOverlay.Visibility = Visibility.Visible;
            try
            {
                var guest = await _guestRepository.GetGuestByIdAsync(_id);
                if (guest != null)
                {
                    var guestAccount = await _guestAccountService.GetAccountAsync(_id);

                    var guestViewModel = new GuestViewModel()
                    {
                        Id = guest.Id,
                        GuestId = guest.GuestId,
                        FullName = $"{guest.FirstName} {guest.MiddleName} {guest.LastName}",
                        GuestImage = guest.GuestImage,
                        Email = guest.Email,
                        Gender = guest.Gender,
                        Street = guest.Street,
                        Status = guest.Status,
                        CurrentBalance = guestAccount.Balance,
                        PhoneNumber = guest.PhoneNumber,
                        City = guest.City,
                        State = guest.State,
                        Country = guest.Country,
                        CreatedBy = guest.ApplicationUser?.FullName,
                        DateCreated = guest.DateCreated,
                        DateModified = guest.DateModified,
                    };

                    this.DataContext = guestViewModel;
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

        private async Task LoadGuestTransactionHistory()
        {
            LoaderOverlay.Visibility = Visibility.Visible;
            try
            {
                var guestTransactionItem = await _guestAccountService.GetGuestAccountSummaryAsync(_id);
                if (guestTransactionItem != null)
                {
                    TransactionItemDataGrid.ItemsSource = new List<GuestAccountSummaryDto> { guestTransactionItem };

                    var (BookingAmount, Discount, ServiceCharge, VAT, TotalAmount, TotalPaid, AmountToReceive, AmountToRefund) = Helper.CalculateSummary(guestTransactionItem);

                    txtSummaryName.Text = $"Total for the period {guestTransactionItem.CheckIn:MM/dd/yy} to {guestTransactionItem.CheckOut:MM/dd/yy}";
                    txtBookingAmount.Text = $"₦ {(BookingAmount + guestTransactionItem.OtherCharges):N2}";
                    txtDiscount.Text = $"₦ {Discount:N2}";
                    txtServiceCharge.Text = $"₦ {ServiceCharge:N2}";
                    txtVAT.Text = $"₦ {VAT:N2}";
                    txtTotalAmount.Text = $"₦ {TotalAmount:N2}";
                    txtAmountPaid.Text = $"₦ {TotalPaid:N2}";
                    txtReceive.Text = $"₦ {AmountToReceive:N2}";
                    txtRefund.Text = $"₦ {AmountToRefund:N2}";

                    if (AmountToReceive > 0)
                    {
                        ReceiveGrid.Visibility = Visibility.Visible;
                    }

                    if (AmountToRefund > 0)
                    {
                        RefundGrid.Visibility = Visibility.Visible;
                    }

                    if (AmountToReceive == 0 && AmountToRefund == 0)
                    {
                        txtAccountBalanced.Visibility = Visibility.Visible;
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

        private async void DeleteGuest_Click(object sender, RoutedEventArgs e)
        {
            LoaderOverlay.Visibility = Visibility.Visible;
            try
            {
                MessageBoxResult result = MessageBox.Show("Are you sure you want to delete this guest?", "Confirmation", MessageBoxButton.YesNo, MessageBoxImage.Warning);
                if (result == MessageBoxResult.Yes)
                {
                    await _guestRepository.DeleteGuestAsync(_id);
                    this.DialogResult = true;
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

        private async void Window_Activated(object sender, EventArgs e)
        {
            await LoadGuestDetails();
        }

        private async void ExportButton_Click(object sender, RoutedEventArgs e)
        {
            LoaderOverlay.Visibility = Visibility.Visible;
            try
            {
                var columnNames = TransactionItemDataGrid.Columns
                    .Where(c => c.Header != null)
                    .Select(c => c.Header.ToString())
                    .Where(name => !string.IsNullOrWhiteSpace(name) && name != "Operation")
                .ToList();

                var guest = await _guestRepository.GetGuestByIdAsync(_id);

                var optionsWindow = new ExportDialog(columnNames, TransactionItemDataGrid, _hotelSettingsService, $"{guest.FullName} Transaction Detals");
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
                            ExportHelper.ExportAndPrint(TransactionItemDataGrid, exportResult.SelectedColumns, exportResult.ExportFormat, exportResult.FileName, hotel.LogoUrl!, hotel.Name, hotel.Email, hotel.PhoneNumber, hotel.Address);
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

        private async void TabControl_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (e.Source is TabControl tabControl)
            {
                var selectedTab = tabControl.SelectedItem as TabItem;

                if (selectedTab == tbTransactionHistory)
                {
                    await LoadGuestTransactionHistory();
                }
            }
        }

        private async void GuestFolioButton_Click(object sender, RoutedEventArgs e)
        {
            LoaderOverlay.Visibility = Visibility.Visible;
            try
            {
                var guest = await _guestRepository.GetGuestByIdAsync(_id);

                if(guest != null)
                {
                    GuestFolioDialog guestFolioDialog = new GuestFolioDialog(guest, _transactionRepository, _hotelSettingsService, _bookingRepository);
                    guestFolioDialog.ShowDialog();
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

        private async void PrintReceiptButton_Click(object sender, RoutedEventArgs e)
        {
            LoaderOverlay.Visibility = Visibility.Visible;
            try
            {
                if (sender is Button button && button.Tag is string Id)
                {
                    var selectedTransaction = (TransactionItemViewModel)TransactionItemDataGrid.SelectedItem;
                    if (selectedTransaction != null)
                    {
                        var transactionItem = await _transactionRepository.GetTransactionItemsByIdAsync(selectedTransaction.Id);
                        var booking = await _bookingRepository.GetBookingById(transactionItem.ServiceId);
                        List<TransactionItemViewModel> transactionItems = new List<TransactionItemViewModel>();

                        if (transactionItem != null)
                        {
                            var transactionItemViewModel = new TransactionItemViewModel()
                            {
                                Id = transactionItem.Id,
                                ServiceId = transactionItem.ServiceId,
                                Amount = transactionItem.Amount.ToString("N2"),
                                Tax = transactionItem.TaxAmount,
                                Service = transactionItem.ServiceCharge,
                                Discount = transactionItem.Discount,
                                BillPost = transactionItem.TotalAmount,
                                Description = transactionItem.Description,
                                Category = transactionItem.Category.ToString(),
                                Type = transactionItem.Type.ToString(),
                                Status = transactionItem.Status,
                                Account = transactionItem.BankAccount,
                                Date = transactionItem.DateAdded,
                                IssuedBy = transactionItem.ApplicationUser.FullName,
                            };

                            transactionItems.Add(transactionItemViewModel);
                        }

                        var hotel = await _hotelSettingsService.GetHotelInformation();
                        if (hotel != null)
                        {
                            if (transactionItem != null)
                            {
                                ReceiptViewerDialog receiptViewerDialog = new ReceiptViewerDialog(transactionItems, _hotelSettingsService, booking, transactionItem.TotalAmount);
                                if (receiptViewerDialog.ShowDialog() == true)
                                {
                                    this.DialogResult = true;
                                }
                            }
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

        private async void MarkTransactionAsPaidButton_Click(object sender, RoutedEventArgs e)
        {
            LoaderOverlay.Visibility = Visibility.Visible;
            try
            {
                if (sender is Button button && button.Tag is string Id)
                {
                    var selectedTransaction = (TransactionItemViewModel)TransactionItemDataGrid.SelectedItem;
                    if (selectedTransaction != null)
                    {
                        var transactionItem = await _transactionRepository.GetTransactionItemsByIdAsync(selectedTransaction.Id);
                        if (transactionItem != null)
                        {
                            await _transactionRepository.MarkTransactionItemAsPaidAsync(transactionItem.Id);
                            MessageBox.Show("Transaction marked as paid successfully", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                            await LoadGuestDetails();
                            await LoadGuestTransactionHistory();
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

        private void DisableMinimizeButton(object sender, RoutedEventArgs e)
        {
            var hwnd = new WindowInteropHelper(this).Handle;
            int currentStyle = GetWindowLong(hwnd, GWL_STYLE);
            SetWindowLong(hwnd, GWL_STYLE, currentStyle & ~WS_MINIMIZEBOX);
        }

        private const int GWL_STYLE = -16;
        private const int WS_MINIMIZEBOX = 0x00020000;

        [DllImport("user32.dll")]
        private static extern int GetWindowLong(IntPtr hWnd, int nIndex);

        [DllImport("user32.dll")]
        private static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);

        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            var guest = await _guestRepository.GetGuestByIdAsync(_id);
            FundGuestAccountDialog fundGuestAccountDialog = new FundGuestAccountDialog(guest, _guestRepository, _bookingRepository, _transactionRepository, _guestAccountService);
            if (fundGuestAccountDialog.ShowDialog() == true)
            {
                await LoadGuestDetails();
                await LoadGuestTransactionHistory();
            }
        }

        private async void EditButton_Click(object sender, RoutedEventArgs e)
        {
            var guest = await _guestRepository.GetGuestByIdAsync(_id);

            UpdateGuestDialog updateGuestDialog = new UpdateGuestDialog(guest.Id, _guestRepository);
            if (updateGuestDialog.ShowDialog() == true)
            {
                await LoadGuestDetails();
            }
        }

        private async void PrintGuest_Click(object sender, RoutedEventArgs e)
        {
            LoaderOverlay.Visibility = Visibility.Visible;
            try
            {
                var guest = await _guestRepository.GetGuestByIdAsync(_id);
                var hotel = await _hotelSettingsService.GetHotelInformation();
                var printer = new PrintHelper();

                var doc = printer.PrintGuestInformation(guest, hotel);

                string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmm");
                PrintHelper.PrintFlowDocument(doc, System.Printing.PageOrientation.Portrait);
                PrintHelper.SaveFlowDocumentToFile(doc, $"{guest.FullName.Replace(" ", "-")}-{timestamp}");

                this.Activate();
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

        private async void PrintButton_Click(object sender, RoutedEventArgs e)
        {
            LoaderOverlay.Visibility = Visibility.Visible;
            try
            {
                var guestTransactionItem = await _guestAccountService.GetGuestAccountSummaryAsync(_id);
                var guestAccount = await _guestAccountService.GetAccountAsync(_id);
                var booking = await _guestAccountService.GetBookingByGuestAccountIdAsync(guestAccount.Id);
                var printer = new PrintHelper();

                var hotel = await _hotelSettingsService.GetHotelInformation();

                var doc = printer.GenerateGuestAccountFlowDocument(guestTransactionItem, booking, hotel);
                PrintHelper.PrintFlowDocument(doc, System.Printing.PageOrientation.Portrait);
                this.Activate();
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
    }
}
