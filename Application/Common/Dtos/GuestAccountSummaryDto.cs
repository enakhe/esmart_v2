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
        public decimal Amount { get; set; }
        public decimal ServiceCharge { get; set; }
        public decimal Discount { get; set; }
        public DateTime CheckIn { get; set; }
        public DateTime CheckOut { get; set; }
        public decimal VAT { get; set; }
        public decimal Tax { get; set; }
        public decimal OtherCharges { get; set; }
        public decimal Paid { get; set; }
        public decimal Refunds { get; set; }
        public decimal Balance { get; set; }
        public decimal FundedBalance { get; set; }
        public decimal OutstandingBalance { get; set; }

        public List<BookingTransactionGroupDto> BookingGroups { get; set; } = new();
        public List<TransactionSummaryDto> ServiceConsumptions { get; set; }
        public List<TransactionSummaryDto> Payments { get; set; }
        public List<TransactionSummaryDto> PayedRefunds { get; set; }

    }


    public class BookingAccountSummaryDto
    {
        public string Id { get; set; }
        public string Guest { get; set; }
        public string PhoneNumber { get; set; }
        public string Room { get; set; }
        public string RoomId { get; set; }
        public string BookingId { get; set; }
        public DateTime CheckIn { get; set; }
        public DateTime CheckOut { get; set; }
        public string PaymentMethod { get; set; }
        public string Duration { get; set; }
        public string Status { get; set; }
        public bool IsOverStayed { get; set; }
        public string TotalAmount { get; set; }
        public decimal Receivables { get; set; }
        public string CreatedBy { get; set; }
        public DateTime DateCreated { get; set; }
        public DateTime DateModified { get; set; }
        public int NumberOfRooms { get; set; }

        public List<BookingTransactionGroupDto> BookingGroups { get; set; } = new();
        public List<TransactionSummaryDto> ServiceConsumptions { get; set; }
        public List<TransactionSummaryDto> Payments { get; set; }
        public List<TransactionSummaryDto> PayedRefunds { get; set; }
    }


    public class BookingSummaryDto
    {
        public string Guest { get; set; }
        public string BookingId { get; set; }
        public string BookingBookingId { get; set; }
        public string RoomRoomId { get; set; }
        public string RoomId { get; set; }
        public string Invoice { get; set; }
        public string RoomNumber { get; set; }
        public string RoomType { get; set; }
        public DateTime CheckIn { get; set; }
        public DateTime CheckOut { get; set; }
        public string Status { get; set; }
    }

    public class TransactionSummaryDto
    {
        public string TransactionId { get; set; }
        public string RoomId { get; set; }
        public DateTime Date { get; set; }
        public string Description { get; set; }
        public string Consumer { get; set; }
        public string Invoice { get; set; }
        public decimal Discount { get; set; }
        public decimal BillPosts { get; set; }
        public decimal Payment { get; set; }
        public decimal Amount { get; set; }
        public TransactionType Type { get; set; }
    }

    public class BookingTransactionGroupDto
    {
        public string BookingId { get; set; }
        public string GuestName { get; set; }
        public string RoomId { get; set; }
        public string RoomNumber { get; set; }
        public string Summary { get; set; }
        public string RoomType { get; set; }
        public DateTime CheckIn { get; set; }
        public DateTime CheckOut { get; set; }
        public string Status { get; set; }

        public List<TransactionSummaryDto> RecentTransactions { get; set; } = new();
    }
}
