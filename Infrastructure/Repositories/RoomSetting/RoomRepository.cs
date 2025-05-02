#nullable disable

using ESMART.Application.Common.Interface;
using ESMART.Application.Common.Models;
using ESMART.Domain.Entities.RoomSettings;
using ESMART.Domain.ViewModels.RoomSetting;
using ESMART.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace ESMART.Infrastructure.Repositories.RoomSetting
{
    public class RoomRepository(IDbContextFactory<ApplicationDbContext> contextFactory) : IRoomRepository
    {
        private readonly IDbContextFactory<ApplicationDbContext> _contextFactory = contextFactory;

        public async Task AddRoom(Room room)
        {
            try
            {
                using var context = _contextFactory.CreateDbContext();
                await context.Rooms.AddAsync(room);
                await context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                throw new Exception("An error occurred when adding room. " + ex.Message);
            }
        }

        public async Task<List<Room>> GetAllRooms()
        {
            try
            {
                using var context = _contextFactory.CreateDbContext();
                var allRooms = await context.Rooms
                                .Where(r => r.IsTrashed == false)
                                .OrderBy(r => r.Number)
                                .ToListAsync();
                return allRooms;
            }
            catch (Exception ex)
            {
                throw new Exception("An error occurred when retrieving rooms. " + ex.Message);
            }
        }

        public async Task<List<Room>> GetAvailableRooms()
        {
            try
            {
                using var context = _contextFactory.CreateDbContext();
                var allRooms = await context.Rooms
                                .Where(r => !r.IsTrashed && r.Status == RoomStatus.Vacant)
                                .OrderBy(r => r.Number)
                                .ToListAsync();
                return allRooms;
            }
            catch (Exception ex)
            {
                throw new Exception("An error occurred when retrieving rooms. " + ex.Message);
            }
        }

        public async Task<Room> GetRoomById(string Id)
        {
            try
            {
                using var context = _contextFactory.CreateDbContext();
                var room = await context.Rooms.FirstOrDefaultAsync(r => r.Id == Id);

                return room;
            }
            catch (Exception ex)
            {
                throw new Exception("An error occurred when retrieving a room with the provided ID. " + ex.Message);
            }
        }

        public async Task<Room> GetRoomByNumber(string number)
        {
            try
            {
                using var context = _contextFactory.CreateDbContext();
                var room = await context.Rooms.FirstOrDefaultAsync(r => r.Number == number);

                return room;
            }

            catch (Exception ex)
            {
                throw new Exception("An error occurred when retrieving a room with the provided number. " + ex.Message);
            }
        }

        public async Task UpdateRoom(Room room)
        {
            try
            {
                using var context = _contextFactory.CreateDbContext();
                context.Entry(room).State = EntityState.Modified;
                await context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                throw new Exception("An error occurred when updating room. " + ex.Message);
            }
        }

        public async Task DeleteRoom(string Id)
        {
            try
            {
                using var context = _contextFactory.CreateDbContext();
                var room = await context.Rooms.FirstOrDefaultAsync(r => r.Id == Id);

                room.IsTrashed = true;
                await UpdateRoom(room);
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
                using var context = _contextFactory.CreateDbContext();
                var allRooms = await context.Rooms
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
                using var context = _contextFactory.CreateDbContext();
                var allRooms = await context.Rooms
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
                using var context = _contextFactory.CreateDbContext();
                var allRooms = await context.Rooms
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

        public async Task<List<RoomViewModel>> GetRoomsByFilter(string roomTypeId, string status)
        {
            try
            {
                using var context = _contextFactory.CreateDbContext();
                var allRooms = await context.Rooms
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
                using var context = _contextFactory.CreateDbContext();
                var result = await context.Areas.AddAsync(area);
                await context.SaveChangesAsync();
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
                using var context = _contextFactory.CreateDbContext();
                return await context.Areas.Where(a => a.IsTrashed != true).OrderBy(a => a.Number).ToListAsync();
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
                using var context = _contextFactory.CreateDbContext();
                context.Entry(area).State = EntityState.Modified;
                await context.SaveChangesAsync();
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
                using var context = _contextFactory.CreateDbContext();
                var area = await context.Areas.FirstOrDefaultAsync(a => a.Id == id);
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
                using var context = _contextFactory.CreateDbContext();
                var area = await context.Areas.FirstOrDefaultAsync(r => r.Id == Id);

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
                using var context = _contextFactory.CreateDbContext();
                var result = await context.Floors.AddAsync(floor);
                await context.SaveChangesAsync();
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
                using var context = _contextFactory.CreateDbContext();
                return await context.Floors.Where(a => a.IsTrashed != true).OrderBy(a => a.Number).ToListAsync();
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
                using var context = _contextFactory.CreateDbContext();
                context.Entry(floor).State = EntityState.Modified;
                await context.SaveChangesAsync();
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
                using var context = _contextFactory.CreateDbContext();
                var floor = await context.Floors.FirstOrDefaultAsync(a => a.Id == id);
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
                using var context = _contextFactory.CreateDbContext();
                var floor = await context.Floors.FirstOrDefaultAsync(r => r.Id == Id);

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
                using var context = _contextFactory.CreateDbContext();
                return await context.Floors.Where(f => f.BuildingId == id).OrderBy(f => f.Number).ToListAsync();
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
                using var context = _contextFactory.CreateDbContext();
                var result = await context.Buildings.AddAsync(building);
                await context.SaveChangesAsync();
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
                using var context = _contextFactory.CreateDbContext();
                return await context.Buildings.Where(a => a.IsTrashed != true).OrderBy(a => a.Number).ToListAsync();
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
                using var context = _contextFactory.CreateDbContext();
                context.Entry(building).State = EntityState.Modified;
                await context.SaveChangesAsync();
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
                using var context = _contextFactory.CreateDbContext();
                var building = await context.Buildings.FirstOrDefaultAsync(a => a.Id == id);
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
                using var context = _contextFactory.CreateDbContext();
                var building = await context.Buildings.FirstOrDefaultAsync(r => r.Id == Id);

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
                using var context = _contextFactory.CreateDbContext();
                var result = await context.RoomTypes.AddAsync(roomType);
                await context.SaveChangesAsync();
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
                using var context = _contextFactory.CreateDbContext();
                var allRoomType = await context.RoomTypes
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
                using var context = _contextFactory.CreateDbContext();
                var roomType = await context.RoomTypes.FirstOrDefaultAsync(r => r.Id == Id);

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
                using var context = _contextFactory.CreateDbContext();
                context.Entry(roomType).State = EntityState.Modified;
                await context.SaveChangesAsync();
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
                using var context = _contextFactory.CreateDbContext();
                var roomType = await context.RoomTypes.FirstOrDefaultAsync(rt => rt.Id == id);
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
            using var context = _contextFactory.CreateDbContext();
            return context.Rooms.Where(r => r.Status.ToString() == RoomStatus.Reserved.ToString()).Count();
        }

        public int GetNoBooking()
        {
            using var context = _contextFactory.CreateDbContext();
            return context.Rooms.Where(r => r.Status.ToString() == RoomStatus.Booked.ToString()).Count();
        }

        public int GetNoMaintenance()
        {
            using var context = _contextFactory.CreateDbContext();
            return context.Rooms.Where(r => r.Status.ToString() == RoomStatus.Maintenance.ToString()).Count();
        }

        public async Task<int> GetBuildingNumber()
        {
            using var context = _contextFactory.CreateDbContext();
            return await context.Buildings.Where(b => b.IsTrashed == false).CountAsync();
        }

        public async Task<int> GetFloorNumber()
        {
            using var context = _contextFactory.CreateDbContext();
            return await context.Floors.Where(b => b.IsTrashed == false).CountAsync();
        }

        public async Task<int> GetAreaNumber()
        {
            using var context = _contextFactory.CreateDbContext();
            return await context.Areas.Where(b => b.IsTrashed == false).CountAsync();
        }

        public async Task<int> GetRoomNumber()
        {
            using var context = _contextFactory.CreateDbContext();
            return await context.Rooms.Where(b => b.IsTrashed == false).CountAsync();
        }
    }
}