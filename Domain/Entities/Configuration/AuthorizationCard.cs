#nullable disable

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ESMART.Domain.Entities.Configuration
{
    public class AuthorizationCard
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string ComputerName { get; set; }
        public string AuthId { get; set; }
        public DateTime DateAdded { get; set; }
        public DateTime DateUpdated { get; set; }
    }
}
