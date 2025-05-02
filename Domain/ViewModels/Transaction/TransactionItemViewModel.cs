#nullable disable

using ESMART.Domain.Enum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ESMART.Domain.ViewModels.Transaction
{
    public class TransactionItemViewModel
    {
        public string ServiceId { get; set; }
        public decimal Amount { get; set; }
        public decimal TaxAmount { get; set; }
        public decimal ServiceCharge { get; set; }
        public decimal Discount { get; set; }
        public string Category { get; set; }
        public string Type { get; set; }
        public TransactionStatus Status { get; set; }
        public string BankAccount { get; set; }
        public DateTime DateAdded { get; set; }
        public string IssuedBy { get; set; }
    }
}
