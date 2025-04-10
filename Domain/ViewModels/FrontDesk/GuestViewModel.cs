namespace ESMART.Domain.ViewModels.FrontDesk
{
    public class GuestViewModel
    {
        public string? Id { get; set; }
        public string? GuestId { get; set; }
        public string? FullName { get; set; }
        public byte[]? GuestImage { get; set; }
        public string? Email { get; set; }
        public string? PhoneNumber { get; set; }
        public string? Gender { get; set; }
        public string? Street { get; set; }
        public string? City { get; set; }
        public string? Status { get; set; }
        public string? State { get; set; }
        public string? Country { get; set; }
        public string? CreatedBy { get; set; }
        public string? UpdatedBy { get; set; }
        public string? DateCreated { get; set; }
        public string? DateModified { get; set; }
    }
}
