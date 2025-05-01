using ESMART.Application.Common.Interface;
using ESMART.Domain.Entities.Verification;
using ESMART.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace ESMART.Infrastructure.Repositories.Verification
{
    public class VerificationCodeService(IDbContextFactory<ApplicationDbContext> contextFactory) : IVerificationCodeService
    {
        private readonly IDbContextFactory<ApplicationDbContext> _contextFactory = contextFactory;

        public async Task AddCode(VerificationCode verificationCode)
        {
            try
            {
                using var context = _contextFactory.CreateDbContext();
                await context.VerificationCodes.AddAsync(verificationCode);
                await context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                throw new Exception("An error occurred when saving code. " + ex.Message);
            }
        }

        public async Task<VerificationCode> GetCodeByIDAsync(string id)
        {
            try
            {
                using var context = _contextFactory.CreateDbContext();
                VerificationCode? verificationCode = await context.VerificationCodes.FirstOrDefaultAsync(v => v.Id == id);
                return verificationCode!;
            }
            catch (Exception ex)
            {
                throw new Exception("An error occurred when retrieving code. " + ex.Message);
            }
        }

        public async Task<VerificationCode> GetCodeByBookingId(string id)
        {
            try
            {
                using var context = _contextFactory.CreateDbContext();
                VerificationCode? verificationCode = await context.VerificationCodes.FirstOrDefaultAsync(v => v.BookingId == id);
                return verificationCode!;
            }
            catch (Exception ex)
            {
                throw new Exception("An error occurred when retrieving code. " + ex.Message);
            }
        }

        public async Task<VerificationCode> GetCodeByCode(string code)
        {
            try
            {
                using var context = _contextFactory.CreateDbContext();
                VerificationCode? verificationCode = await context.VerificationCodes.FirstOrDefaultAsync(v => v.Code == code);
                return verificationCode!;
            }
            catch (Exception ex)
            {
                throw new Exception("An error occurred when retrieving code. " + ex.Message);
            }
        }

        public async Task DeleteAsync(string id)
        {
            try
            {
                using var context = _contextFactory.CreateDbContext();
                var code = await GetCodeByIDAsync(id);
                if (code != null)
                {
                    context.VerificationCodes.Remove(code);
                }
                await context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                throw new Exception("An error occurred when removing code. " + ex.Message);
            }
        }

        public async Task<bool> VerifyCodeAsync(string bookingId, string code)
        {
            try
            {
                using var context = _contextFactory.CreateDbContext();
                var foundCode = await context.VerificationCodes.FirstOrDefaultAsync(v => v.BookingId == bookingId && v.Code == code);

                if (foundCode != null)
                {
                    return true;
                }

                return false;
            }
            catch (Exception ex)
            {
                throw new Exception("An error occurred when verifying code. " + ex.Message);
            }
        }
    }
}
