using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ESMART.Domain.Entities.Configuration
{
    public class UserBackupSettings
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public BackupFrequency Frequency { get; set; }
        public DateTime LastBackup { get; set; }
    }

    public enum BackupFrequency
    {
        Daily,
        Weekly,
        Monthly
    }
}
