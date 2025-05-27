using ESMART.Application.Common.Interface;
using ESMART.Application.Common.Utils;
using ESMART.Domain.Entities.Transaction;
using ESMART.Domain.ViewModels.Transaction;
using ESMART.Infrastructure.Repositories.FrontDesk;
using ESMART.Presentation.Forms.Export;
using ESMART.Presentation.Forms.Receipt;
using ESMART.Presentation.Utils;
using System.Windows;
using System.Windows.Controls;

namespace ESMART.Presentation.Forms.Reports
{
    /// <summary>
    /// Interaction logic for RoomTransactionReport.xaml
    /// </summary>
    public partial class RoomTransactionReport : Page
    {
        private readonly IRoomRepository _roomRepository;
        private readonly IHotelSettingsService _hotelSettingsService;
        private readonly ITransactionRepository _transactionRepository;
        private readonly IBookingRepository _bookingRepository;

        public RoomTransactionReport(IHotelSettingsService hotelSettingsService, ITransactionRepository transactionRepository, IRoomRepository roomRepository, IBookingRepository bookingRepository)
        {
            _roomRepository = roomRepository;
            _hotelSettingsService = hotelSettingsService;
            _transactionRepository = transactionRepository;
            _bookingRepository = bookingRepository;
            InitializeComponent();
        }

        private void LoadDefaultSetting()
        {
            txtFrom.SelectedDate = DateTime.Now;
            txtTo.SelectedDate = DateTime.Now.AddDays(1);
        }

        private async Task LoadRooms()
        {
            LoaderOverlay.Visibility = Visibility.Visible;
            try
            {
                var rooms = await _roomRepository.GetAllRooms();

                if (rooms != null)
                {
                    cmbRoom.ItemsSource = rooms;
                    cmbRoom.DisplayMemberPath = "Number";
                    cmbRoom.SelectedValuePath = "Id";
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

        private async Task FilterRoomTransaction()
        {
            LoaderOverlay.Visibility = Visibility.Visible;
            try
            {
                bool isNull = Helper.AreAnyNullOrEmpty(cmbRoom.SelectedValue.ToString()!, txtFrom.SelectedDate.ToString()!, txtTo.SelectedDate.ToString()!);

                if (!isNull)
                {
                    var fromDate = txtFrom.SelectedDate.Value;
                    var toDate = txtTo.SelectedDate.Value;
                    var roomId = ((Domain.Entities.RoomSettings.Room)cmbRoom.SelectedItem).Id;

                    var roomTransaction = await _transactionRepository.GetTransactionItemByRoomIdAndDate(roomId, fromDate, toDate);
                    TransactionItemDataGrid.ItemsSource = roomTransaction;
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

                var optionsWindow = new ExportDialog(columnNames, TransactionItemDataGrid, _hotelSettingsService, "Room Transaction Report");
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
                            await FilterRoomTransaction();
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
            await FilterRoomTransaction();
        }

        private async void Page_Loaded(object sender, RoutedEventArgs e)
        {
            LoadDefaultSetting();
            await LoadRooms();
        }
    }
}
