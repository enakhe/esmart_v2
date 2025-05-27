#nullable disable

using ESMART.Domain.Entities.FrontDesk;
using ESMART.Domain.Entities.Transaction;
using ESMART.Domain.ViewModels.StoreKepping;
using System.ComponentModel;

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
        public decimal OtherCharges { get; set; }
        public decimal Balance { get; set; }
        public decimal Paid { get; set; }
        public decimal Refund { get; set; }
        public string Invoice { get; set; }
        public string Description { get; set; }
        public string IssuedBy { get; set; }
        public DateTime DateCreated { get; set; }
        public DateTime DateUpdated { get; set; }
        public Dictionary<string, List<TransactionItemViewModel>> GroupedTransactionItems { get; set; }
        public List<TransactionItemViewModel> FlatTransactionItems { get; set; }

        public Booking Booking { get; set; }
    }

    public class TransactionItemCategoryGroup
    {
        public string Title { get; set; }
        public List<TransactionItemViewModel> TransactionItems { get; set; }
    }
}
