﻿#nullable disable

using ESMART.Application.Common.Dtos;
using ESMART.Application.Common.Interface;
using ESMART.Domain.Entities.Configuration;
using ESMART.Domain.Entities.Transaction;
using ESMART.Domain.ViewModels.FrontDesk;
using ESMART.Domain.ViewModels.Transaction;
using ESMART.Infrastructure.Repositories.FrontDesk;
using ESMART.Infrastructure.Services;
using ESMART.Presentation.Forms.Export;
using ESMART.Presentation.Forms.FrontDesk.Guest;
using ESMART.Presentation.Forms.Receipt;
using ESMART.Presentation.Session;
using ESMART.Presentation.Utils;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interop;

namespace ESMART.Presentation.Forms.FrontDesk.Booking
{
    /// <summary>
    /// Interaction logic for ViewBookingDetailsDialog.xaml
    /// </summary>
    public partial class ViewBookingDetailsDialog : Window
    {
        private readonly Domain.Entities.FrontDesk.Booking _booking;
        private readonly ITransactionRepository _transactionRepository;
        private readonly IBookingRepository _bookingRepository;
        private readonly IHotelSettingsService _hotelSettingsService;
        private readonly GuestAccountService _guestAccountService;
        public ViewBookingDetailsDialog(Domain.Entities.FrontDesk.Booking booking, ITransactionRepository transactionRepository, IBookingRepository bookingRepository, IHotelSettingsService hotelSettingsService, GuestAccountService guestAccountService)
        {
            _booking = booking;
            _transactionRepository = transactionRepository;
            _bookingRepository = bookingRepository;
            _guestAccountService = guestAccountService;
            _hotelSettingsService = hotelSettingsService;
            InitializeComponent();

            Loaded += DisableMinimizeButton;
        }

        private void LoadBookinDetails()
        {
            LoaderOverlay.Visibility = Visibility.Visible;
            try
            {
                var bookingViewModel = new BookingViewModel()
                {
                    Id = _booking.Id,
                    Guest = _booking.Guest.FullName,
                    PhoneNumber = _booking.Guest.PhoneNumber,
                    CheckIn = _booking.CheckIn,
                    CheckOut = _booking.CheckOut,
                    NumberOfRooms = _booking.RoomBookings.Count,
                    PaymentMethod = _booking.PaymentMethod.ToString(),
                    Duration = _booking.Duration.ToString(),
                    Status = _booking.Status.ToString(),
                    //TotalAmount = _booking.TotalAmount.ToString("N2"),
                    CreatedBy = _booking.ApplicationUser?.FullName,
                    DateCreated = _booking.DateCreated,
                    DateModified = _booking.DateModified
                };

                this.DataContext = bookingViewModel;
            }
            catch (Exception ex)
            {
                var sb = new StringBuilder();
                sb.AppendLine(ex.Message);
                MessageBox.Show(sb.ToString(), "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                LoaderOverlay.Visibility = Visibility.Collapsed;
            }
        }

        private async Task LoadBookingTransactionHistory()
        {
            LoaderOverlay.Visibility = Visibility.Visible;
            try
            {
                var bookingTransactionItem = await _guestAccountService.GetBookingAccountSummaryAsync(_booking.Id);
                if (bookingTransactionItem != null)
                {
                    TransactionItemDataGrid.ItemsSource = new List<BookingAccountSummaryDto> { bookingTransactionItem };
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
                    await LoadBookingTransactionHistory();
                }
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

                var optionsWindow = new ExportDialog(columnNames, TransactionItemDataGrid, _hotelSettingsService, $"{_booking.Guest.FullName} Booking {_booking.BookingId} Details");
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

        private async void BookingFolioButton_Click(object sender, RoutedEventArgs e)
        {
            LoaderOverlay.Visibility = Visibility.Visible;
            try
            {
                var guest = await _bookingRepository.GetBookingById(_booking.Id);

                if (guest != null)
                {
                    BookingFolioDialog bookingFolioDialog = new BookingFolioDialog(_booking, _transactionRepository, _hotelSettingsService);
                    bookingFolioDialog.ShowDialog();
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

                        if (transactionItems != null)
                        {
                            ReceiptViewerDialog receiptViewerDialog = new ReceiptViewerDialog(transactionItems, _hotelSettingsService, _booking, transactionItem.TotalAmount);
                            if (receiptViewerDialog.ShowDialog() == true)
                            {
                                this.DialogResult = true;
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
                            await LoadBookingTransactionHistory();
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

        private void Window_Activated(object sender, EventArgs e)
        {
            LoadBookinDetails();
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
    }
}
