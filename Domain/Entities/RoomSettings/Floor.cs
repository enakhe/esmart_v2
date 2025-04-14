namespace ESMART.Domain.Entities.RoomSettings
{
    public class Floor
    {
        public Floor()
        {
            Rooms = new HashSet<Room>();
        }
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string? Name { get; set; }
        public string? Number { get; set; }
        public bool IsTrashed { get; set; } = false;
        public DateTime DateCreated { get; set; } = DateTime.Now;
        public DateTime DateModified { get; set; } = DateTime.Now;

        public string? BuildingId { get; set; }
        public virtual Building? Building { get; set; }

        public ICollection<Room> Rooms { get; set; }
    }
}
