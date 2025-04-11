using ESMART.Domain.Entities.RoomSettings;

namespace ESMART.Domain.ViewModels.RoomSetting
{
    public class RoomViewModel
    {
        public string? Id { get; set; }
        public string? Number { get; set; }
        public string? RoomType { get; set; }
        public string? Floor { get; set; }
        public string? Building { get; set; }
        public string? Area { get; set; }
        public string? Rate { get; set; }
        public RoomStatus Status { get; set; }
        public string? CreatedBy { get; set; }
        public DateTime? DateCreated { get; set; }
        public DateTime? DateModified { get; set; }
    }
}
