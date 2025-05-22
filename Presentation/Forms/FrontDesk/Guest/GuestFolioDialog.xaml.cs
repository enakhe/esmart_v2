using ESMART.Application.Common.Interface;
using ESMART.Domain.ViewModels.Transaction;
using ESMART.Infrastructure.Repositories.Configuration;
using ESMART.Presentation.Forms.Export;
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

namespace ESMART.Presentation.Forms.FrontDesk.Guest
{
    /// <summary>
    /// Interaction logic for GuestFolioDialog.xaml
    /// </summary>
    public partial class GuestFolioDialog : Window
    {
        private readonly Domain.Entities.FrontDesk.Guest _guest;
        private readonly ITransactionRepository _transactionRepository;
        private readonly TransactionPageViewModel _viewModel;
        private readonly IHotelSettingsService _hotelSettingsService;
        public GuestFolioDialog(Domain.Entities.FrontDesk.Guest guest, ITransactionRepository transactionRepository, IHotelSettingsService hotelSettingsService)
        {
            _guest = guest;
            _transactionRepository = transactionRepository;
            _hotelSettingsService = hotelSettingsService;
            InitializeComponent();

            Loaded += DisableMinimizeButton;
            _viewModel = new TransactionPageViewModel();
            this.DataContext = _viewModel;

            txtGuestName.Text = _guest.FullName;
        }

        private async Task LoadTransaction()
        {
            LoaderOverlay.Visibility = Visibility.Visible;
            try
            {
                var transactions = await _transactionRepository.GetTransactionByGuestIdAsync(_guest.Id);

                _viewModel.Transactions.Clear();

                foreach (var transaction in transactions)
                    _viewModel.Transactions.Add(transaction);
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

        private async void GetTransactionItem_Click(object sender, RoutedEventArgs e)
        {
            LoaderOverlay.Visibility = Visibility.Visible;
            try
            {
                var transactionId = (sender as Button)?.Tag.ToString();

                if (string.IsNullOrEmpty(transactionId))
                {
                    MessageBox.Show("Transaction ID is not valid.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                var transaction = await _transactionRepository.GetByTransactionIdAsync(transactionId);

                if (transaction == null)
                {
                    MessageBox.Show("Transaction not found.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                var transactionItems = await _transactionRepository.GetTransactionItemsByTransactionIdAsync(transactionId);
                
                TransactionItemDataGrid.ItemsSource = transactionItems;

                txtReceivables.Text = $"Total Receivables: ₦{transaction.TotalReceivables:N2}";
                txtPayables.Text = $"Total Payables: ₦{transaction.TotalRevenue:N2}";

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
                var columnNames = TransactionItemDataGrid.Columns
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

        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            await LoadTransaction();
        }
    }
}
