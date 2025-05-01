namespace ESMART.Domain.Entities.Configuration
{
    public class SettingsCategory
    {
        public SettingsCategory()
        {
            this.HotelSetting = new HashSet<HotelSetting>();
        }

        public string Id { get; set; } = Guid.NewGuid().ToString();
        public required string Name { get; set; }

        public ICollection<HotelSetting> HotelSetting { get; set; }
    }
}
