#nullable disable

using ESMART.Domain.Enum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ESMART.Application.Common.Dtos
{
    public class GuestAccountSummaryDto
    {
        public string GuestId { get; set; }
        public string AccountId { get; set; }

        public string GuestName { get; set; }
        public string Invoice { get; set; }
        public decimal Discount { get; set; }
        public decimal Tax { get; set; }
        public decimal OtherCharges { get; set; }
        public decimal Paid { get; set; }
        public decimal Refunds { get; set; }
        public decimal Balance { get; set; }

        public decimal FundedBalance { get; set; }
        public decimal TopUps { get; set; }
        public decimal DirectPayments { get; set; }
        public decimal OutstandingBalance { get; set; }
        public DateTime LastFunded { get; set; }
        public bool IsClosed { get; set; }

        public List<BookingSummaryDto> Bookings { get; set; }
        public List<TransactionSummaryDto> RecentTransactions { get; set; }
        public List<TransactionSummaryDto> ServiceConsumptions { get; set; }
        public List<TransactionSummaryDto> Payments { get; set; }
    }

    public class BookingSummaryDto
    {
        public string BookingId { get; set; }
        public string RoomNumber { get; set; }
        public string RoomType { get; set; }
        public DateTime CheckIn { get; set; }
        public DateTime CheckOut { get; set; }
        public string Status { get; set; }
    }

    public class TransactionSummaryDto
    {
        public string TransactionId { get; set; }
        public DateTime Date { get; set; }
        public string Description { get; set; }
        public string Invoice { get; set; }
        public decimal Discount { get; set; }
        public decimal BillPosts { get; set; }
        public decimal Payment { get; set; }
        public decimal Amount { get; set; }
        public TransactionType Type { get; set; }
    }
}
