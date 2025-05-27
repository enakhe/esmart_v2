using ESMART.Domain.Entities.FrontDesk;
using ESMART.Domain.ViewModels.FrontDesk;

namespace ESMART.Application.Common.Interface
{
    public interface IGuestRepository
    {
        Task<int> GetGuestNumber();
        Task<int> GetInHouseGuestNumber();
        Task AddGuestAsync(Guest guest);
        Task FundAccountAsync(GuestAccount guestAccounts);
        Task UpdateGuestAccountAsync(GuestAccount guestAccount);
        Task AddGuestTransactionAsync(GuestTransaction guestTransaction);
        Task<List<GuestTransaction>> GetGuestTransactionsAsync(string guestId);
        Task<GuestAccount> GetGuestAccountByGuestIdAsync(string guestId);
        Task<GuestAccount> GetGuestAccountByIdAsync(string id);
        Task<List<Guest>> GetAllGuestsAsync();
        Task<Guest> GetGuestByIdAsync(string id);
        Task UpdateGuestAsync(Guest guest);
        Task DeleteGuestAsync(string id);
        Task<List<GuestViewModel>> SearchGuestAsync(string keyword);
        Task<List<GuestViewModel>> GetDeletedGuestAsync();
        Task AddGuestIdentityAsync(GuestIdentity guestIdentity);
        Task<GuestIdentity> GetGuestIdentityByGuestIdAsync(string id);
        Task UpdateGuestIdentityAsync(GuestIdentity guestIdentity);
    }
}
