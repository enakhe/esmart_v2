namespace ESMART.Domain.Entities.Configuration
{
    public class HotelSetting
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public required string Key { get; set; }
        public required string Value { get; set; }
        public required string DataType { get; set; }
        public required string Description { get; set; }
        public bool IsActive { get; set; }
        public DateTime UpdatedAt { get; set; }
        public required string UpdatedBy { get; set; }

        public string? CategoryId { get; set; }
        public virtual SettingsCategory? Category { get; set; }
    }

}
