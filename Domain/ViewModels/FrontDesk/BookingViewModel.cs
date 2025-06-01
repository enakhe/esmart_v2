#nullable disable

using ESMART.Domain.Entities.FrontDesk;

namespace ESMART.Domain.ViewModels.FrontDesk
{
    public class BookingViewModel
    {
        public string Id { get; set; }
        public string Guest { get; set; }
        public string PhoneNumber { get; set; }
        public string Room { get; set; }
        public string RoomId { get; set; }
        public string BookingId { get; set; }
        public DateTime CheckIn { get; set; }
        public DateTime CheckOut { get; set; }
        public string PaymentMethod { get; set; }
        public string Duration { get; set; }
        public string Status { get; set; }
        public bool IsOverStayed { get; set; }
        public string TotalAmount { get; set; }
        public decimal Receivables { get; set; }
        public string CreatedBy { get; set; }
        public DateTime DateCreated { get; set; }
        public DateTime DateModified { get; set; }
        public int NumberOfRooms { get; set; }
        public ICollection<RoomBooking> RoomBookings { get; set; }
    }
}
