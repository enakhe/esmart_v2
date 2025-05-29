#nullable disable

using ESMART.Domain.Entities.RoomSettings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ESMART.Domain.Entities.FrontDesk
{
    public class GuestAccount
    {
        public GuestAccount()
        {
            this.Transactions = new HashSet<GuestTransaction>();
            this.BookingDetails = new HashSet<BookingDetail>();
        }

        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string GuestId { get; set; }
        public string Invoice { get; set; }
        public decimal Amount { get; set; }
        public decimal Discount { get; set; }
        public decimal Tax { get; set; }
        public decimal ServiceCharge { get; set; } 
        public decimal OtherCharges { get; set; }
        public decimal FundedBalance { get; set; }
        public decimal Paid => TopUps + DirectPayments;
        public decimal TotalConsumptions => Amount + Tax + ServiceCharge + OtherCharges;
        public decimal Refunds => Math.Max(0, Paid - TotalConsumptions);
        public decimal Balance => Paid - (Amount + Tax + ServiceCharge + OtherCharges);
        public decimal TopUps { get; set; }
        public decimal DirectPayments { get; set; }
        public decimal OutstandingBalance => OtherCharges - Paid;
        public DateTime LastFunded { get; set; }

        public bool IsClosed { get; set; } = false;
        public bool AllowBarAndRes { get; set; } = true;
        public bool AllowLaundry { get; set; } = true;

        public virtual Guest Guest { get; set; }

        public ICollection<GuestTransaction> Transactions { get; set; }
        public ICollection<BookingDetail> BookingDetails { get; set; }
    }
}
