using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ESMART.Domain.Entities.RoomSettings
{
    public class RoomType
    {
        public RoomType()
        {
            this.Rooms = new HashSet<Room>();
        }
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string? Name { get; set; }
        public decimal? Rate { get; set; }
        public bool IsTrashed { get; set; } = false;
        public DateTime DateCreated { get; set; } = DateTime.Now;
        public DateTime DateModified { get; set; } = DateTime.Now;

        public virtual ICollection<Room> Rooms { get; set; }
    }
}
