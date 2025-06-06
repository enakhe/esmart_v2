﻿using ESMART.Application.Common.Interface;
using ESMART.Domain.Entities.FrontDesk;
using ESMART.Domain.Entities.Transaction;
using ESMART.Domain.ViewModels.Transaction;
using ESMART.Infrastructure.Repositories.FrontDesk;
using ESMART.Presentation.Forms.Export;
using ESMART.Presentation.Forms.Receipt;
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

namespace ESMART.Presentation.Forms.FrontDesk.Room
{
    /// <summary>
    /// Interaction logic for RoomFolioDialog.xaml
    /// </summary>
    public partial class RoomFolioDialog : Window
    {
        private readonly Domain.Entities.RoomSettings.Room _room;
        private readonly ITransactionRepository _transactionRepository;
        private readonly TransactionPageViewModel _viewModel;
        private readonly IBookingRepository _bookingRepository;
        private readonly IHotelSettingsService _hotelSettingsService;
        public RoomFolioDialog(Domain.Entities.RoomSettings.Room room, ITransactionRepository transactionRepository, IHotelSettingsService hotelSettingsService, IBookingRepository bookingRepository)
        {
            _room = room;
            _transactionRepository = transactionRepository;
            _bookingRepository = bookingRepository;
            _hotelSettingsService = hotelSettingsService;
            InitializeComponent();

            Loaded += DisableMinimizeButton;
            _viewModel = new TransactionPageViewModel();
            this.DataContext = _viewModel;

            txtRoomNo.Text = _room.Number;
        }

        private async Task LoadTransaction()
        {
            LoaderOverlay.Visibility = Visibility.Visible;
            try
            {
                var transactions = await _transactionRepository.GetTransactionByRoomNoAsync(_room.Number);

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

                await LoadTransationItem(transactionId);

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

        private async Task LoadTransationItem(string transactionId)
        {
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

            txtReceivables.Text = $"Total Receivables: ₦{transaction.Balance:N2}";
            txtPayables.Text = $"Total Payables: ₦{transaction.Paid:N2}";
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

                var optionsWindow = new ExportDialog(columnNames, TransactionItemDataGrid, _hotelSettingsService, $"{_room.Number} Folio");
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
                        var transaction = await _transactionRepository.GetByTransactionItemIdAsync(selectedTransaction.Id);

                        var transactionItem = await _transactionRepository.GetTransactionItemsByIdAsync(selectedTransaction.Id);
                        if (transactionItem != null)
                        {
                            await _transactionRepository.MarkTransactionItemAsPaidAsync(transactionItem.Id);
                            MessageBox.Show("Transaction marked as paid successfully", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                            await LoadTransationItem(transaction.Id);
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
