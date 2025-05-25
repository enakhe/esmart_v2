#nullable disable

using ESMART.Domain.Enum;

namespace ESMART.Domain.ViewModels.Transaction
{
    public class TransactionItemViewModel
    {
        public string Id { get; set; }
        public string ServiceId { get; set; }
        public string ReferenceNo { get; set; }
        public string Amount { get; set; }
        public decimal Tax { get; set; }
        public decimal Service { get; set; }
        public decimal Discount { get; set; }
        public string Description { get; set; }
        public decimal BillPost { get; set; }
        public string Category { get; set; }
        public string Type { get; set; }
        public TransactionStatus Status { get; set; }
        public string Account { get; set; }
        public DateTime Date { get; set; }
        public string IssuedBy { get; set; }
    }
}
