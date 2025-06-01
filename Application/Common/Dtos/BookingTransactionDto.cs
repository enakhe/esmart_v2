#nullable disable

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ESMART.Application.Common.Dtos
{
    public class BookingTransactionDto
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
        public List<TransactionSummaryDto> ServiceCharges { get; set; } = new();

    }
}
