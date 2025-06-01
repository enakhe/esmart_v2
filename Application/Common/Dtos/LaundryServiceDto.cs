#nullable disable

using ESMART.Domain.Entities.Laundry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ESMART.Application.Common.Dtos
{
    public class LaundryServiceDto
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string ItemId { get; set; }
        public LaundaryCategory Category { get; set; }
        public string Description { get; set; }
        public decimal LaundryPrice { get; set; }
        public decimal PressingPrice { get; set; }
    }
}
