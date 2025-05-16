using ESMART.Domain.Entities.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ESMART.Application.Common.Interface
{
    public interface IBackupRepository
    {
        Task AddBackupSettingsAsync(UserBackupSettings settings);
        Task UpdateBackupSettingsAsync(UserBackupSettings settings);
        Task<UserBackupSettings> GetBackupSettingsAsync();
    }
}
