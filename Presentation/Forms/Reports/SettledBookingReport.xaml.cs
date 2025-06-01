using ESMART.Application.Common.Dtos;
using ESMART.Application.Common.Utils;
using ESMART.Infrastructure.Services;
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
    /// Interaction logic for SettledBookingReport.xaml
    /// </summary>
    public partial class SettledBookingReport : Page
    {
        public GuestAccountService _guestAccountService;
        public SettledBookingReport(GuestAccountService guestAccountService)
        {
            _guestAccountService = guestAccountService;
            InitializeComponent();
        }

        private async void SearchButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var invoice = txtSearch.Text;
                var settlement = cmbSettlement.Text;

                // Validate input
                if (Helper.AreAnyNullOrEmpty(invoice, settlement))
                    return;

                // Determine settlement status
                bool isSettled = settlement.Equals("Settled", StringComparison.OrdinalIgnoreCase);

                // Retrieve guest transaction summary
                var guestTransactionItem = await _guestAccountService.GetBookingAccountSummaryByInvoiceAsync(invoice, isSettled);
                if (guestTransactionItem == null)
                    return;

                var guestAccount = await _guestAccountService.GetAccountByInvoiceAsync(invoice);
                var booking = await _guestAccountService.GetBookingByGuestAccountIdAsync(guestAccount.Id);

                // Update UI with retrieved transaction details
                TransactionItemDataGrid.ItemsSource = new List<GuestAccountSummaryDto> { guestTransactionItem };

                var (BookingAmount, Discount, ServiceCharge, VAT, TotalAmount, TotalPaid, AmountToReceive, AmountToRefund) = Helper.CalculateSummary(guestTransactionItem);

                UpdateTransactionSummaryUI(booking, guestTransactionItem, BookingAmount, Discount, ServiceCharge, VAT, TotalAmount, TotalPaid, AmountToReceive, AmountToRefund);

                if (guestTransactionItem.BookingGroups.Count == 0)
                {
                    SummaryPanel.Visibility = Visibility.Collapsed;
                }
                else
                {
                    SummaryPanel.Visibility = Visibility.Visible;
                }
            }
            catch (Exception ex)
            {
                LogError(ex);
            }
            finally
            {
                LoaderOverlay.Visibility = Visibility.Collapsed;
            }
        }


        private void UpdateTransactionSummaryUI(Domain.Entities.FrontDesk.Booking booking, GuestAccountSummaryDto item, decimal bookingAmount, decimal discount, decimal serviceCharge, decimal vat, decimal totalAmount, decimal totalPaid, decimal amountToReceive, decimal amountToRefund)
        {
            if (booking != null)
            {
                txtSummaryName.Text = $"Total for the period {booking.CheckIn:MM/dd/yy} to {booking.CheckOut:MM/dd/yy}";
            }
            txtBookingAmount.Text = $"₦ {(bookingAmount + item.OtherCharges):N2}";
            txtDiscount.Text = $"₦ {discount:N2}";
            txtServiceCharge.Text = $"₦ {serviceCharge:N2}";
            txtVAT.Text = $"₦ {vat:N2}";
            txtTotalAmount.Text = $"₦ {totalAmount:N2}";
            txtAmountPaid.Text = $"₦ {totalPaid:N2}";
            txtReceive.Text = $"₦ {amountToReceive:N2}";
            txtRefund.Text = $"₦ {amountToRefund:N2}";

            ReceiveGrid.Visibility = amountToReceive > 0 ? Visibility.Visible : Visibility.Collapsed;
            RefundGrid.Visibility = amountToRefund > 0 ? Visibility.Visible : Visibility.Collapsed;
            txtAccountBalanced.Visibility = (amountToReceive == 0 && amountToRefund == 0) ? Visibility.Visible : Visibility.Collapsed;
        }

        private static void LogError(Exception ex)
        {
            MessageBox.Show($"Error: {ex.Message}", "Transaction Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }
}
