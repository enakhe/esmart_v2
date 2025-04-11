using ESMART.Application.Common.Models;
using ESMART.Domain.Entities.RoomSettings;
using ESMART.Domain.ViewModels.RoomSetting;

namespace ESMART.Application.Interface
{
    public interface IRoomTypeRepository
    {
        Task<RoomTypeResult> AddRoomTypeAsync(RoomType roomType);
        Task<List<RoomTypeViewModel>> GetAllRoomTypes();
        Task<RoomTypeResult> GetRoomTypeById(string Id);
        Task<RoomTypeResult> UpdateRoomType(RoomType roomType);
        Task<RoomTypeResult> DeleteRoomType(string id);
    }
}
