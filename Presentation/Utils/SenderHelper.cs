using ESMART.Domain.Entities.Configuration;
using ESMART.Domain.Entities.FrontDesk;
using System.Net.Http;

namespace ESMART.Presentation.Utils
{
    public static class SenderHelper
    {
        public static async Task<HttpResponseMessage> SendOtp(Hotel hotel, string accountNumber, Guest guest, string service, string otp, decimal amount)
        {
            var apiService = new ApiService(new HttpClient());
            var response = await apiService.PostAsync("https://esmart-api.vercel.app/api/otp", new
            {
                to = hotel.PhoneNumber,
                otp,
                guest = guest.FullName,
                service,
                amount = amount.ToString("N2"),
                account = accountNumber
            });

            return response;
        }
    }
}
