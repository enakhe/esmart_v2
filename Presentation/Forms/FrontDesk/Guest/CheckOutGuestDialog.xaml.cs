using CommunityToolkit.Mvvm.DependencyInjection;
using DocumentFormat.OpenXml.InkML;
using ESMART.Application.Common.Dtos;
using ESMART.Domain.Entities.FrontDesk;
using ESMART.Infrastructure.Services;
using ESMART.Presentation.Session;
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
using System.Windows.Shapes;

namespace ESMART.Presentation.Forms.FrontDesk.Guest
{
    /// <summary>
    /// Interaction logic for CheckOutGuestDialog.xaml
    /// </summary>
    public partial class CheckOutGuestDialog : Window
    {
        private readonly GuestAccount _guestAccount;
        private readonly string _bookingId;
        private readonly FlowDocument _flowDocument;
        private readonly GuestAccountService _guestAccountService;
        private readonly GuestAccountSummaryDto _guestAccountSummaryDto;

        public CheckOutGuestDialog(FlowDocument flowDocument, GuestAccountSummaryDto guestAccountSummaryDto, GuestAccountService guestAccountService, GuestAccount guest, string bookingId)
        {
            InitializeComponent();
            _flowDocument = flowDocument;
            _guestAccount = guest;
            _bookingId = bookingId;
            _guestAccountSummaryDto = guestAccountSummaryDto;
            _guestAccountService = guestAccountService;
        }

        private void LoadDocument()
        {
            LoaderOverlay.Visibility = Visibility.Collapsed;
            try
            {
                docViewer.Document = _flowDocument;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occured when getting user bill. {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
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

        private void PrintButton_Click(object sender, RoutedEventArgs e)
        {
            string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmm");
            PrintHelper.PrintFlowDocument(_flowDocument, System.Printing.PageOrientation.Portrait);
            PrintHelper.SaveFlowDocumentToFile(_flowDocument, $"{_guestAccount.Guest.FullName.Replace(" ", "-")}-{timestamp}");
            this.DialogResult = true;
        }

        private void Window_Activated(object sender, EventArgs e)
        {
            LoadDocument();
        }

        private async void CheckOutButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                decimal balance = _guestAccountSummaryDto.Balance;
                decimal refund = _guestAccountSummaryDto.Refunds;
                bool isRefund = _guestAccountSummaryDto.Refunds > 0;
                bool isOutstandingBalance = balance < 0;

                if (isOutstandingBalance)
                {
                    var result = MessageBox.Show(
                        $"This guest account has an outstanding balance of ₦{balance:N2}.\n\nDo you still want to check out the guest? The account will be flagged as unsettled.",
                        "Outstanding Balance",
                        MessageBoxButton.YesNoCancel,
                        MessageBoxImage.Warning);

                    if (result == MessageBoxResult.Yes)
                    {
                        await _guestAccountService.CheckoutGuestAsync(_bookingId, allowUnsettled: true);
                        MessageBox.Show(
                            "Guest has been checked out and account marked as unsettled.", 
                            "Checkout Complete", 
                            MessageBoxButton.OK, 
                            MessageBoxImage.Information);

                        this.DialogResult = true;
                    }

                    return;
                }

                if (isRefund)
                {
                    var booking = await _guestAccountService.GetBookingByGuestAccountIdAsync(_guestAccount.Id);

                    var result = MessageBox.Show(
                        $"This guest account is due for a refund of ₦{Math.Abs(refund):N2}.\n\nPlease confirm that the refund has been processed before proceeding.",
                        "Pending Refund",
                        MessageBoxButton.YesNo,
                        MessageBoxImage.Question);

                    if (result == MessageBoxResult.Yes)
                    {
                        var transactionDto = new GuestTransactionDto()
                        {
                            Amount = refund,
                            ApplicationUserId = AuthSession.CurrentUser.Id,
                            GuestAccountId = _guestAccount.Id,
                            BankAccountId = booking.BankAccountId,
                            Discount = 0,
                            BookingId = booking.Id,
                            GuestId = booking.GuestId,
                            Consumer = booking.Guest.FullName,
                            PaymentMethod = booking.PaymentMethod,
                            TransactionType = Domain.Enum.TransactionType.Refund,
                            Tax = 0,
                            Description = $"Refund processed at checkout for {_guestAccountSummaryDto.GuestName} by {AuthSession.CurrentUser.FullName}"
                        };

                        await _guestAccountService.AddTransaction(_guestAccount.GuestId, transactionDto);
                        //await _guestAccountService.AddRefundAsync(_guestAccount.GuestId, refund);
                        await _guestAccountService.CheckoutGuestAsync(_bookingId, allowUnsettled: false);
                        MessageBox.Show("Guest has been checked out and refund noted.", "Checkout Complete", MessageBoxButton.OK, MessageBoxImage.Information);

                        this.DialogResult = true;
                    }

                    return;
                }

                // No balance, no refund – normal checkout
                await _guestAccountService.CheckoutGuestAsync(_bookingId);
                MessageBox.Show("Guest has been successfully checked out.", "Checkout Complete", MessageBoxButton.OK, MessageBoxImage.Information);

                this.DialogResult = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred during checkout:\n{ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

    }
}
