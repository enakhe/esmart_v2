using ESMART.Application.Common.Interface;
using ESMART.Domain.Entities.Configuration;
using ESMART.Domain.Entities.Transaction;
using ESMART.Domain.ViewModels.FrontDesk;
using ESMART.Domain.ViewModels.Transaction;
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
        private readonly Hotel _hotel;
        private readonly TransactionItem _transactionItem;
        public ReceiptViewerDialog(Hotel hotel, TransactionItem transactionItem)
        {
            _hotel = hotel;
            _transactionItem = transactionItem;
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            LoaderOverlay.Visibility = Visibility.Visible;
            try
            {
                //var receiptHelper = new ReceiptHelper();
                //var document = receiptHelper.GeneratePreviewFlowDocument(_transactionItem, _hotel.Name, _hotel.Address, _hotel.PhoneNumber);
                //ReceiptViewer.Document = document;
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

        private void PrintButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                ReceiptViewer.Print();
                this.DialogResult = true;
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
}
