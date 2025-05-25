#nullable disable

using ESMART.Domain.Entities.FrontDesk;
using ESMART.Domain.Entities.Transaction;

namespace ESMART.Domain.ViewModels.Transaction
{
    public class TransactionViewModel
    {
        public string TransactionId { get; set; }
        public string Guest { get; set; }
        public string GuestPhoneNo { get; set; }
        public DateTime Date { get; set; }
        public decimal Amount { get; set; }
        public decimal Discount { get; set; }
        public decimal Tax { get; set; }
        public decimal Charge { get; set; }
        public decimal Balance { get; set; }
        public decimal Paid { get; set; }
        public decimal Refund { get; set; }
        public string Invoice { get; set; }
        public string Description { get; set; }
        public string IssuedBy { get; set; }
        public DateTime DateCreated { get; set; }
        public DateTime DateUpdated { get; set; }
        public List<TransactionItemViewModel> TransactionItems { get; set; }
        public Booking Booking { get; set; }
    }
}
