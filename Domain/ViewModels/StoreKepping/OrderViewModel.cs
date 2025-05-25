#nullable disable

using ESMART.Domain.Entities.StoreKeeping;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;

namespace ESMART.Domain.ViewModels.StoreKepping
{
    public class OrderViewModel : INotifyPropertyChanged
    {
        public ObservableCollection<MenuCategoryGroup> GroupedMenuItems { get; set; } = new();
        public ObservableCollection<MenuItemViewModel> MenuItems { get; set; } = new();
        public ObservableCollection<MenuCategory> MenuCategories { get; set; } = new();
        public ObservableCollection<CartItem> CartItems { get; set; } = new();

        private decimal _totalAmount;
        public decimal TotalAmount
        {
            get => _totalAmount;
            set
            {
                if (_totalAmount != value)
                {
                    _totalAmount = value;
                    OnPropertyChanged(nameof(TotalAmount));
                }
            }
        }

        public OrderViewModel()
        {
            CartItems.CollectionChanged += CartItems_CollectionChanged;
        }

        private void CartItems_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.NewItems != null)
            {
                foreach (CartItem item in e.NewItems)
                {
                    item.PropertyChanged += CartItem_PropertyChanged;
                }
            }

            if (e.OldItems != null)
            {
                foreach (CartItem item in e.OldItems)
                {
                    item.PropertyChanged -= CartItem_PropertyChanged;
                }
            }

            CalculateTotalAmount();
        }

        private void CartItem_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(CartItem.Quantity) || e.PropertyName == nameof(CartItem.TotalPrice))
            {
                CalculateTotalAmount();
            }
        }

        public void AddToCart(MenuItemViewModel item)
        {
            var existing = CartItems.FirstOrDefault(i => i.Id == item.Id);
            if (existing != null)
            {
                existing.Quantity++;
            }
            else
            {
                var cartItem = new CartItem
                {
                    Id = item.Id,
                    Name = item.Name,
                    Price = item.Price,
                    Quantity = 1
                };

                CartItems.Add(cartItem);
            }

            // No need to call CalculateTotalAmount here — it's triggered by PropertyChanged
        }

        public void RemoveFromCart(string itemId)
        {
            var itemToRemove = CartItems.FirstOrDefault(i => i.Id == itemId);
            if (itemToRemove != null)
            {
                CartItems.Remove(itemToRemove);
                // `CollectionChanged` will trigger total amount recalculation
            }
        }

        public void DecreaseOrRemoveFromCart(string itemId)
        {
            var item = CartItems.FirstOrDefault(i => i.Id == itemId);
            if (item != null)
            {
                if (item.Quantity > 1)
                {
                    item.Quantity--; // triggers PropertyChanged + total update
                }
                else
                {
                    CartItems.Remove(item); // triggers CollectionChanged + total update
                }
            }
        }

        public void CalculateTotalAmount()
        {
            foreach (var i in CartItems)
            {
                Debug.WriteLine($"Item: {i.Name}, Qty: {i.Quantity}, Price: {i.Price}, Total: {i.TotalPrice}");
            }

            TotalAmount = CartItems.Sum(i => i.TotalPrice);
            Debug.WriteLine($"TotalAmount: {TotalAmount}");
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string name) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }

    public class MenuCategoryGroup : INotifyPropertyChanged
    {
        public string CategoryName { get; set; }
        public byte[] Image { get; set; }
        public List<MenuItemViewModel> Items { get; set; }

        private bool _isSelected;
        public bool IsSelected
        {
            get => _isSelected;
            set
            {
                _isSelected = value;
                OnPropertyChanged(nameof(IsSelected));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string name) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }

    public class CartItem : INotifyPropertyChanged
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public decimal Price { get; set; }

        private int _quantity;
        public int Quantity
        {
            get => _quantity;
            set
            {
                if (_quantity != value)
                {
                    _quantity = value;
                    OnPropertyChanged(nameof(Quantity));
                    OnPropertyChanged(nameof(TotalPrice));
                }
            }
        }

        public decimal TotalPrice => Price * Quantity;

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
