#nullable disable

using ESMART.Domain.Entities.FrontDesk;
using ESMART.Domain.Enum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ESMART.Domain.ViewModels.FrontDesk
{
    public class GuestTransactionViewModel
    {
        public string Id { get; set; }
        public decimal Amount { get; set; }
        public string Description { get; set; }
        public DateTime Date { get; set; }
        public TransactionType TransactionType { get; set; }
    }
}
