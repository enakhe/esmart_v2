using ESMART.Domain.Entities.RoomSettings;
using System.Collections.ObjectModel;

namespace ESMART.Presentation.Forms.Home
{
    public class IndexPageViewModel
    {
        public ObservableCollection<Room> Rooms { get; set; } = new();
    }

}
