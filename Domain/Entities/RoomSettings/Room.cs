using ESMART.Domain.Entities.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ESMART.Domain.Entities.RoomSettings
{
    public class Room
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string? Number { get; set; }
        public RoomStatus? Status { get; set; }
        public string? CreatedBy { get; set; }
        public string? UpdatedBy { get; set; }
        public bool IsTrashed { get; set; } = false;
        public DateTime DateCreated { get; set; } = DateTime.Now;
        public DateTime DateModified { get; set; } = DateTime.Now;

        public string? BuildingId { get; set; }
        public string? RoomTypeId { get; set; }
        public string? AreaId { get; set; }

        public virtual ApplicationUser? ApplicationUser { get; set; }
    }

    public enum RoomStatus
    {
        Vacant,
        Booked,
        Reserved,
        Dirty,
        Maintenance,
        OutOfOrder
    }
}
