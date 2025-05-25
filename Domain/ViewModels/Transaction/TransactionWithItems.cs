using ESMART.Domain.Entities.Transaction;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ESMART.Domain.ViewModels.Transaction
{
    public class TransactionWithItems
    {
        public Domain.Entities.Transaction.Transaction Transaction { get; set; }
        public List<TransactionItem> TransactionItems { get; set; }
    }

}
