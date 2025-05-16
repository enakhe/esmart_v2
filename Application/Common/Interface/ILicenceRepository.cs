using ESMART.Domain.Entities.Configuration;

namespace ESMART.Application.Common.Interface
{
    public interface ILicenceRepository
    {
        Task AddLicenceAsync(LicenceInformation licence);
        Task<LicenceInformation?> GetLicenceAsync();
        Task UpdateLicenceAsync(LicenceInformation licence);
    }
}
