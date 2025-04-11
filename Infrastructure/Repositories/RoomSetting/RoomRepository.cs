#nullable disable

using ESMART.Application.Common.Models;
using ESMART.Application.Interface;
using ESMART.Domain.Entities.FrontDesk;
using ESMART.Domain.Entities.RoomSettings;
using ESMART.Domain.Enum;
using ESMART.Domain.ViewModels.FrontDesk;
using ESMART.Domain.ViewModels.RoomSetting;
using ESMART.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System.Globalization;
using System.Threading.Tasks;

namespace ESMART.Infrastructure.Repositories.RoomSetting
{
    public class RoomRepository : IRoomRepository
    {
        private readonly ApplicationDbContext _db;
        private readonly IRoomTypeRepository _roomTypeRepository;

        public RoomRepository(ApplicationDbContext db, IRoomTypeRepository roomTypeRepository)
        {
            _db = db;
            _roomTypeRepository = roomTypeRepository;
        }

        public async Task<RoomResult> AddRoom(Room room)
        {
            try
            {
                var result = await _db.Rooms.AddAsync(room);
                await _db.SaveChangesAsync();
                return RoomResult.Success(room);
            }
            catch (Exception ex)
            {
                throw new Exception("An error occurred when adding room. " + ex.Message);
            }
        }

        public async Task<List<RoomViewModel>> GetAllRooms()
        {
            try
            {
                var allRooms = await _db.Rooms
                                .Where(r => r.IsTrashed == false)
                                .OrderBy(r => r.Number)
                                .Select(r => new RoomViewModel
                                {
                                    Id = r.Id,
                                    Number = r.Number,
                                    RoomType = r.RoomType.Name,
                                    Floor = r.Floor.Number,
                                    Building = r.Building.Number,
                                    Area = r.Area.Number,
                                    Rate = r.RoomType.Rate.ToString(),
                                    Status = r.Status,
                                    CreatedBy = r.ApplicationUser.FullName,
                                    DateCreated = r.DateCreated,
                                    DateModified = r.DateModified,
                                }).ToListAsync();
                return allRooms;
            }
            catch (Exception ex)
            {
                throw new Exception("An error occurred when retrieving rooms. " + ex.Message);
            }
        }

        public async Task<RoomResult> GetRoomById(string Id)
        {
            try
            {
                var room = await _db.Rooms.FirstOrDefaultAsync(r => r.Id == Id);

                if (room != null)
                    return RoomResult.Success(room);

                return RoomResult.Failure(["Unable to find a room with the provided ID"]);
            }
            catch (Exception ex)
            {
                throw new Exception("An error occurred when retrieving a room with the provided ID. " + ex.Message);
            }
        }

        public async Task<RoomResult> GetRoomByNumber(string number)
        {
            try
            {
                var room = await _db.Rooms.FirstOrDefaultAsync(r => r.Number == number);

                if (room != null)
                    return RoomResult.Success(room);

                return RoomResult.Failure(["Unable to find a room with the provided number"]);
            }
            catch (Exception ex)
            {
                throw new Exception("An error occurred when retrieving a room with the provided number. " + ex.Message);
            }
        }

        public async Task<RoomResult> UpdateRoom(Room room)
        {
            try
            {
                _db.Entry(room).State = EntityState.Modified;
                await _db.SaveChangesAsync();
                return RoomResult.Success(room);
            }
            catch (Exception ex)
            {
                throw new Exception("An error occurred when updating room. " + ex.Message);
            }
        }

        public async Task<RoomResult> DeleteRoom(string Id)
        {
            try
            {
                var room = await _db.Rooms.FirstOrDefaultAsync(r => r.Id == Id);
                if (room != null)
                {
                    room.IsTrashed = true;
                    await UpdateRoom(room);
                }

                return RoomResult.Failure(["Unable to delete a room with the provided ID"]);
            }
            catch (Exception ex)
            {
                throw new Exception("An error occurred when deleting a room" + ex.Message);
            }
        }

        public async Task<List<RoomViewModel>> SearchRoom(string keyword)
        {
            try
            {
                var allRooms = await _db.Rooms
                                .Where(r => r.Number == (keyword) && r.IsTrashed == false)
                                .OrderBy(r => r.Number)
                                .Select(r => new RoomViewModel
                                {
                                    Id = r.Id,
                                    Number = r.Number,
                                    RoomType = r.RoomType.Name,
                                    Floor = r.Floor.Number,
                                    Building = r.Building.Number,
                                    Area = r.Area.Number,
                                    Rate = r.RoomType.Rate.ToString(),
                                    Status = r.Status,
                                    CreatedBy = r.ApplicationUser.FullName,
                                    DateCreated = r.DateCreated,
                                    DateModified = r.DateModified,
                                }).ToListAsync();
                return allRooms;
            }
            catch (Exception ex)
            {
                throw new Exception("An error occurred when retrieving rooms. " + ex.Message);
            }
        }

        public async Task<List<RoomViewModel>> FilterByStatus(string keyword)
        {
            try
            {
                var allRooms = await _db.Rooms
                                .Where(r => r.Status.ToString() == (keyword) && r.IsTrashed == false)
                                .OrderBy(r => r.Number)
                                .Select(r => new RoomViewModel
                                {
                                    Id = r.Id,
                                    Number = r.Number,
                                    RoomType = r.RoomType.Name,
                                    Floor = r.Floor.Number,
                                    Building = r.Building.Number,
                                    Area = r.Area.Number,
                                    Rate = r.RoomType.Rate.ToString(),
                                    Status = r.Status,
                                    CreatedBy = r.ApplicationUser.FullName,
                                    DateCreated = r.DateCreated,
                                    DateModified = r.DateModified,
                                }).ToListAsync();
                return allRooms;
            }
            catch (Exception ex)
            {
                throw new Exception("An error occurred when retrieving rooms. " + ex.Message);
            }
        }

        public async Task<List<RoomViewModel>> FilterByType(string keyword)
        {
            try
            {
                var allRooms = await _db.Rooms
                                .Where(r => r.RoomType.Name == (keyword) && r.IsTrashed == false)
                                .OrderBy(r => r.Number)
                                .Select(r => new RoomViewModel
                                {
                                    Id = r.Id,
                                    Number = r.Number,
                                    RoomType = r.RoomType.Name,
                                    Floor = r.Floor.Number,
                                    Building = r.Building.Number,
                                    Area = r.Area.Number,
                                    Rate = r.RoomType.Rate.ToString(),
                                    Status = r.Status,
                                    CreatedBy = r.ApplicationUser.FullName,
                                    DateCreated = r.DateCreated,
                                    DateModified = r.DateModified,
                                }).ToListAsync();
                return allRooms;
            }
            catch (Exception ex)
            {
                throw new Exception("An error occurred when retrieving rooms. " + ex.Message);
            }
        }

        public async Task<RoomResult> FindByRoomNo(string roomNumber)
        {
            try
            {
                var room = await _db.Rooms.FirstOrDefaultAsync(r => r.Number == roomNumber);

                if (room != null)
                    return RoomResult.Success(room);

                return RoomResult.Failure(["Unable to find a room with the provided number"]);
            }
            catch (Exception ex)
            {
                throw new Exception("An error occurred when retrieving a room with the provided number. " + ex.Message);
            }
        }

        public async Task<List<RoomViewModel>> GetRoomsByFilter(string roomTypeId, string status)
        {
            try
            {
                var allRooms = await _db.Rooms
                                .Where(r => r.RoomType.Id == (roomTypeId) && r.Status.ToString() == status && r.IsTrashed == false)
                                .OrderBy(r => r.Number)
                                .Select(r => new RoomViewModel
                                {
                                    Id = r.Id,
                                    Number = r.Number,
                                    RoomType = r.RoomType.Name,
                                    Floor = r.Floor.Number,
                                    Building = r.Building.Number,
                                    Area = r.Area.Number,
                                    Rate = r.RoomType.Rate.ToString(),
                                    Status = r.Status,
                                    CreatedBy = r.ApplicationUser.FullName,
                                    DateCreated = r.DateCreated,
                                    DateModified = r.DateModified,
                                }).ToListAsync();
                return allRooms;
            }
            catch (Exception ex)
            {
                throw new Exception("An error occurred when retrieving rooms. " + ex.Message);
            }
        }


        public async Task<AreaResult> AddArea(Area area)
        {
            try
            {
                var result = await _db.Areas.AddAsync(area);
                await _db.SaveChangesAsync();
                return AreaResult.Success(area);
            }
            catch (Exception ex)
            {
                throw new Exception("An error occurred when adding area. " + ex.Message);
            }
        }

        public async Task<List<Area>> GetAllAreas()
        {
            try
            {
                return await _db.Areas.Where(a => a.IsTrashed != true).OrderBy(a => a.Number).ToListAsync();
            }
            catch (Exception ex)
            {
                throw new Exception("An error occurred when retrieving areas. " + ex.Message);
            }
        }

        public async Task<AreaResult> UpdateArea(Area area)
        {
            try
            {
                _db.Entry(area).State = EntityState.Modified;
                await _db.SaveChangesAsync();
                return AreaResult.Success(area);
            }
            catch (Exception ex)
            {
                throw new Exception("An error occurred when updating area. " + ex.Message);
            }
        }

        public async Task<AreaResult> GetAreaById(string id)
        {
            try
            {
                var area = await _db.Areas.FirstOrDefaultAsync(a => a.Id == id);
                if (area != null)
                    return AreaResult.Success(area);

                return AreaResult.Failure(["Unable to find an area with the provided ID"]);
            }
            catch (Exception ex)
            {
                throw new Exception("An error occurred when getting area by the provided Id" + ex.Message);
            }
        }

        public async Task<AreaResult> DeleteArea(string Id)
        {
            try
            {
                var area = await _db.Areas.FirstOrDefaultAsync(r => r.Id == Id);

                if (area == null)
                    return AreaResult.Failure(["Unable to delete a area with the provided ID"]);

                area.IsTrashed = true;
                var result = await UpdateArea(area);

                return AreaResult.Success(result.Response);
            }
            catch (Exception ex)
            {
                throw new Exception("An error occurred when deleting a area" + ex.Message);
            }
        }


        public async Task<FloorResult> AddFloor(Floor floor)
        {
            try
            {
                var result = await _db.Floors.AddAsync(floor);
                await _db.SaveChangesAsync();
                return FloorResult.Success(floor);
            }
            catch (Exception ex)
            {
                throw new Exception("An error occurred when adding floor. " + ex.Message);
            }
        }

        public async Task<List<Floor>> GetAllFloors()
        {
            try
            {
                return await _db.Floors.Where(a => a.IsTrashed != true).OrderBy(a => a.Number).ToListAsync();
            }
            catch (Exception ex)
            {
                throw new Exception("An error occurred when retrieving floors. " + ex.Message);
            }
        }

        public async Task<FloorResult> UpdateFloor(Floor floor)
        {
            try
            {
                _db.Entry(floor).State = EntityState.Modified;
                await _db.SaveChangesAsync();
                return FloorResult.Success(floor);
            }
            catch (Exception ex)
            {
                throw new Exception("An error occurred when updating floor. " + ex.Message);
            }
        }

        public async Task<FloorResult> GetFloorById(string id)
        {
            try
            {
                var floor = await _db.Floors.FirstOrDefaultAsync(a => a.Id == id);
                if (floor != null)
                    return FloorResult.Success(floor);

                return FloorResult.Failure(["Unable to find an floor with the provided ID"]);
            }
            catch (Exception ex)
            {
                throw new Exception("An error occurred when getting floor by the provided Id" + ex.Message);
            }
        }

        public async Task<FloorResult> DeleteFloor(string Id)
        {
            try
            {
                var floor = await _db.Floors.FirstOrDefaultAsync(r => r.Id == Id);

                if (floor == null)
                    return FloorResult.Failure(["Unable to delete a building with the provided ID"]);

                floor.IsTrashed = true;
                var result = await UpdateFloor(floor);

                return FloorResult.Success(result.Response);
            }
            catch (Exception ex)
            {
                throw new Exception("An error occurred when deleting a floor" + ex.Message);
            }
        }

        public async Task<List<Floor>> GetFloorsByBuilding(string id)
        {
            try
            {
                return await _db.Floors.Where(f => f.BuildingId == id).OrderBy(f => f.Number).ToListAsync();
            }
            catch (Exception ex)
            {
                throw new Exception("An error occurred when retrieving floors. " + ex.Message);
            }
        }

        public async Task<BuildingResult> AddBuilding(Building building)
        {
            try
            {
                var result = await _db.Buildings.AddAsync(building);
                await _db.SaveChangesAsync();
                return BuildingResult.Success(building);
            }
            catch (Exception ex)
            {
                throw new Exception("An error occurred when adding building. " + ex.Message);
            }
        }

        public async Task<List<Building>> GetAllBuildings()
        {
            try
            {
                return await _db.Buildings.Where(a => a.IsTrashed != true).OrderBy(a => a.Number).ToListAsync();
            }
            catch (Exception ex)
            {
                throw new Exception("An error occurred when retrieving buildings. " + ex.Message);
            }
        }

        public async Task<BuildingResult> UpdateBuilding(Building building)
        {
            try
            {
                _db.Entry(building).State = EntityState.Modified;
                await _db.SaveChangesAsync();
                return BuildingResult.Success(building);
            }
            catch (Exception ex)
            {
                throw new Exception("An error occurred when updating building. " + ex.Message);
            }
        }

        public async Task<BuildingResult> GetBuildingById(string id)
        {
            try
            {
                var building = await _db.Buildings.FirstOrDefaultAsync(a => a.Id == id);
                if (building != null)
                    return BuildingResult.Success(building);

                return BuildingResult.Failure(["Unable to find an building with the provided ID"]);
            }
            catch (Exception ex)
            {
                throw new Exception("An error occurred when getting building by the provided Id" + ex.Message);
            }
        }

        public async Task<BuildingResult> DeleteBuilding(string Id)
        {
            try
            {
                var building = await _db.Buildings.FirstOrDefaultAsync(r => r.Id == Id);

                if (building == null)
                    return BuildingResult.Failure(["Unable to delete a building with the provided ID"]);

                building.IsTrashed = true;
                var result = await UpdateBuilding(building);

                return BuildingResult.Success(result.Response);
            }
            catch (Exception ex)
            {
                throw new Exception("An error occurred when deleting a building" + ex.Message);
            }
        }

        public async Task<RoomTypeResult> AddRoomTypeAsync(RoomType roomType)
        {
            try
            {
                var result = await _db.RoomTypes.AddAsync(roomType);
                await _db.SaveChangesAsync();
                return RoomTypeResult.Success(roomType);
            }
            catch (Exception ex)
            {
                throw new Exception("An error occurred when adding room type. " + ex.Message);
            }
        }

        public async Task<List<RoomType>> GetAllRoomTypes()
        {
            try
            {
                var allRoomType = await _db.RoomTypes
                                    .Where(r => r.IsTrashed == false)
                                    .OrderBy(r => r.Name)
                                    .ToListAsync();

                return allRoomType;
            }
            catch (Exception ex)
            {
                throw new Exception("An error occurred when retrieving room types. " + ex.Message);
            }
        }

        public async Task<RoomTypeResult> GetRoomTypeById(string Id)
        {
            try
            {
                var roomType = await _db.RoomTypes.FirstOrDefaultAsync(r => r.Id == Id);

                if (roomType != null)
                    return RoomTypeResult.Success(roomType);

                return RoomTypeResult.Failure(["Unable to find a room type with the provided ID"]);
            }
            catch (Exception ex)
            {
                throw new Exception("An error occurred when retrieving a room type with the provided ID. " + ex.Message);
            }
        }

        public async Task<RoomTypeResult> UpdateRoomType(RoomType roomType)
        {
            try
            {
                _db.Entry(roomType).State = EntityState.Modified;
                await _db.SaveChangesAsync();
                return RoomTypeResult.Success(roomType);
            }
            catch (Exception ex)
            {
                throw new Exception("An error occurred when updating room type. " + ex.Message);
            }
        }

        public async Task<RoomTypeResult> DeleteRoomType(string id)
        {
            try
            {
                var roomType = await _db.RoomTypes.FirstOrDefaultAsync(rt => rt.Id == id);
                if (roomType != null)
                {
                    roomType.IsTrashed = true;
                    await UpdateRoomType(roomType);
                    return RoomTypeResult.Success(roomType);
                }

                return RoomTypeResult.Failure(["Unable to delete a room type with the provided ID"]);
            }
            catch (Exception ex)
            {
                throw new Exception("An error occurred when deleting a room type" + ex.Message);
            }
        }

        public int GetNoReserved()
        {
            return _db.Rooms.Where(r => r.Status.ToString() == RoomStatus.Reserved.ToString()).Count();
        }

        public int GetNoBooking()
        {
            return _db.Rooms.Where(r => r.Status.ToString() == RoomStatus.Booked.ToString()).Count();
        }

        public int GetNoMaintenance()
        {
            return _db.Rooms.Where(r => r.Status.ToString() == RoomStatus.Maintenance.ToString()).Count();
        }
    }
}