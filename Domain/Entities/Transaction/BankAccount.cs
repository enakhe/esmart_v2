#nullable disable

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ESMART.Domain.Entities.Transaction
{
    public class BankAccount
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string BankName { get; set; }
        public string BankAccountNumber { get; set; }
        public string BankAccountName { get; set; }
        public DateTime DateCreated { get; set; } = DateTime.Now;
    }
}
