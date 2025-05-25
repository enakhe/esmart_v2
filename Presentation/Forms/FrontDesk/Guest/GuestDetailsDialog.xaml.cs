﻿#nullable disable

using ESMART.Application.Common.Interface;
using ESMART.Domain.Entities.FrontDesk;
using ESMART.Domain.Enum;
using ESMART.Domain.ViewModels.FrontDesk;
using ESMART.Domain.ViewModels.Transaction;
using ESMART.Presentation.Forms.Export;
using ESMART.Presentation.Forms.Receipt;
using ESMART.Presentation.Utils;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interop;

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
        public GuestDetailsDialog(string id, IGuestRepository guestRepository, ITransactionRepository transactionRepository, IHotelSettingsService hotelSettingsService)
        {
            _id = id;
            _guestRepository = guestRepository;
            _transactionRepository = transactionRepository;
            _hotelSettingsService = hotelSettingsService;
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
                    var guestAccount = await _guestRepository.GetGuestTransactionsAsync(_id);

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
                        CurrentBalance = guestAccount.Where(g => g.TransactionType == TransactionType.Credit).Sum(g => g.Amount) - guestAccount.Where(g => g.TransactionType == TransactionType.Debit).Sum(g => g.Amount),
                        PhoneNumber = guest.PhoneNumber,
                        City = guest.City,
                        State = guest.State,
                        Country = guest.Country,
                        CreatedBy = guest.ApplicationUser?.FullName,
                        DateCreated = guest.DateCreated,
                        DateModified = guest.DateModified,
                    };

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

        private void LoadDefaultSetting()
        {
            txtFrom.SelectedDate = DateTime.Now;
            txtTo.SelectedDate = DateTime.Now.AddDays(1);
        }

        private async Task LoadGuestTransactionHistory()
        {
            LoaderOverlay.Visibility = Visibility.Visible;
            try
            {
                var guestTransactionItem = await _transactionRepository.GetTransactionItemsByGuestIdAsync(_id);
                if (guestTransactionItem != null)
                {
                    this.TransactionItemDataGrid.ItemsSource = guestTransactionItem;
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

        private async Task LoadGuestAccountTransactionHistory()
        {
            LoaderOverlay.Visibility = Visibility.Visible;
            try
            {
                var guestAccountTransaction = await _guestRepository.GetGuestTransactionsAsync(_id);
                if (guestAccountTransaction != null)
                {
                    this.AccTransactionItemDataGrid.ItemsSource = guestAccountTransaction;
                }

                txtIn.Text = $"In: ₦{guestAccountTransaction.Sum(g => g.Amount).ToString("N2")}";
                txtOut.Text = $"Out: ₦{guestAccountTransaction.Where(g => g.TransactionType == TransactionType.Debit).Sum(g => g.Amount).ToString("N2")}";
                txtCurrent.Text = $"Current Balance: ₦{guestAccountTransaction.Where(g => g.TransactionType == TransactionType.Credit).Sum(g => g.Amount) - guestAccountTransaction.Where(g => g.TransactionType == TransactionType.Debit).Sum(g => g.Amount):N2}";
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

        private async Task LoadGuestTransactionByDate()
        {
            LoaderOverlay.Visibility = Visibility.Visible;
            try
            {
                var fromDate = txtFrom.SelectedDate.Value;
                var toDate = txtTo.SelectedDate.Value;

                if (fromDate > toDate)
                {
                    MessageBox.Show("From date cannot be greater than To date", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                var guestTransactionItem = await _transactionRepository.GetTransactionItemByGuestIdAndDate(_id, fromDate, toDate);

                if (guestTransactionItem != null)
                {
                    this.TransactionItemDataGrid.ItemsSource = guestTransactionItem;
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
            LoadDefaultSetting();
        }

        private async void FilterButton_Click(object sender, RoutedEventArgs e)
        {
            await LoadGuestTransactionByDate();
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
                    await LoadGuestTransactionHistory();
                }
                if (selectedTab == tcAccountHistory)
                {
                    await LoadGuestAccountTransactionHistory();
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
                    GuestFolioDialog guestFolioDialog = new GuestFolioDialog(guest, _transactionRepository, _hotelSettingsService);
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
                        var transactionItem = await _transactionRepository.GetTransactionItemsByIdAsync(selectedTransaction.Id);
                        if (transactionItem != null)
                        {
                            await _transactionRepository.MarkTransactionItemAsPaidAsync(transactionItem.Id);
                            MessageBox.Show("Transaction marked as paid successfully", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                            await LoadGuestDetails();
                            LoadDefaultSetting();
                            await LoadGuestTransactionHistory();
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
    }
}
