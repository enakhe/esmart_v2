using ESMART.Application.Common.Interface;
using ESMART.Domain.Entities.Configuration;
using ESMART.Domain.Entities.FrontDesk;
using ESMART.Domain.Entities.Transaction;
using ESMART.Domain.ViewModels.FrontDesk;
using ESMART.Domain.ViewModels.Transaction;
using ESMART.Infrastructure.Repositories.Configuration;
using ESMART.Presentation.Forms.Export;
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

namespace ESMART.Presentation.Forms.Receipt
{
    /// <summary>
    /// Interaction logic for ReceiptViewerDialog.xaml
    /// </summary>
    public partial class ReceiptViewerDialog : Window
    {
        private readonly List<TransactionItemViewModel> _transactionItem;
        private readonly IHotelSettingsService _hotelSettingsService;
        private readonly Domain.Entities.FrontDesk.Booking _booking;
        private readonly decimal _amount = 0;
        public ReceiptViewerDialog(List<TransactionItemViewModel> transactionItem, IHotelSettingsService hotelSettingsService, Booking booking, decimal amount)
        {
            _transactionItem = transactionItem;
            _hotelSettingsService = hotelSettingsService;
            _booking = booking;
            _amount = amount;
            InitializeComponent();

            txtExpectedAmount.Text = "₦ " + _amount.ToString("N2");
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            LoaderOverlay.Visibility = Visibility.Visible;
            try
            {
                Transaction.ItemsSource = _transactionItem;
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

        private async void PrintButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                LoaderOverlay.Visibility = Visibility.Visible;
                try
                {
                    var columnNames = Transaction.Columns
                        .Where(c => c.Header != null)
                        .Select(c => c.Header.ToString())
                        .Where(name => !string.IsNullOrWhiteSpace(name) && name != "Operation")
                        .ToList();

                    var receiptExport = new ReceiptExport()
                    {
                        ReceiptNo = _booking.BookingId,
                        Cashier = AuthSession.CurrentUser?.FullName ?? "Unknown Cashier"
                    };

                    var optionsWindow = new ExportBillDialog(columnNames!, Transaction, _hotelSettingsService, _booking, null, null, $"Receipt {_booking.Guest.FullName}", _amount, receiptExport, System.Printing.PageOrientation.Portrait);
                    var result = optionsWindow.ShowDialog();

                    if (result == true)
                    {
                        this.DialogResult = true;
                        var exportResult = optionsWindow.GetResult();
                        var hotel = await _hotelSettingsService.GetHotelInformation();

                        if (exportResult.SelectedColumns.Count == 0)
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
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
        }
    }

    public class ReceiptExport
    {
        public string ReceiptNo { get; set; }
        public string Cashier { get; set; }
    }
}
