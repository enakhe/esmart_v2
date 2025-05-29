using ESMART.Domain.Entities.RoomSettings;
using ESMART.Domain.ViewModels.FrontDesk;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace ESMART.Domain.ViewModels.RoomSetting
{
    public class SelectableRoomViewModel : INotifyPropertyChanged
    {
        public SelectableRoomViewModel(Room room)
        {
            Room = room;
            Rack = room?.Rate ?? 0;
            Discount = 0;
            Tax = 0;
        }

        public Room Room { get; }

        private bool _isSelected;
        public bool IsSelected
        {
            get => _isSelected;
            set { _isSelected = value; OnPropertyChanged(nameof(IsSelected)); }
        }

        private DateTime? _checkoutTime;


        public DateTime? CheckoutTime
        {
            get => _checkoutTime;
            set
            {
                _checkoutTime = value;
                OnPropertyChanged(nameof(CheckoutTime));
            }
        }

        private decimal _rack;
        public decimal Rack
        {
            get => _rack;
            set
            {
                _rack = value;
                OnPropertyChanged(nameof(Rack));
            }
        }

        private decimal _discount;
        public decimal Discount
        {
            get => _rack;
            set
            {
                _rack = value;
                OnPropertyChanged(nameof(Discount));
            }
        }

        private decimal _tax;
        public decimal Tax
        {
            get => _rack;
            set
            {
                _rack = value;
                OnPropertyChanged(nameof(Tax));
            }
        }

        private decimal _serviceCharge;
        public decimal ServceCharge
        {
            get => _rack;
            set
            {
                _rack = value;
                OnPropertyChanged(nameof(ServceCharge));
            }
        }

        private RoomOccupantViewModel _occupant = new();
        public RoomOccupantViewModel Occupant
        {
            get => _occupant;
            set
            {
                _occupant = value;
                OnPropertyChanged(nameof(Occupant));
            }
        }

        public decimal FinalRate { get; set; }
        public decimal RackRate { get; set; }
        public decimal DiscountRate { get; set; }
        public decimal TaxRate { get; set; }
        public decimal ServiceChargeRate { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string name) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
