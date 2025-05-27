#nullable disable

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ESMART.Domain.Entities.Transaction
{
    public class InvoiceStatement
    {
        public string Guest { get; set; }
        public string Invoice { get; set; }
        public decimal Amount { get; set; }
        public decimal Discount { get; set; }
        public decimal Tax { get; set; }
        public decimal OtherCharges { get; set; }
        public decimal Paid { get; set; }
        public decimal Refund { get; set; }
        public decimal Balance { get; set; }
    }

    public class TransactionItemInvoice
    {
        public DateTime Date { get; set; }
        public string Description { get; set; }
        public string Invoice { get; set; }
        public decimal Discount { get; set; }
        public decimal BillPost { get; set; }
        public decimal Amount { get; set; }
        public decimal Payment { get; set; }
        public string Category { get; set; }
    }

    public class DepositInvoice
    {
        public DateTime Date { get; set; }
        public string Description { get; set; }
        public string Invoice { get; set; }
        public decimal Discount { get; set; }
        public decimal BillPost { get; set; }
        public decimal Amount { get; set; }
        public decimal Payment { get; set; }
        public string Category { get; set; }

    }
}
