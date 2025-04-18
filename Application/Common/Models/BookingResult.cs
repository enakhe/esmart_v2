using ESMART.Domain.Entities.FrontDesk;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ESMART.Application.Common.Models
{
    public class BookingResult
    {
        internal BookingResult(bool succeeded, IEnumerable<string> errors, Booking? data)
        {
            Succeeded = succeeded;
            Errors = [.. errors];
            Response = data;
        }

        public bool Succeeded { get; init; }
        public string[] Errors { get; init; }
        public Booking? Response { get; set; }

        public static BookingResult Success(Booking data)
        {
            return new BookingResult(true, [], data);
        }

        public static BookingResult Failure(IEnumerable<string> errors)
        {
            return new BookingResult(false, errors, null);
        }
    }
}
