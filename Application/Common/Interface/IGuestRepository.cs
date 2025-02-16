using ESMART.Domain.Entities.FrontDesk;
using ESMART.Domain.ViewModels.FrontDesk;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ESMART.Application.Common.Interface
{
    public interface IGuestRepository
    {
        Task AddGuestAsync(Guest guest);
        Task<List<GuestViewModel>> GetAllGuestsAsync();
        Task<Guest?> GetGuestByIdAsync(string id);
        Task UpdateGuestAsync(Guest guest);
        Task DeleteGuestAsync(string id);
        Task<List<GuestViewModel>> SearchGuestAsync(string keyword);
        Task<List<GuestViewModel>> GetDeletedGuestAsync();
        Task<List<GuestBillViewModel>> GetGuestBillAsync(string guestId);
    }
}
