using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ESMART.Domain.Entities.Transaction
{
    public class TransactionItem
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string? Description { get; set; }
        public decimal? Amount { get; set; }
        public string? Category { get; set; }

        public string? TransactionId { get; set; }
        public virtual Transaction? Transaction { get; set; }
    }
}
