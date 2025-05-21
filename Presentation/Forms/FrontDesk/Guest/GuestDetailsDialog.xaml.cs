#nullable disable

using ESMART.Application.Common.Interface;
using ESMART.Domain.ViewModels.FrontDesk;
using ESMART.Presentation.Forms.Export;
using ESMART.Presentation.Utils;
using System.Runtime.InteropServices;
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

        private async void TabControl_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (e.Source is TabControl tabControl)
            {
                var selectedTab = tabControl.SelectedItem as TabItem;

                if (selectedTab == tbTransactionHistory)
                {
                    await LoadGuestTransactionHistory();
                }
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
