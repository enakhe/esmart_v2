using ESMART.Domain.Entities.Configuration;
using ESMART.Domain.Entities.FrontDesk;
using System.Net.Http;

namespace ESMART.Presentation.Utils
{
    public static class SenderHelper
    {
        public static async Task<HttpResponseMessage> SendOtp(string phoneNumber, string accountNumber, string guest, string service, string otp, decimal amount)
        {
            var apiService = new ApiService(new HttpClient());
            var response = await apiService.PostAsync("https://esmart-api.vercel.app/api/otp", new
            {
                to = phoneNumber,
                otp,
                guest,
                service,
                amount = amount.ToString("N2"),
                account = accountNumber
            });

            return response;
        }
    }
}
