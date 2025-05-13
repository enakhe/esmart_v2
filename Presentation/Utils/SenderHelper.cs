#nullable disable

using ESMART.Domain.Entities.Configuration;
using ESMART.Domain.Entities.FrontDesk;
using System.Net.Http;
using System.Text.Json;

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

        public static async Task<HttpResponseMessage> SendEmail(string to, string subject, string templateName, ReceiptVariable variables)
        {
            var apiService = new ApiService(new HttpClient());
            var response = await apiService.PostAsync("http://localhost:8000/api/email", new
            {
                to,
                subject,
                templateName,
                variables
            });

            return response;
        }
    }

    public class ReceiptVariable
    {
        public string accountNumber { get; set; }
        public string amount { get; set; }
        public string guestName { get; set; }
        public string hotelName { get; set; }
        public string invoiceNumber { get; set; }
        public string paymentMethod { get; set; }
        public string receptionist { get; set; }
        public string receptionistContact { get; set; }
        public string service { get; set; }
        public string logo { get; set; }
    }
}
