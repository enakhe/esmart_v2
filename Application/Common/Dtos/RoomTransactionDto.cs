#nullable disable

using ESMART.Domain.Entities.FrontDesk;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ESMART.Application.Common.Dtos
{
    public class RoomTransactionDto
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

        public List<GuestTransaction> RoomCharges { get; set; } = new();
        public List<GuestTransaction> OtherCharges { get; set; } = new();
    }
}
