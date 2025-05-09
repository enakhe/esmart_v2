using ESMART.Domain.Entities.RoomSettings;
using System.Collections.ObjectModel;

namespace ESMART.Presentation.Forms.Home
{
    public class IndexPageViewModel
    {
        public ObservableCollection<Domain.Entities.RoomSettings.Room> Rooms { get; set; } = new();
    }

}
