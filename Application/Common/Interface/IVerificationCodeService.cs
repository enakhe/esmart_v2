using ESMART.Domain.Entities.Verification;

namespace ESMART.Application.Common.Interface
{
    public interface IVerificationCodeService
    {
        Task AddCode(VerificationCode verificationCode);
        Task<VerificationCode> GetCodeByIDAsync(string id);
        Task DeleteAsync(string id);
        Task<bool> VerifyCodeAsync(string bookingId, string code);
        Task<VerificationCode> GetCodeByBookingId(string id);
        Task<VerificationCode> GetCodeByCode(string code);
    }
}
