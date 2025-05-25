﻿using ESMART.Application.Common.Interface;
using ESMART.Application.Common.Utils;
using ESMART.Domain.ViewModels.Transaction;
using ESMART.Presentation.Forms.Export;
using ESMART.Presentation.Forms.Receipt;
using ESMART.Presentation.Utils;
using LiveCharts;
using LiveCharts.Wpf;
using System.Windows;
using System.Windows.Controls;

namespace ESMART.Presentation.Forms.Reports
{
    /// <summary>
    /// Interaction logic for DailyRevenueReport.xaml
    /// </summary>
    public partial class DailyRevenueReport : Page
    {
        private readonly IHotelSettingsService _hotelSettingsService;
        private readonly ITransactionRepository _transactionRepository;
        public SeriesCollection RevenueSeries { get; set; }
        public List<string> Days { get; set; }
        public Func<double, string> Formatter { get; set; }
        public DailyRevenueReport(IHotelSettingsService hotelSettingsService, ITransactionRepository transactionRepository)
        {
            _hotelSettingsService = hotelSettingsService;
            _transactionRepository = transactionRepository;
            InitializeComponent();

            RevenueSeries = new SeriesCollection();
        }
        private void LoadDefaultSetting()
        {
            txtFrom.SelectedDate = DateTime.Now;
            txtTo.SelectedDate = DateTime.Now.AddDays(1);
        }

        private async Task LoadData()
        {
            LoaderOverlay.Visibility = Visibility.Visible;
            try
            {
                bool isNull = Helper.AreAnyNullOrEmpty(txtFrom.SelectedDate.ToString()!, txtTo.SelectedDate.ToString()!);

                if (!isNull)
                {
                    var fromDate = txtFrom.SelectedDate.Value;
                    var toDate = txtTo.SelectedDate.Value;

                    var revenues = await _transactionRepository.GetRevenueByDateRange(fromDate, toDate);

                    // If there are revenues, update chart data
                    if (revenues.Any())
                    {
                        Days = revenues.Select(r => r.Date.ToString("yyyy-MM-dd")).ToList();

                        // Clear the existing data and add new series
                        RevenueSeries.Clear();  // Now RevenueSeries is guaranteed to be initialized
                        RevenueSeries.Add(new ColumnSeries
                        {
                            Values = new ChartValues<double>(revenues.Select(r => (double)r.TotalRevenue)),
                            Name = "Revenue"
                        });

                        Formatter = value => value.ToString("C");
                    }
                    else
                    {
                        // If no data, clear the series
                        if (Days != null)
                        {
                            Days.Clear();
                        }
                        RevenueSeries.Clear();
                        RevenueSeries.Add(new ColumnSeries
                        {
                            Values = new ChartValues<double>(), // Empty data for no revenue
                            Name = "Revenue"
                        });
                    }

                    // Ensure the chart is updated
                    DataContext = this;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading rooms: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                LoaderOverlay.Visibility = Visibility.Collapsed;
            }
        }


        private async Task GetTransactionsData()
        {
            LoaderOverlay.Visibility = Visibility.Visible;
            try
            {
                var fromDate = txtFrom.SelectedDate.Value;
                var toDate = txtTo.SelectedDate.Value;

                var transactions = await _transactionRepository.GetTransactionItemsByDateAsync(fromDate, toDate);
                TransactionItemDataGrid.ItemsSource = transactions;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading transactions: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
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

                        var hotel = await _hotelSettingsService.GetHotelInformation();
                        if (hotel != null)
                        {
                            if (transactionItem != null)
                            {
                                ReceiptViewerDialog receiptViewerDialog = new ReceiptViewerDialog(hotel, transactionItem);
                                if (receiptViewerDialog.ShowDialog() == true)
                                {
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

                var optionsWindow = new ExportDialog(columnNames, TransactionItemDataGrid, _hotelSettingsService, $"Daily Revenue");
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

        private async void FilterButton_Click(object sender, RoutedEventArgs e)
        {
            // Clear existing data and refresh the chart
            RevenueSeries.Clear();

            if(Days != null)
            {
                Days.Clear();
            }

            await LoadData();
            await GetTransactionsData();
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

                            RevenueSeries.Clear();

                            if (Days != null)
                            {
                                Days.Clear();
                            }

                            await LoadData();
                            await GetTransactionsData();
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

        private async void Page_Loaded(object sender, RoutedEventArgs e)
        {
            LoadDefaultSetting();
            await LoadData();
            await GetTransactionsData();
        }
    }
}
