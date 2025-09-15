#nullable disable

using ESMART.Domain.Entities.Laundry;
using ESMART.Domain.Entities.StoreKeeping;
using ESMART.Domain.ViewModels.StoreKepping;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ESMART.Domain.ViewModels.Laundry
{
    public class LaundryViewModel : INotifyPropertyChanged
    {
        public ObservableCollection<LaundryCategoryGroup> GroupedLaundryItems { get; set; } = new();
        public ObservableCollection<LaundryCategoryGroup> LaundaryItems { get; set; } = new();
        public ObservableCollection<LaundaryCategory> LaundryCategories { get; set; } = new();

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

        public LaundryViewModel()
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
            if (e.PropertyName == nameof(CartItem.Quantity) ||
                e.PropertyName == nameof(CartItem.TotalPrice) ||
                e.PropertyName == nameof(CartItem.IsPressingSelected))
            {
                CalculateTotalAmount();
            }
        }


        public void AddToCart(Domain.Entities.Laundry.Laundry item)
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
                    Name = item.Description,
                    LaundryPrice = item.LaundryPrice,
                    PressingPrice = item.PressingPrice,
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
            TotalAmount = CartItems.Sum(i => i.TotalPrice);
            Debug.WriteLine($"TotalAmount Updated: {TotalAmount}");
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string name) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }

    public class LaundryCategoryGroup : INotifyPropertyChanged
    {
        public LaundaryCategory CategoryName { get; set; }
        public List<Domain.Entities.Laundry.Laundry> Items { get; set; }

        private bool _isSelected;

        private bool _isPressing;
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
        public decimal LaundryPrice { get; set; }
        public decimal PressingPrice { get; set; }

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

        private bool _isPressingSelected;
        public bool IsPressingSelected
        {
            get => _isPressingSelected;
            set
            {
                if (_isPressingSelected != value)
                {
                    _isPressingSelected = value;
                    OnPropertyChanged(nameof(IsPressingSelected));

                    // Ensure price updates
                    OnPropertyChanged(nameof(TotalPrice));
                }
            }
        }



        public decimal TotalPrice => (LaundryPrice * Quantity) + (IsPressingSelected ? PressingPrice * Quantity : 0);

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }


}
