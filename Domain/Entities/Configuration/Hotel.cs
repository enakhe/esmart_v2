namespace ESMART.Domain.Entities.Configuration
{
    public class Hotel
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public required string Name { get; set; }
        public required string Address { get; set; }
        public required string PhoneNumber { get; set; }
        public required string Email { get; set; }
        public byte[]? LogoUrl { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
