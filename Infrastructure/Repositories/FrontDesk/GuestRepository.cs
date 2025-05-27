#nullable disable

using ESMART.Application.Common.Interface;
using ESMART.Domain.Entities.FrontDesk;
using ESMART.Domain.ViewModels.FrontDesk;
using ESMART.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace ESMART.Infrastructure.Repositories.FrontDesk
{
    public class GuestRepository(IDbContextFactory<ApplicationDbContext> contextFactory) : IGuestRepository
    {
        private readonly IDbContextFactory<ApplicationDbContext> _contextFactory = contextFactory;

        public async Task AddGuestAsync(Guest guest)
        {
            try
            {
                using var context = _contextFactory.CreateDbContext();
                await context.Guests.AddAsync(guest);
                await context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                throw new Exception("An error occurred when adding guest. " + ex.Message);
            }
        }

        public async Task AddGuestIdentityAsync(GuestIdentity guestIdentity)
        {
            try
            {
                using var context = _contextFactory.CreateDbContext();
                await context.GuestIdentities.AddAsync(guestIdentity);
                await context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                throw new Exception("An error occurred when adding guest identity information. " + ex.Message);
            }
        }

        public async Task<List<Guest>> GetAllGuestsAsync()
        {
            try
            {
                using var context = _contextFactory.CreateDbContext();

                var allGuest = await context.Guests
                    .Where(g => g.IsTrashed == false)
                    .OrderBy(g => g.FirstName)
                    .ToListAsync();

                return allGuest;
            }
            catch (Exception ex)
            {
                throw new Exception("An error occurred when retrieving guests. " + ex.Message);
            }
        }

        public async Task<Guest> GetGuestByIdAsync(string id)
        {
            try
            {
                using var context = _contextFactory.CreateDbContext();
                var guest = await context.Guests.Include(g => g.ApplicationUser).Include(g => g.GuestAccount).FirstOrDefaultAsync(g => g.Id == id);

                return guest;
            }
            catch (Exception ex)
            {
                throw new Exception("An error occurred when retrieving guest by ID. " + ex.Message);
            }
        }

        public async Task<GuestIdentity> GetGuestIdentityByGuestIdAsync(string id)
        {
            try
            {
                using var context = _contextFactory.CreateDbContext();
                var guestIdentities = await context.GuestIdentities.FirstOrDefaultAsync(c => c.GuestId == id);

                return guestIdentities;
            }
            catch (Exception ex)
            {
                throw new Exception("An error occurred when retrieving guest identity by ID. " + ex.Message);
            }
        }

        public async Task UpdateGuestAsync(Guest guest)
        {
            try
            {
                using var context = _contextFactory.CreateDbContext();
                context.Entry(guest).State = EntityState.Modified;
                await context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                throw new Exception("An error occurred when updating guest. " + ex.Message);
            }
        }

        public async Task UpdateGuestIdentityAsync(GuestIdentity guestIdentity)
        {
            try
            {
                using var context = _contextFactory.CreateDbContext();
                context.Entry(guestIdentity).State = EntityState.Modified;
                await context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                throw new Exception("An error occurred when updating guest. " + ex.Message);
            }
        }

        public async Task DeleteGuestAsync(string id)
        {
            try
            {
                using var context = _contextFactory.CreateDbContext();
                var guest = await context.Guests.FirstOrDefaultAsync(c => c.Id == id);
                if (guest != null)
                {
                    guest.IsTrashed = true;
                    context.Entry(guest).State = EntityState.Modified;
                    await context.SaveChangesAsync();
                }
                else
                {
                    throw new Exception("Guest not found");
                }
            }
            catch (Exception ex)
            {
                throw new Exception("An error occurred when deleting guest. " + ex.Message);
            }
        }

        public async Task<List<GuestViewModel>> SearchGuestAsync(string keyword)
        {
            try
            {
                using var context = _contextFactory.CreateDbContext();

                var searchGuest = await context.Guests
                    .Where(c => (c.FullName.Contains(keyword) || c.Email.Contains(keyword) || c.PhoneNumber.Contains(keyword) || c.Street.Contains(keyword) || c.City.Contains(keyword) || c.State.Contains(keyword) || c.Country.Contains(keyword) || c.GuestId.Contains(keyword)) && c.IsTrashed == false)
                    .OrderBy(g => g.FullName)
                    .Select(guest => new GuestViewModel
                    {
                        Id = guest.Id,
                        GuestId = guest.GuestId,
                        FullName = guest.FullName,
                        Email = guest.Email,
                        PhoneNumber = guest.PhoneNumber,
                        City = guest.City,
                        State = guest.State,
                        CreatedBy = guest.ApplicationUser.FullName,
                        DateCreated = guest.DateCreated,
                        DateModified = guest.DateModified,
                    }).ToListAsync();

                return searchGuest;
            }
            catch (Exception ex)
            {
                throw new Exception("An error occurred when searching guests. " + ex.Message);
            }
        }

        public async Task<List<GuestViewModel>> GetDeletedGuestAsync()
        {
            try
            {
                using var context = _contextFactory.CreateDbContext();
                var allGuest = await context.Guests
                    .Where(g => g.IsTrashed == true)
                    .OrderBy(g => g.FullName)
                    .Select(guest => new GuestViewModel
                    {
                        Id = guest.Id,
                        GuestId = guest.GuestId,
                        FullName = guest.FullName,
                        Email = guest.Email,
                        PhoneNumber = guest.PhoneNumber,
                        City = guest.City,
                        State = guest.State,
                        CreatedBy = guest.ApplicationUser.FullName,
                        DateCreated = guest.DateCreated,
                        DateModified = guest.DateModified,
                    }).ToListAsync();

                return allGuest;
            }
            catch (Exception ex)
            {
                throw new Exception("An error occurred when retrieving deleted guests. " + ex.Message);
            }
        }

        public async Task<int> GetGuestNumber()
        {
            using var context = _contextFactory.CreateDbContext();
            return await context.Guests.Where(b => b.IsTrashed == false).CountAsync();
        }

        public async Task<int> GetInHouseGuestNumber()
        {
            using var context = _contextFactory.CreateDbContext();
            return await context.Guests.Where(b => b.IsTrashed == false && b.Status == "Active").CountAsync();
        }

        // Fund guest account
        public async Task FundAccountAsync(GuestAccount guestAccounts)
        {
            try
            {
                using var context = _contextFactory.CreateDbContext();

                await context.GuestAccounts.AddAsync(guestAccounts);
                await context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                throw new Exception("An error occurred when adding guest. " + ex.Message);
            }
        }

        // add guest transaction
        public async Task AddGuestTransactionAsync(GuestTransaction guestTransaction)
        {
            try
            {
                using var context = _contextFactory.CreateDbContext();
                await context.GuestTransactions.AddAsync(guestTransaction);
                await context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                throw new Exception("An error occurred when adding guest transaction. " + ex.Message);
            }
        }

        // Return list of guest transactions
        public async Task<List<GuestTransaction>> GetGuestTransactionsAsync(string guestId)
        {
            try
            {
                using var context = _contextFactory.CreateDbContext();
                var guestTransactions = await context.GuestTransactions
                    .Where(gt => gt.GuestId == guestId)
                    .OrderByDescending(gt => gt.Date)
                    .ToListAsync();
                return guestTransactions;
            }
            catch (Exception ex)
            {
                throw new Exception("An error occurred when retrieving guest transactions. " + ex.Message);
            }
        }

        // Get guest account by guest ID
        public async Task<GuestAccount> GetGuestAccountByGuestIdAsync(string guestId)
        {
            try
            {
                using var context = _contextFactory.CreateDbContext();
                var guestAccount = await context.GuestAccounts.FirstOrDefaultAsync(c => c.GuestId == guestId && !c.IsClosed);
                return guestAccount;
            }
            catch (Exception ex)
            {
                throw new Exception("An error occurred when retrieving guest account by ID. " + ex.Message);
            }
        }

        // Get guest account by ID
        public async Task<GuestAccount> GetGuestAccountByIdAsync(string id)
        {
            try
            {
                using var context = _contextFactory.CreateDbContext();
                var guestAccount = await context.GuestAccounts.FirstOrDefaultAsync(c => c.Id == id);
                return guestAccount;
            }
            catch (Exception ex)
            {
                throw new Exception("An error occurred when retrieving guest account by ID. " + ex.Message);
            }
        }

        // Update guest account
        public async Task UpdateGuestAccountAsync(GuestAccount guestAccount)
        {
            try
            {
                using var context = _contextFactory.CreateDbContext();
                context.Entry(guestAccount).State = EntityState.Modified;
                await context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                throw new Exception("An error occurred when updating guest account. " + ex.Message);
            }
        }
    }
}
