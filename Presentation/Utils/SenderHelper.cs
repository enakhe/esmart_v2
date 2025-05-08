using ESMART.Domain.Entities.Configuration;
using ESMART.Domain.Entities.FrontDesk;
using System.Net.Http;

namespace ESMART.Presentation.Utils
{
    public static class SenderHelper
    {
        public static async Task<HttpResponseMessage> SendOtp(string to, string hotel, string account, string guest, string service, string otp, decimal amount, string paymentMethod, string receptionist, string contact)
        {
            var apiService = new ApiService(new HttpClient());
            var response = await apiService.PostAsync("https://esmart-api.vercel.app/api/otp", new
            {
                to,
                otp,
                guest,
                service,
                amount = amount.ToString("N2"),
                account,
                hotel,
                paymentMethod,
                receptionist,
                contact
            });

            return response;
        }

        public static async Task<HttpResponseMessage> SendRefundOtp(string to, string hotel, string guest, string service, string otp, decimal amount, string paymentMethod, string receptionist, string contact)
        {
            var apiService = new ApiService(new HttpClient());
            var response = await apiService.PostAsync("https://esmart-api.vercel.app/api/refund", new
            {
                to,
                refundReference = otp,
                guest,
                service,
                amount = amount.ToString("N2"),
                hotel,
                paymentMethod,
                receptionist,
                contact
            });
            return response;
        }
    }
}
