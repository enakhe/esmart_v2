using ESMART.Application.Common.Utils;
using ESMART.Domain.Entities.RoomSettings;
using ESMART.Domain.ViewModels.RoomSetting;
using System.Collections.ObjectModel;
using System.ComponentModel;

namespace ESMART.Presentation.Forms.Home
{
    public class IndexPageViewModel
    {
        public ObservableCollection<SelectableRoomViewModel> Rooms { get; set; } = new();
        public ObservableCollection<SelectableRoomViewModel> SelectedRooms { get; set; } = new();

        private decimal _totalAmount;
        public decimal TotalAmount
        {
            get => _totalAmount;
            set
            {
                _totalAmount = value;
                OnPropertyChanged(nameof(TotalAmount));
            }
        }

        public void CalculateTotalAmount(DateTime checkIn, DateTime checkOut, decimal discount, decimal vat, decimal serviceCharge)
        {
            decimal total = 0;
            foreach (var room in SelectedRooms)
            {
                var price = Helper.GetPriceByRateAndTime(checkIn, checkOut, room.Room.Rate);
                total += Helper.CalculateTotal(price, discount, vat, serviceCharge);
            }

            TotalAmount = total;
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string name) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }

}
