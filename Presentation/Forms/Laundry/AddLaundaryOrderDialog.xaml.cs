using ESMART.Application.Common.Dtos;
using ESMART.Application.Common.Interface;
using ESMART.Application.Common.Utils;
using ESMART.Domain.Entities.Laundry;
using ESMART.Domain.Entities.StoreKeeping;
using ESMART.Domain.Entities.Transaction;
using ESMART.Domain.Enum;
using ESMART.Domain.ViewModels.Laundry;
using ESMART.Infrastructure.Repositories.StockKeeping;
using ESMART.Infrastructure.Services;
using ESMART.Presentation.Forms.StockKeeping.Order;
using ESMART.Presentation.Session;
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

namespace ESMART.Presentation.Forms.Laundry
{
    /// <summary>
    /// Interaction logic for AddLaundaryOrderDialog.xaml
    /// </summary>
    public partial class AddLaundaryOrderDialog : Window
    {
        private readonly LaundryViewModel _itemViewModel;
        private readonly GuestAccountService _guestAccountService;
        private readonly ITransactionRepository _transactionRepository;

        public AddLaundaryOrderDialog(GuestAccountService guestAccountService, ITransactionRepository transactionRepository)
        {
            _guestAccountService = guestAccountService;
            _transactionRepository = transactionRepository;

            _itemViewModel = new LaundryViewModel();
            Loaded += DisableMinimizeButton;
            this.DataContext = _itemViewModel;
            InitializeComponent();
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

        private async Task LoadActiveBooking()
        {
            LoaderOverlay.Visibility = Visibility.Visible;
            try
            {
                var activeBooking = await _guestAccountService.GetCurrentBooking();

                if (activeBooking != null && activeBooking.Count != 0)
                {
                    var displayList = activeBooking.Select(b => new BookingDisplayItem
                    {
                        GuestId = b.Booking.GuestId,
                        RoomId = b.Room.Id,
                        BookingId = b.Booking.Id,
                        Consumer = b.OccupantName,
                        DisplayName = $"{b.OccupantName} - Room {b.Room.Number}"
                    }).ToList();

                    cmbActiveBooking.ItemsSource = displayList;
                    cmbActiveBooking.DisplayMemberPath = "DisplayName";
                    cmbActiveBooking.SelectedValuePath = "Id";
                    cmbActiveBooking.SelectedIndex = 0;
                }
                else
                {
                    MessageBox.Show("No active booking found.", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
                }

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

        public void LoadPaymentMethod()
        {
            try
            {
                var method = Enum.GetValues<PaymentMethod>()
                    .Cast<PaymentMethod>()
                    .Select(e => new { Id = (int)e, Name = e.ToString() })
                    .ToList();

                cmbPaymentMethod.ItemsSource = method;
                cmbPaymentMethod.DisplayMemberPath = "Name";
                cmbPaymentMethod.SelectedValuePath = "Name";
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public async Task LoadBankAccount()
        {
            try
            {
                var accountNumber = await _transactionRepository.GetAllBankAccountAsync();

                cmbAccountNumber.ItemsSource = accountNumber;
                cmbAccountNumber.DisplayMemberPath = "BankAccountNumber";
                cmbAccountNumber.SelectedValuePath = "Id";
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public void LoadServiceArea()
        {
            LoaderOverlay.Visibility = Visibility.Visible;
            try
            {
                var area = Enum.GetValues<LaundaryCategory>()
                    .Cast<LaundaryCategory>()
                    .Select(e => new { Id = (int)e, Name = e.ToString() })
                    .ToList();

                cmbCategory.ItemsSource = area;
                cmbCategory.DisplayMemberPath = "Name";
                cmbCategory.SelectedValuePath = "Name";
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

        private async void Order_Click(object sender, RoutedEventArgs e)
        {
            LoaderOverlay.Visibility = Visibility.Visible;
            try
            {
                if (_itemViewModel.CartItems.Count > 0)
                {
                    if (cmbActiveBooking.SelectedItem is BookingDisplayItem selectedBooking)
                    {
                        var guestAccount = await _guestAccountService.GetAccountAsync(selectedBooking.GuestId);
                        var roomBooking = await _guestAccountService.GetRoomBookingByRoomIdAsync(selectedBooking.RoomId);
                        var activeUser = AuthSession.CurrentUser.FullName;

                        var createOrderDto = new LaundryOrderDto()
                        {
                            Invoice = guestAccount.Invoice,
                            BookingId = selectedBooking.BookingId,
                            RoomBookingId = roomBooking.Id,
                            RoomId = selectedBooking.RoomId,
                            Consumer = selectedBooking.Consumer,
                            GuestAccountId = guestAccount.Id,
                            OrderId = Helper.GenerateInvoiceNumber("OR"),
                            GuestId = selectedBooking.GuestId,
                            Amount = _itemViewModel.TotalAmount,
                            TransactionType = TransactionType.Laundry,
                            BankAccountId = cmbAccountNumber.SelectedItem is BankAccount selectedBankAccount ? selectedBankAccount.Id : null,
                            PaymentMethod = cmbPaymentMethod.SelectedItem != null ? Enum.Parse<PaymentMethod>(cmbPaymentMethod.SelectedValue.ToString()!) : PaymentMethod.Other,
                            ApplicationUserId = AuthSession.CurrentUser.Id,
                            OrderItems = [.. _itemViewModel.CartItems.Select(ci => new LaundaryOrderItem
                            {
                                LaundryOrderItemId = Helper.GenerateInvoiceNumber("OR"),
                                LaundaryId = ci.Id,
                                Quantity = ci.Quantity,
                                UnitPrice = ci.TotalPrice,
                            })],
                        };

                        var orderId = await _guestAccountService.CreateLaundaryOrder(createOrderDto, activeUser);
                        MessageBox.Show("Successfully placed order", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                        this.DialogResult = true;
                    }
                    else
                    {
                        MessageBox.Show("Please select a booking.", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                    }
                }
                else
                {
                    MessageBox.Show("Please add items to the cart.", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                }


            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occured when placing order. {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                LoaderOverlay.Visibility = Visibility.Collapsed;
            }
        }

        private void SearchButton_Click(object sender, RoutedEventArgs e)
        {

        }
        private async Task LoadItem(LaundaryCategory category)
        {
            try
            {
                var groupedItems = await _guestAccountService.GetLaundryItemsByCategoryAsync(category);

                _itemViewModel.GroupedLaundryItems.Clear();

                foreach (var group in groupedItems)
                {
                    _itemViewModel.GroupedLaundryItems.Add((LaundryCategoryGroup)group);
                }

                _itemViewModel.CalculateTotalAmount();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void RemoveCart_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is string itemId)
            {
                _itemViewModel.DecreaseOrRemoveFromCart(itemId);
            }
        }

        private void AddButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is string itemId)
            {
                var item = _itemViewModel.GroupedLaundryItems
                    .SelectMany(g => g.Items)
                    .FirstOrDefault(m => m.Id == itemId);

                if (item == null) return;

                var existingCartItem = _itemViewModel.CartItems.FirstOrDefault(ci => ci.Id == item.Id);

                if (existingCartItem != null)
                {
                    existingCartItem.Quantity++;
                }
                else
                {
                    _itemViewModel.CartItems.Add(new CartItem
                    {
                        Id = item.Id,
                        Name = item.Description,
                        LaundryPrice = item.LaundryPrice,
                        PressingPrice = item.PressingPrice,
                        Quantity = 1,
                        IsPressingSelected = false // Initially unchecked
                    });
                }
            }
        }

        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            await LoadBankAccount();
            LoadPaymentMethod();
            await LoadActiveBooking();
            LoadServiceArea();
        }

        private async void cmbCategory_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            LoaderOverlay.Visibility = Visibility.Visible;
            try
            {
                var selectecCategory = Enum.Parse<LaundaryCategory>(cmbCategory.SelectedValue.ToString()!);
                await LoadItem(selectecCategory);
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
    }
}
