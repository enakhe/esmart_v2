#nullable disable

using ESMART.Application.Common.Dtos;
using ESMART.Application.Common.Interface;
using ESMART.Application.Common.Utils;
using ESMART.Domain.Entities.FrontDesk;
using ESMART.Domain.Entities.RoomSettings;
using ESMART.Domain.Entities.StoreKeeping;
using ESMART.Domain.Entities.Transaction;
using ESMART.Domain.Enum;
using ESMART.Domain.ViewModels.StoreKepping;
using ESMART.Infrastructure.Repositories.FrontDesk;
using ESMART.Infrastructure.Services;
using ESMART.Presentation.Session;
using Google.Apis.Drive.v3.Data;
using Microsoft.Identity.Client;
using Org.BouncyCastle.Crypto.Modes;
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

namespace ESMART.Presentation.Forms.StockKeeping.Order
{
    /// <summary>
    /// Interaction logic for OrderDialog.xaml
    /// </summary>
    public partial class OrderDialog : Window
    {
        private readonly OrderViewModel _viewModel;
        private readonly IStockKeepingRepository _stockKeepingRepository;
        private readonly IBookingRepository _bookingRepository;
        private readonly IGuestRepository _guestRepository;
        private readonly ITransactionRepository _transactionRepository;
        private readonly GuestAccountService _guestAccountService;

        public OrderDialog(
            IStockKeepingRepository stockKeepingRepository, 
            IBookingRepository bookingRepository, 
            IGuestRepository guestRepository, 
            ITransactionRepository transactionRepository, 
            GuestAccountService guestAccountService)
        {
            _stockKeepingRepository = stockKeepingRepository;
            _bookingRepository = bookingRepository;
            _guestRepository = guestRepository;
            _transactionRepository = transactionRepository;
            _guestAccountService = guestAccountService;

            _viewModel = new OrderViewModel();
            Loaded += DisableMinimizeButton;
            this.DataContext = _viewModel;
            InitializeComponent();
        }

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
                var area = Enum.GetValues<ServiceArea>()
                    .Cast<ServiceArea>()
                    .Select(e => new { Id = (int)e, Name = e.ToString() })
                    .ToList();

                cmbServiceArea.ItemsSource = area;
                cmbServiceArea.DisplayMemberPath = "Name";
                cmbServiceArea.SelectedValuePath = "Name";
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

        private async Task LoadItem(string category)
        {
            try
            {
                var groupedItems = await _stockKeepingRepository.GetGroupedMenuItemsAsync(category);

                _viewModel.GroupedMenuItems.Clear();

                foreach (var group in groupedItems)
                {
                    _viewModel.GroupedMenuItems.Add((MenuCategoryGroup)group);
                }

                _viewModel.CalculateTotalAmount();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void AddMenuItemToCart_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is string itemId)
            {
                var item = _viewModel.GroupedMenuItems
                    .SelectMany(g => g.Items)
                    .FirstOrDefault(m => m.Id == itemId);

                if (item == null) return;

                var existingCartItem = _viewModel.CartItems.FirstOrDefault(ci => ci.Id == item.Id);
                if (existingCartItem != null)
                {
                    existingCartItem.Quantity++;
                }
                else
                {
                    _viewModel.CartItems.Add(new CartItem
                    {
                        Id = item.Id,
                        Name = item.Name,
                        Price = item.Price,
                        Quantity = 1
                    });
                }

                _viewModel.CalculateTotalAmount();
            }
        }

        private void RemoveCartItem_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is string itemId)
            {
                _viewModel.DecreaseOrRemoveFromCart(itemId);
            }
        }

        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            await LoadActiveBooking();
            await LoadBankAccount();
            LoadPaymentMethod();
            LoadServiceArea();
        }

        private async void cmbServiceArea_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            LoaderOverlay.Visibility = Visibility.Visible;
            try
            {
                var menuCategory = await _stockKeepingRepository.GetMenuCategoriesByServiceAreaAsync(ServiceArea.Restaurant);

                cmbCategory.ItemsSource = menuCategory;
                cmbCategory.DisplayMemberPath = "Name";
                cmbCategory.SelectedValuePath = "Id";
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

        private async void cmbCategory_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            LoaderOverlay.Visibility = Visibility.Visible;
            try
            {
                var selectecCategory = ((Domain.Entities.StoreKeeping.MenuCategory)cmbCategory.SelectedItem).Name;
                if (selectecCategory != null)
                {
                    await LoadItem(selectecCategory);
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

        private async void btnCreateOrder_Click(object sender, RoutedEventArgs e)
        {
            LoaderOverlay.Visibility = Visibility.Visible;
            try
            {
                if(cmbAccountNumber.SelectedItem != null || cmbPaymentMethod.SelectedItem != null)
                {
                    if(_viewModel.CartItems.Count > 0)
                    {
                        if (cmbActiveBooking.SelectedItem is BookingDisplayItem selectedBooking)
                        {
                            var guestAccount = await _guestAccountService.GetAccountAsync(selectedBooking.GuestId);

                            var roomBooking = await _guestAccountService.GetRoomBookingByRoomIdAsync(selectedBooking.RoomId);

                            var createOrderDto = new CreateOrderDto()
                            {
                                Invoice = guestAccount.Invoice,
                                BookingId = selectedBooking.BookingId,
                                RoomBookingId = roomBooking.Id,
                                RoomId = selectedBooking.RoomId,
                                Consumer = selectedBooking.Consumer,
                                GuestAccountId = guestAccount.Id,
                                OrderId = Helper.GenerateInvoiceNumber("OR"),
                                GuestId = selectedBooking.GuestId,
                                Amount = _viewModel.TotalAmount,
                                TransactionType = TransactionType.BarRestaurantOrder,
                                BankAccountId = ((BankAccount)cmbAccountNumber.SelectedItem).Id,
                                PaymentMethod = Enum.Parse<PaymentMethod>(cmbPaymentMethod.SelectedValue.ToString()!),
                                ApplicationUserId = AuthSession.CurrentUser.Id,
                                OrderItems = [.. _viewModel.CartItems.Select(ci => new OrderItem
                                {
                                    OrderItemId = Helper.GenerateInvoiceNumber("OR"),
                                    MenuItemId = ci.Id,
                                    Quantity = ci.Quantity,
                                    UnitPrice = ci.Price
                                })],
                            };

                            var orderId = await _guestAccountService.CreateOrder(createOrderDto);
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
                        MessageBox.Show("Please add items to cart.", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                    }

                }
                else
                {
                    MessageBox.Show("Please fill in all required fields.", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
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
    public class BookingDisplayItem
    {
        public string GuestId { get; set; }
        public string BookingId { get; set; }
        public string DisplayName { get; set; }
        public string RoomId { get; set; }
        public string Consumer { get; set; }
    }

}
