using ESMART.Domain.Entities.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
