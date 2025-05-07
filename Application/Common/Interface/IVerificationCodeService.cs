using ESMART.Domain.Entities.Verification;

namespace ESMART.Application.Common.Interface
{
    public interface IVerificationCodeService
    {
        Task AddCode(VerificationCode verificationCode);
        Task<VerificationCode> GetCodeByIDAsync(string id);
        Task DeleteAsync(string id);
        Task<bool> VerifyCodeAsync(string serviceId, string code);
        Task<VerificationCode> GetCodeByServiceId(string id);
        Task<VerificationCode> GetCodeByCode(string code);
    }
}
