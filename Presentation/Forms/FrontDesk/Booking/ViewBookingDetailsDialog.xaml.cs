#nullable disable

using ESMART.Application.Common.Interface;
using ESMART.Domain.ViewModels.FrontDesk;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
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
        public ViewBookingDetailsDialog(Domain.Entities.FrontDesk.Booking booking, ITransactionRepository transactionRepository, IBookingRepository bookingRepository)
        {
            _booking = booking;
            _transactionRepository = transactionRepository;
            _bookingRepository = bookingRepository;
            InitializeComponent();
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
                    GuestPhoneNo = _booking.Guest.PhoneNumber,
                    Room = _booking.Room.Number,
                    CheckIn = _booking.CheckIn,
                    CheckOut = _booking.CheckOut,
                    PaymentMethod = _booking.PaymentMethod.ToString(),
                    Duration = _booking.Duration.ToString(),
                    Status = _booking.Status.ToString(),
                    TotalAmount = _booking.TotalAmount,
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

        private void LoadDefaultSetting()
        {
            txtFrom.SelectedDate = DateTime.Now;
            txtTo.SelectedDate = DateTime.Now.AddDays(1);
        }

        private async Task LoadBookingTransactionHistory()
        {
            LoaderOverlay.Visibility = Visibility.Visible;
            try
            {
                var bookingTransactionItem = await _transactionRepository.GetTransactionItemByBookingIdAsync(_booking.Id);
                if (bookingTransactionItem != null)
                {
                    this.TransactionItemDataGrid.ItemsSource = bookingTransactionItem;
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

        private async Task LoadBookingTransactionByDate()
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

                var bookingTransactionItem = await _transactionRepository.GetTransactionItemByBookingIdAndDate(_booking.Id, fromDate, toDate);

                if (bookingTransactionItem != null)
                {
                    this.TransactionItemDataGrid.ItemsSource = bookingTransactionItem;
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
            await LoadBookingTransactionByDate();
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

        private void Window_Activated(object sender, EventArgs e)
        {
            LoadBookinDetails();
            LoadDefaultSetting();
        }
    }
}
