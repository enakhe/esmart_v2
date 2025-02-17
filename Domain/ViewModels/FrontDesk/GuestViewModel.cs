using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ESMART.Domain.ViewModels.FrontDesk
{
    public class GuestViewModel
    {
        public string? Id { get; set; }
        public string? GuestId { get; set; }
        public string? FullName { get; set; }
        public byte[]? GuestImage { get; set; }
        public string? Email { get; set; }
        public string? PhoneNumber { get; set; }
        public string? City { get; set; }
        public string? Status { get; set; }
        public string? State { get; set; }
        public string? Country { get; set; }
        public string? CreatedBy { get; set; }
        public DateTime DateCreated { get; set; }
        public DateTime DateModified { get; set; }
    }
}
