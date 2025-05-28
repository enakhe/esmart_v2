#nullable disable

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ESMART.Domain.ViewModels.FrontDesk
{
    public class RoomOccupantViewModel : INotifyPropertyChanged
    {
        private string _occupantName;
        public string OccupantName
        {
            get => _occupantName;
            set { _occupantName = value; OnPropertyChanged(nameof(OccupantName)); }
        }

        private string _phoneNumber;
        public string PhoneNumber
        {
            get => _phoneNumber;
            set { _phoneNumber = value; OnPropertyChanged(nameof(PhoneNumber)); }
        }

        private DateTime _checkoutTime;
        public DateTime CheckoutTime
        {
            get => _checkoutTime;
            set
            {
                _checkoutTime = value;
                OnPropertyChanged(nameof(CheckoutTime));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string name) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }

}
