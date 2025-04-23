using ESMART.Domain.Entities.Configuration;
using ESMART.Domain.Entities.FrontDesk;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace ESMART.Presentation.Utils
{
    public static class SenderHelper
    {
        public static async Task<HttpResponseMessage> SendOtp(Hotel hotel, Booking booking, Guest guest, string service, string otp)
        {
            var apiService = new ApiService(new HttpClient());
            var response = await apiService.PostAsync("http://localhost:8000/api/send-otp", new
            {
                to = hotel.PhoneNumber,
                otp = otp,
                guest = guest.FullName,
                service,
                amount = booking.TotalAmount.ToString("N2"),
                account = booking.AccountNumber
            });

            return response;
        }
    }
}
