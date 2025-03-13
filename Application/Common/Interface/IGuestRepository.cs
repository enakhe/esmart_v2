using ESMART.Application.Common.Models;
using ESMART.Domain.Entities.FrontDesk;
using ESMART.Domain.ViewModels.FrontDesk;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ESMART.Application.Interface
{
    public interface IGuestRepository
    {
        Task<GuestResult> AddGuestAsync(Guest guest);
        Task<List<GuestViewModel>> GetAllGuestsAsync();
        Task<GuestResult> GetGuestByIdAsync(string id);
        Task<GuestResult> UpdateGuestAsync(Guest guest);
        Task DeleteGuestAsync(string id);
        Task<List<GuestViewModel>> SearchGuestAsync(string keyword);
        Task<List<GuestViewModel>> GetDeletedGuestAsync();
        Task<List<GuestBillViewModel>> GetGuestBillAsync(string guestId);
        Task<List<GuestBillViewModel>> GetGuestBillByDateAsync(string guestId, DateTime startDate, DateTime endDate);
        Task AddGuestIdentityAsync(GuestIdentity guestIdentity);
    }
}
