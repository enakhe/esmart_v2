using ESMART.Application.Common.Dtos;
using ESMART.Application.Common.Interface;
using ESMART.Application.Common.Utils;
using ESMART.Domain.Entities.FrontDesk;
using ESMART.Infrastructure.Repositories.Configuration;
using ESMART.Infrastructure.Services;
using ESMART.Presentation.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace ESMART.Presentation.Forms.Reports
{
    /// <summary>
    /// Interaction logic for GuestTransactionReport.xaml
    /// </summary>
    public partial class GuestTransactionReport : Window
    {
        private readonly GuestAccountSummaryDto _guestAccountSummaryDto;
        private readonly GuestAccountService _guestAccountService;
        private readonly GuestAccount _guestAccount;
        private readonly IHotelSettingsService _hotelSettingsService;

        public GuestTransactionReport(GuestAccountSummaryDto guestAccountSummaryDto, GuestAccountService guestAccountService, GuestAccount guestAccount, IHotelSettingsService hotelSettingsService)
        {
            _guestAccountSummaryDto = guestAccountSummaryDto;
            _guestAccountService = guestAccountService;
            _guestAccount = guestAccount;
            _hotelSettingsService = hotelSettingsService;

            InitializeComponent();

            Loaded += DisableMinimizeButton;
        }

        private async Task LoadGuestTransactionHistoryAsync()
        {
            try
            {
                if (_guestAccountSummaryDto != null)
                {
                    var booking = await _guestAccountService.GetBookingByGuestAccountIdAsync(_guestAccount.Id);

                    TransactionItemDataGrid.ItemsSource = new List<GuestAccountSummaryDto> { _guestAccountSummaryDto };

                    var (BookingAmount, Discount, ServiceCharge, VAT, TotalAmount, TotalPaid, AmountToReceive, AmountToRefund) = Helper.CalculateSummary(_guestAccountSummaryDto);

                    UpdateTransactionSummaryUI(booking, _guestAccountSummaryDto, BookingAmount, Discount, ServiceCharge, VAT, TotalAmount, TotalPaid, AmountToReceive, AmountToRefund);

                    if (_guestAccountSummaryDto.BookingGroups.Count == 0)
                    {
                        SummaryPanel.Visibility = Visibility.Collapsed;
                    }

                    if (booking.IsSettled)
                    {
                        txtTitle.Text = "Booking Invoice List (Invoices Settled)";
                    }
                    else
                    {
                        txtTitle.Text = "Booking Invoice List (Invoices UnSettled)";
                    }
                }
                else
                {
                    SummaryPanel.Visibility = Visibility.Collapsed;
                }
            }
            catch (Exception ex)
            {
                LogError(ex);
            }
        }

        private async Task<FlowDocument> GetBillFlowDocument()
        {
            var booking = await _guestAccountService.GetBookingByGuestAccountIdAsync(_guestAccount.Id);
            var printer = new PrintHelper();

            var hotel = await _hotelSettingsService.GetHotelInformation();

            var doc = printer.GenerateGuestAccountFlowDocument(_guestAccountSummaryDto, booking, hotel);

            return doc;
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

        private async void Window_Activated(object sender, EventArgs e)
        {
            await LoadGuestTransactionHistoryAsync();
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

        private void PrintButon_Click(object sender, RoutedEventArgs e)
        {

        }

        private void PrintFolioButton_Click(object sender, RoutedEventArgs e)
        {

        }

        private async void Button_Click(object sender, RoutedEventArgs e)
        {
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
        }
    }
}
