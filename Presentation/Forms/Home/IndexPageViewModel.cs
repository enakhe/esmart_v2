#nullable disable

using ESMART.Application.Common.Utils;
using ESMART.Domain.Entities.FrontDesk;
using ESMART.Domain.Entities.RoomSettings;
using ESMART.Domain.ViewModels.FrontDesk;
using ESMART.Domain.ViewModels.RoomSetting;
using System.Collections.ObjectModel;
using System.ComponentModel;

namespace ESMART.Presentation.Forms.Home
{
    public class IndexPageViewModel
    {
        public ObservableCollection<SelectableRoomViewModel> Rooms { get; set; } = new();
        public ObservableCollection<SelectableRoomViewModel> SelectedRooms { get; set; } = new();
        public ObservableCollection<Guest> GuestList { get; set; }
        public ObservableCollection<RoomOccupantViewModel> RoomOccupants { get; set; } = new();
        public string MainGuestName { get; set; }
        public string MainGuestPhoneNumber { get; set; }

        private Guest _selectedGuest;

        private bool _copyMainGuestToAll;
        public Guest SelectedGuest
        {
            get => _selectedGuest;
            set
            {
                _selectedGuest = value;
                OnPropertyChanged(nameof(SelectedGuest));

                MainGuestName = value?.FullName;
                MainGuestPhoneNumber = value?.PhoneNumber;

                if (CopyMainGuestToAll)
                    CopyMainGuestToOccupants();
            }
        }

        public bool CopyMainGuestToAll
        {
            get => _copyMainGuestToAll;
            set
            {
                _copyMainGuestToAll = value;
                OnPropertyChanged(nameof(CopyMainGuestToAll));
                if (value)
                    CopyMainGuestToOccupants();
            }
        }


        private void CopyMainGuestToOccupants()
        {
            foreach (var room in SelectedRooms)
            {
                if (room.Occupant == null)
                    room.Occupant = new RoomOccupantViewModel();

                room.Occupant.OccupantName = MainGuestName;
                room.Occupant.PhoneNumber = MainGuestPhoneNumber;

                RoomOccupants.Add(room.Occupant);
            }
        }


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
            foreach (var room in SelectedRooms)
            {
                decimal rate = Helper.GetPriceByRateAndTime(checkIn, checkOut, room.Room.Rate);
                var (rack, discountPrce, serviceFeeAmount, tax, final) = Helper.CalculateRackAndDiscountedTotal(rate, vat, serviceCharge, discount);
                room.RackRate = rack;
                room.FinalRate = final;
                room.DiscountRate = discountPrce;
                room.TaxRate = tax;
                room.ServiceChargeRate = serviceFeeAmount;
            }

            TotalAmount = SelectedRooms.Sum(r => r.FinalRate);
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string name) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }

}
