using ESMART.Application.Common.Models;
using ESMART.Domain.Entities.FrontDesk;
using ESMART.Domain.ViewModels.FrontDesk;

namespace ESMART.Application.Common.Interface
{
    public interface IGuestRepository
    {
        Task<GuestResult> AddGuestAsync(Guest guest);
        Task<List<Guest>> GetAllGuestsAsync();
        Task<Guest> GetGuestByIdAsync(string id);
        Task<GuestResult> UpdateGuestAsync(Guest guest);
        Task DeleteGuestAsync(string id);
        Task<List<GuestViewModel>> SearchGuestAsync(string keyword);
        Task<List<GuestViewModel>> GetDeletedGuestAsync();
        //Task<List<GuestBillViewModel>> GetGuestBillAsync(string guestId);
        //Task<List<GuestBillViewModel>> GetGuestBillByDateAsync(string guestId, DateTime startDate, DateTime endDate);
        Task AddGuestIdentityAsync(GuestIdentity guestIdentity);
        Task<GuestIdenityResult> GetGuestIdentityByGuestIdAsync(string id);
        Task<GuestIdenityResult> UpdateGuestIdentityAsync(GuestIdentity guestIdentity);
    }
}
