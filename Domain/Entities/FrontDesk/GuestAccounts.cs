#nullable disable

using ESMART.Domain.Entities.RoomSettings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ESMART.Domain.Entities.FrontDesk
{
    public class GuestAccounts
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string GuestId { get; set; }
        public decimal FundedBalance { get; set; } = 0;     // Always zero
        public decimal TotalCharges { get; set; } = 0;      // All costs tracked
        public decimal TopUps { get; set; } = 0;            // None
        public decimal DirectPayments { get; set; } = 0;    // Paid at checkout
        public decimal TotalPayments => TopUps + DirectPayments;
        public decimal OutstandingBalance => TotalCharges - TotalPayments;
        public bool IsClosed { get; set; } = false;
        public DateTime LastFunded { get; set; }

        public virtual Guest Guest { get; set; }
    }
}
