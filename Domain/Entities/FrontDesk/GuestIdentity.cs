namespace ESMART.Domain.Entities.FrontDesk
{
    public class GuestIdentity
    {
        public string? Id { get; set; } = Guid.NewGuid().ToString();
        public string? IdNumber { get; set; }
        public string? IdType { get; set; }
        public byte[]? IdentificationDocumentFront { get; set; }
        public byte[]? IdentificationDocumentBack { get; set; }

        public string? GuestId { get; set; }
        public virtual Guest? Guest { get; set; }
    }
}
