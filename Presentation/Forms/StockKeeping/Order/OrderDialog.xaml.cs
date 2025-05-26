#nullable disable

using ESMART.Application.Common.Interface;
using ESMART.Domain.Entities.FrontDesk;
using ESMART.Domain.Entities.RoomSettings;
using ESMART.Domain.Entities.StoreKeeping;
using ESMART.Domain.Entities.Transaction;
using ESMART.Domain.Enum;
using ESMART.Domain.ViewModels.StoreKepping;
using ESMART.Infrastructure.Repositories.FrontDesk;
using ESMART.Presentation.Session;
using Google.Apis.Drive.v3.Data;
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
        private bool _isUnpaid = false;
        public OrderDialog(IStockKeepingRepository stockKeepingRepository, IBookingRepository bookingRepository, IGuestRepository guestRepository, ITransactionRepository transactionRepository)
        {
            _stockKeepingRepository = stockKeepingRepository;
            _bookingRepository = bookingRepository;
            _guestRepository = guestRepository;
            _transactionRepository = transactionRepository;

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
                var activeBooking = await _bookingRepository.GetActiveBooking();

                if (activeBooking != null && activeBooking.Any())
                {
                    var displayList = activeBooking.Select(b => new BookingDisplayItem
                    {
                        GuestId = b.GuestId,
                        BookingId = b.Id,
                        DisplayName = $"{b.Guest.FullName} - Room {b.Room.Number}"
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

        private void MarkAsUnPaid_Click(object sender, RoutedEventArgs e)
        {
            _isUnpaid = !_isUnpaid;
            UnpaidBtn.Background = _isUnpaid
                ? new SolidColorBrush(Colors.Red)
                : new SolidColorBrush(Colors.Black);
        }

        private async Task LoadItem(string category)
        {
            try
            {
                var groupedItems = await _stockKeepingRepository.GetGroupedMenuItemsAsync(category);

                _viewModel.GroupedMenuItems.Clear(); // not MenuItems, since you're loading groups

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

            await LoadActiveBooking();
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

            }
        }

        private async Task<bool> CheckIfGuestHasAccount(Domain.Entities.FrontDesk.Guest guest)
        {
            LoaderOverlay.Visibility = Visibility.Visible;
            try
            {
                var guestAccount = await _guestRepository.GetGuestAccountByGuestIdAsync(guest.Id);
                if (guestAccount != null && !guestAccount.IsClosed)
                {
                    return true;
                }

                return false;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
            finally
            {
                LoaderOverlay.Visibility = Visibility.Collapsed;
            }
        }

        private async Task ChargeGuestAccount(Domain.Entities.FrontDesk.Guest guest, decimal amount)
        {
            LoaderOverlay.Visibility = Visibility.Visible;
            try
            {
                var guestAccount = await _guestRepository.GetGuestAccountByGuestIdAsync(guest.Id);

                if (guestAccount != null)
                {
                    guestAccount.FundedBalance -= amount;
                    guestAccount.TotalCharges += amount;
                    guestAccount.LastFunded = DateTime.Now;

                    await _guestRepository.UpdateGuestAccountAsync(guestAccount);
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

        private async Task AddGuestTransaction(Domain.Entities.FrontDesk.Guest guest, decimal amount)
        {
            LoaderOverlay.Visibility = Visibility.Visible;
            try
            {
                var guestTransaction = new GuestTransaction
                {
                    Id = Guid.NewGuid().ToString(),
                    GuestId = guest.Id,
                    Amount = amount,
                    TransactionType = TransactionType.Debit,
                    Description = "Order Payment",
                    Date = DateTime.Now,
                    ApplicationUserId = AuthSession.CurrentUser?.Id
                };
                await _guestRepository.AddGuestTransactionAsync(guestTransaction);
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

        private async Task<bool> CheckIfGuestHasMoney(Domain.Entities.FrontDesk.Guest guest, decimal amount)
        {
            try
            {
                var guestAccount = await _guestRepository.GetGuestAccountByGuestIdAsync(guest.Id);
                if (guestAccount != null && guestAccount.FundedBalance > amount)
                {
                    return true;
                }
                return false;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
        }

        private async Task CreateTransactionItem(string guestId, bool isUnPaid, string serviceId)
        {
            try
            {
                var transaction = await _transactionRepository.GetByGuestIdAsync(guestId);
                if (transaction != null)
                {
                    var transactionItem = new TransactionItem()
                    {
                        Id = Guid.NewGuid().ToString(),
                        TransactionId = transaction.Id,
                        Amount = _viewModel.TotalAmount,
                        Description = "Order Payment",
                        DateAdded = DateTime.Now,
                        BankAccount = "",
                        Category = Category.FoodAndBeverage,
                        ServiceId = serviceId,
                        Type = TransactionType.Charge,
                        Status = TransactionStatus.Unpaid,
                        Discount = 0,
                        TaxAmount = 0,
                        ServiceCharge = 0,
                        TotalAmount = _viewModel.TotalAmount,
                        ApplicationUserId = AuthSession.CurrentUser?.Id
                    };

                    if (!isUnPaid)
                    {
                        transactionItem.Status = TransactionStatus.Paid;
                    }

                    await _transactionRepository.AddTransactionItemAsync(transactionItem);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // create order
        private async Task CreateOrder(string bookingId, Domain.Entities.FrontDesk.Guest guest)
        {
            try
            {
                var order = new Domain.Entities.StoreKeeping.Order
                {
                    Id = Guid.NewGuid().ToString(),
                    BookingId = bookingId,
                    CreatedAt = DateTime.UtcNow,
                    OrderItems = _viewModel.CartItems.Select(ci => new OrderItem
                    {
                        OrderId = $"Or{Guid.NewGuid().ToString().Split('-')[0].ToUpper().AsSpan(0, 5)}",
                        MenuItemId = ci.Id,
                        Quantity = ci.Quantity,
                        UnitPrice = ci.Price
                    }).ToList()
                };

                await _stockKeepingRepository.AddOrderAsync(order);

                if (await CheckIfGuestHasAccount(guest))
                {
                    await ChargeGuestAccount(guest, _viewModel.TotalAmount);
                    await AddGuestTransaction(guest, _viewModel.TotalAmount);

                    if (await CheckIfGuestHasMoney(guest, _viewModel.TotalAmount))
                    {
                        _isUnpaid = false;
                    }
                    else
                    {
                        _isUnpaid = true;
                        MessageBox.Show("Guest does not have enough balance to pay for the booking. Payment will be flagged as pending.", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                }

                await CreateTransactionItem(guest.Id, _isUnpaid, order.OrderItems.First().OrderId);

                MessageBox.Show("Order created successfully.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async void cmbActiveBooking_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var guestId = ((BookingDisplayItem)cmbActiveBooking.SelectedItem).GuestId;

            if (guestId != null)
            {
                var guest = await _guestRepository.GetGuestByIdAsync(guestId);
                if (guest != null)
                {
                    if (await CheckIfGuestHasAccount(guest))
                    {
                        btnAddRecipe.Visibility = Visibility.Collapsed;
                    }
                    else
                    {
                        btnAddRecipe.Visibility = Visibility.Visible;
                    }
                }
            }
        }

        private async void btnCreateOrder_Click(object sender, RoutedEventArgs e)
        {
            if (cmbActiveBooking.SelectedItem is BookingDisplayItem selectedBooking)
            {
                var guestId = selectedBooking.GuestId;
                var guest = await _guestRepository.GetGuestByIdAsync(guestId);

                if (guest != null && _viewModel.CartItems.Any())
                {
                    await CreateOrder(selectedBooking.BookingId, guest);
                }
                else
                {
                    MessageBox.Show("Please select a booking and add items to the cart.", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
            else
            {
                MessageBox.Show("Please select a booking.", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
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
    }

}
