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
        public decimal FundedBalance { get; set; }
        public decimal TotalCharges { get; set; }
        public decimal TopUps { get; set; }
        public decimal DirectPayments { get; set; }
        public decimal TotalPayments => TopUps + DirectPayments;
        public decimal OutstandingBalance => TotalCharges - TotalPayments;
        public bool IsClosed { get; set; } = false;
        public DateTime LastFunded { get; set; }

        public bool AllowBarAndRes { get; set; } = true;
        public bool AllowLaundry { get; set; } = true;

        public virtual Guest Guest { get; set; }
    }
}
