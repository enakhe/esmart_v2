#nullable disable

using ESMART.Domain.Entities.Data;
using ESMART.Domain.Enum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ESMART.Domain.Entities.FrontDesk
{
    public class GuestTransaction
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string GuestId { get; set; }
        public string TransactionId { get; set; }
        public string ApplicationUserId { get; set; }
        public decimal Amount { get; set; }
        public string Description { get; set; }
        public TransactionType TransactionType { get; set; }
        public DateTime Date { get; set; }

        public Guest Guest { get; set; }
        public ApplicationUser ApplicationUser { get; set; }
    }
}
