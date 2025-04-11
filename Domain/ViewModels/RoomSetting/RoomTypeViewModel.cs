using System;

namespace ESMART.Domain.ViewModels.RoomSetting
{
    public class RoomTypeViewModel
    {
        public string? Id { get; set; }
        public string? Name { get; set; }
        public decimal? Rate { get; set; }
        public DateTime DateCreated { get; set; }
        public DateTime DateModified { get; set; }
    }
}
