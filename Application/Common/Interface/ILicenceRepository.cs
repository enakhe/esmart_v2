using ESMART.Domain.Entities.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ESMART.Application.Common.Interface
{
    public interface ILicenceRepository
    {
        Task AddLicenceAsync(LicenceInformation licence);
        Task<LicenceInformation?> GetLicenceAsync();
        Task UpdateLicenceAsync(LicenceInformation licence);
    }
}
