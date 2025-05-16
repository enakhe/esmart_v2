#nullable disable

namespace ESMART.Domain.ViewModels.FrontDesk
{
    public class ReservationViewModel
    {
        public string Id { get; set; }
        public string Guest { get; set; }
        public string PhoneNumber { get; set; }
        public string Room { get; set; }
        public DateTime ArrivalDate { get; set; }
        public DateTime DepartureDate { get; set; }
        public string PaymentMethod { get; set; }
        public string Duration { get; set; }
        public string Status { get; set; }
        public string PaymentStatus { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal AmountPaid { get; set; }
        public decimal Receivables { get; set; }
        public string CreatedBy { get; set; }
        public DateTime DateCreated { get; set; }
        public DateTime DateModified { get; set; }
    }
}
