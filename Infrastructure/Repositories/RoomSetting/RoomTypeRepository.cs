
using ESMART.Application.Common.Models;
using ESMART.Application.Interface;
using ESMART.Domain.Entities.RoomSettings;
using ESMART.Domain.ViewModels.RoomSetting;
using ESMART.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace ESMART.Infrastructure.Repositories.RoomSetting
{
    public class RoomTypeRepository : IRoomTypeRepository
    {
        private readonly ApplicationDbContext _db;

        public RoomTypeRepository(ApplicationDbContext db)
        {
            _db = db;
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

        public async Task<List<RoomTypeViewModel>> GetAllRoomTypes()
        {
            try
            {
                var allRoomType = await _db.RoomTypes
                                    .Where(r => r.IsTrashed == false)
                                    .OrderBy(r => r.Name)
                                    .Select(r => new RoomTypeViewModel
                                    {
                                        Id = r.Id,
                                        Name = r.Name,
                                        Rate = r.Rate,
                                        DateCreated = r.DateCreated,
                                        DateModified = r.DateModified,
                                    }).ToListAsync();

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
    }
}
