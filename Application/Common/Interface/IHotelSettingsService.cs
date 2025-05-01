using ESMART.Domain.Entities.Configuration;

namespace ESMART.Application.Common.Interface
{
    public interface IHotelSettingsService
    {
        Task<List<HotelSetting>> GetSettingsByCategoryAsync(string categoryName);
        Task<HotelSetting?> GetSettingAsync(string key);
        Task<bool> UpdateSettingAsync(HotelSetting setting);
        Task<bool> UpdateHotelInformation(Hotel hotel);
        Task SeedHotelInformation();
        Task<Hotel?> GetHotelInformation();
    }
}
