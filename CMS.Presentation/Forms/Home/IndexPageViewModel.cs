using ESMART.Application.Interface;
using ESMART.Domain.ViewModels.RoomSetting;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ESMART.Presentation.Forms.Home
{
    public class IndexPageViewModel
    {
        public ObservableCollection<RoomViewModel> Rooms { get; set; } = new();
    }

}
