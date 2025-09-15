#nullable disable

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ESMART.Application.Common.Dtos
{
    public class HouseListDto
    {
        public ICollection<InHouseGuest> InHouseGuests { get; set; }

        public decimal RevenueToday { get; set; }

        public decimal RevenueThisMonth { get; set; }

        public decimal RevenueThisYear { get; set; }


        public int CurrentGuestsInHouse { get; set; }

        public int BookingsThisMonth { get; set; }

        public int BookingsThisYear { get; set; }

    }

    public class InHouseGuest
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        public int SerialNumber { get; set; }
        public string GuestId { get; set; }
        public byte[] ProfilePicture { get; set; }

        public string GuestName { get; set; }

        public string PhoneNumber { get; set; }
        public string Email { get; set; }

        public string RoomNumber { get; set; }
        public string RoomId { get; set; }
        public int Duration => (CheckOutDate - CheckInDate).Days;

        public decimal Rate { get; set; }
        public decimal Tax { get; set; }
        public decimal Discount { get; set; }
        public decimal ServiceCharge { get; set; }

        public decimal AmountPaid { get; set; }

        public string CreatedBy { get; set; }

        public DateTime CheckInDate { get; set; }

        public DateTime CheckOutDate { get; set; }
    }
}
