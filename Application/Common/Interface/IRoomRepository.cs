using ESMART.Application.Common.Models;
using ESMART.Domain.Entities.RoomSettings;
using ESMART.Domain.ViewModels.RoomSetting;
using System.Collections.Generic;
namespace ESMART.Application.Interface
{
    public interface IRoomRepository
    {
        Task<RoomResult> AddRoom(Room room);
        Task<List<RoomViewModel>> GetAllRooms();
        Task<RoomResult> GetRoomById(string Id);
        Task<RoomResult> GetRoomByNumber(string number);
        Task<RoomResult> UpdateRoom(Room room);
        Task<RoomResult> DeleteRoom(string Id);
        Task<List<RoomViewModel>> SearchRoom(string keyword);
        Task<List<RoomViewModel>> FilterByStatus(string keyword);
        Task<List<RoomViewModel>> FilterByType(string keyword);
        Task<RoomResult> FindByRoomNo(string roomNumber);
        Task<List<RoomViewModel>> GetRoomsByFilter(string roomTypeId, string status);
        Task<AreaResult> AddArea(Area area);
        Task<List<Area>> GetAllAreas();
        Task<AreaResult> GetAreaById(string Id);
        Task<AreaResult> UpdateArea(Area area);
        Task<AreaResult> DeleteArea(string Id);
        Task<FloorResult> AddFloor(Floor floor);
        Task<List<Floor>> GetAllFloors();
        Task<FloorResult> GetFloorById(string Id);
        Task<FloorResult> UpdateFloor(Floor floor);
        Task<FloorResult> DeleteFloor(string Id);
        Task<List<Floor>> GetFloorsByBuilding(string id);
        Task<BuildingResult> AddBuilding(Building building);
        Task<List<Building>> GetAllBuildings();
        Task<BuildingResult> UpdateBuilding(Building building);
        Task<BuildingResult> GetBuildingById(string Id);
        Task<BuildingResult> DeleteBuilding(string Id);
        Task<RoomTypeResult> AddRoomTypeAsync(RoomType roomType);
        Task<List<RoomType>> GetAllRoomTypes();
        Task<RoomTypeResult> GetRoomTypeById(string Id);
        Task<RoomTypeResult> UpdateRoomType(RoomType roomType);
        Task<RoomTypeResult> DeleteRoomType(string id);
        int GetNoReserved();
        int GetNoBooking();
        int GetNoMaintenance();
    }
}
