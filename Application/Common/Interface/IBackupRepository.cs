using ESMART.Domain.Entities.Configuration;

namespace ESMART.Application.Common.Interface
{
    public interface IBackupRepository
    {
        Task AddBackupSettingsAsync(UserBackupSettings settings);
        Task UpdateBackupSettingsAsync(UserBackupSettings settings);
        Task<UserBackupSettings> GetBackupSettingsAsync();
    }
}
