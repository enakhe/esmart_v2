#nullable disable

using ESMART.Application.Common.Dtos;
using ESMART.Application.Common.Interface;
using ESMART.Application.Common.Utils;
using ESMART.Domain.Entities.FrontDesk;
using ESMART.Domain.Entities.Transaction;
using ESMART.Domain.Enum;
using ESMART.Domain.ViewModels.FrontDesk;
using ESMART.Domain.ViewModels.Transaction;
using ESMART.Infrastructure.Repositories.RoomSetting;
using ESMART.Infrastructure.Services;
using ESMART.Presentation.Forms.Export;
using ESMART.Presentation.Forms.Receipt;
using ESMART.Presentation.Session;
using ESMART.Presentation.Utils;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Interop;
using System.Windows.Media.Animation;

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
                        PhoneNumber = guest.PhoneNumber,
                        City = guest.City,
                        State = guest.State,
                        Country = guest.Country,
                        CreatedBy = guest.ApplicationUser?.FullName,
                        DateCreated = guest.DateCreated,
                        DateModified = guest.DateModified,
                    };

                    if (guestAccount != null)
                    {
                        guestViewModel.CurrentBalance = guestAccount!.Balance;
                    }

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

        private async Task<GuestAccountSummaryDto> LoadGuestTransactionHistoryAsync()
        {
            LoaderOverlay.Visibility = Visibility.Visible;

            try
            {
                var guestTransactionItem = await _guestAccountService.GetGuestAccountSummaryAsync(_id);
                var guestAccount = await _guestAccountService.GetAccountAsync(_id);

                if (guestAccount != null)
                {
                    var booking = await _guestAccountService.GetBookingByGuestAccountIdAsync(guestAccount.Id);

                    if (guestTransactionItem == null) return null;

                    TransactionItemDataGrid.ItemsSource = new List<GuestAccountSummaryDto> { guestTransactionItem };

                    var (BookingAmount, Discount, ServiceCharge, VAT, TotalAmount, TotalPaid, AmountToReceive, AmountToRefund) = Helper.CalculateSummary(guestTransactionItem);

                    UpdateTransactionSummaryUI(booking, guestTransactionItem, BookingAmount, Discount, ServiceCharge, VAT, TotalAmount, TotalPaid, AmountToReceive, AmountToRefund);

                    if (guestTransactionItem.BookingGroups.Count == 0)
                    {
                        CheckOutButton.Visibility = Visibility.Hidden;
                        SummaryPanel.Visibility = Visibility.Collapsed;
                    }
                }
                else
                {
                    CheckOutButton.Visibility = Visibility.Hidden;
                    SummaryPanel.Visibility = Visibility.Collapsed;
                }
          
                return guestTransactionItem;
            }
            catch (Exception ex)
            {
                LogError(ex);
            }
            finally
            {
                LoaderOverlay.Visibility = Visibility.Collapsed;
            }

            return null;
        }

        private void UpdateTransactionSummaryUI(Domain.Entities.FrontDesk.Booking booking, GuestAccountSummaryDto item, decimal bookingAmount, decimal discount, decimal serviceCharge, decimal vat, decimal totalAmount, decimal totalPaid, decimal amountToReceive, decimal amountToRefund)
        {
            if (booking != null)
            {
                txtSummaryName.Text = $"Total for the period {booking.CheckIn:dd/MM/yy} to {booking.CheckOut:dd/MM/yy}";
            }
            txtBookingAmount.Text = $"₦ {(bookingAmount + item.OtherCharges):N2}";
            txtDiscount.Text = $"₦ {discount:N2}";
            txtServiceCharge.Text = $"₦ {serviceCharge:N2}";
            txtVAT.Text = $"₦ {vat:N2}";
            txtTotalAmount.Text = $"₦ {Math.Round(totalAmount):N2}";
            txtAmountPaid.Text = $"₦ {totalPaid:N2}";
            txtReceive.Text = $"₦ {amountToReceive:N2}";
            txtRefund.Text = $"₦ {Math.Round(amountToRefund):N2}";

            ReceiveGrid.Visibility = amountToReceive > 0 ? Visibility.Visible : Visibility.Collapsed;
            RefundGrid.Visibility = amountToRefund > 0 ? Visibility.Visible : Visibility.Collapsed;
            txtAccountBalanced.Visibility = (amountToReceive == 0 && amountToRefund == 0) ? Visibility.Visible : Visibility.Collapsed;
        }

        private static void LogError(Exception ex)
        {
            MessageBox.Show($"Error: {ex.Message}", "Transaction Error", MessageBoxButton.OK, MessageBoxImage.Error);
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
                    await LoadGuestTransactionHistoryAsync();
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
            //LoaderOverlay.Visibility = Visibility.Visible;
            //try
            //{
            //    if (sender is Button button && button.Tag is string TransactionId)
            //    {
            //        var transation = await _guestAccountService.GetTansactionByTransactionIdAsync(TransactionId);

            //        if(transation != null)
            //        {
            //            var guestAccount = await _guestAccountService.GetAccountAsync(_id);
            //            var booking = await _guestAccountService.GetBookingByGuestAccountIdAsync(guestAccount.Id);
            //            var hotel = await _hotelSettingsService.GetHotelInformation();

            //            var printHelper = new PrintHelper();

            //            var doc = printHelper.GenerateReceipt(transation, booking, hotel);

            //            string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmm");
            //            PrintHelper.PrintFlowDocument(doc, System.Printing.PageOrientation.Portrait);
            //            PrintHelper.SaveFlowDocumentToFile(doc, $"{guestAccount.Guest.FullName.Replace(" ", "-")}-Receipt-{timestamp}");
            //        }
            //    }
            //}
            //catch (Exception ex)
            //{
            //    MessageBox.Show(ex.Message, ex.Source, MessageBoxButton.OK,
            //        MessageBoxImage.Error);
            //}
            //finally
            //{
            //    LoaderOverlay.Visibility = Visibility.Collapsed;
            //}
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
                            await LoadGuestTransactionHistoryAsync();
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

        private async void GuestDataGrid_CellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
        {
            try
            {
                if (e.Row.Item is GuestAccountSummaryDto editedItem)
                {
                    var guestAccount = await _guestAccountService.GetAccountAsync(_id);

                    guestAccount.TopUps = editedItem.Paid;

                    await _guestAccountService.UpdateGuestAccount(guestAccount);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error updating record: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
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
                await LoadGuestTransactionHistoryAsync();
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
                var doc = await GetBillFlowDocument();
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

        private async Task<FlowDocument> GetBillFlowDocument()
        {
            var guestTransactionItem = await LoadGuestTransactionHistoryAsync();
            var guestAccount = await _guestAccountService.GetAccountAsync(_id);
            var booking = await _guestAccountService.GetBookingByGuestAccountIdAsync(guestAccount.Id);
            var printer = new PrintHelper();

            var hotel = await _hotelSettingsService.GetHotelInformation();

            var doc = printer.GenerateGuestAccountFlowDocument(guestTransactionItem, booking, hotel);

            return doc;
        }

        private async void CheckOutButton_Click(object sender, RoutedEventArgs e)
        {
            var guestTransaction = await LoadGuestTransactionHistoryAsync();
            var guestAccount = await _guestAccountService.GetAccountAsync(_id);
            var booking = await _guestAccountService.GetBookingByGuestAccountIdAsync(guestAccount.Id);

            var doc = await GetBillFlowDocument();
            var checkoutDialog = new CheckOutGuestDialog(doc, guestTransaction, _guestAccountService, guestAccount, booking.Id)
            {
                Owner = this
            };

            if(checkoutDialog.ShowDialog() == true)
            {
                await LoadGuestDetails();
                await LoadGuestTransactionHistoryAsync();
                this.Activate();
            }
        }

        private async void SettingButton_Click(object sender, RoutedEventArgs e)
        {
            var guest = await _guestRepository.GetGuestByIdAsync(_id);

            GuestSettngsDialog guestSettngs = new GuestSettngsDialog(guest, _guestAccountService);
            if (guestSettngs.ShowDialog() == true)
            {
                await LoadGuestDetails();
            }
        }

        private async void PrintButon_Click(object sender, RoutedEventArgs e)
        {
            LoaderOverlay.Visibility = Visibility.Visible;
            try
            {
                if (sender is Button button && button.Tag is string TransactionId)
                {
                    var transation = await _guestAccountService.GetTansactionByTransactionIdAsync(TransactionId);

                    if (transation != null)
                    {
                        var guestAccount = await _guestAccountService.GetAccountAsync(_id);
                        var booking = await _guestAccountService.GetBookingByGuestAccountIdAsync(guestAccount.Id);
                        var hotel = await _hotelSettingsService.GetHotelInformation();
                        var applicationUser = AuthSession.CurrentUser.FullName;

                        var printHelper = new PrintHelper();

                        var doc = printHelper.GenerateReceipt(transation, booking, hotel, applicationUser);

                        string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmm");
                        PrintHelper.PrintFlowDocument(doc, System.Printing.PageOrientation.Portrait);
                        PrintHelper.SaveFlowDocumentToFile(doc, $"{guestAccount.Guest.FullName.Replace(" ", "-")}-Receipt-{timestamp}");

                        this.Activate();
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
    }
}
