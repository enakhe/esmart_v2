using ESMART.Domain.Entities.FrontDesk;
using ESMART.Domain.ViewModels.FrontDesk;
using ESMART.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ESMART.Infrastructure.Repositories.FrontDesk
{
    public class GuestRepository
    {
        private readonly ApplicationDbContext _db;
        public GuestRepository(ApplicationDbContext db)
        {
            _db = db;
        }

        public async Task AddGuestAsync(Guest guest)
        {
            try
            {
                await _db.Guests.AddAsync(guest);
                await _db.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                throw new Exception("An error occurred when adding guest. " + ex.Message);
            }
        }

        public async Task<List<GuestViewModel>> GetAllGuestsAsync()
        {
            try
            {
                var allGuest = await _db.Guests
                    .Where(g => g.IsTrashed == false)
                    .OrderBy(g => g.FullName)
                    .Select(guest => new GuestViewModel
                    {
                        Id = guest.Id,
                        GuestId = guest.GuestId,
                        FullName = guest.Title + " " + guest.FullName,
                        Email = guest.Email,
                        PhoneNumber = guest.PhoneNumber,
                        City = guest.City,
                        State = guest.State,
                        Country = guest.Country,
                        CreatedBy = guest.ApplicationUser.FullName,
                        DateCreated = guest.DateCreated,
                        DateModified = guest.DateModified,
                    }).ToListAsync();

                return allGuest;
            }
            catch (Exception ex)
            {
                throw new Exception("An error occurred when retrieving guests. " + ex.Message);
            }
        }

        public async Task<Guest?> GetGuestByIdAsync(string id)
        {
            try
            {
                return await _db.Guests.FirstOrDefaultAsync(c => c.Id == id);
            }
            catch (Exception ex)
            {
                throw new Exception("An error occurred when retrieving guest by ID. " + ex.Message);
            }
        }

        public async Task UpdateGuestAsync(Guest guest)
        {
            try
            {
                _db.Entry(guest).State = EntityState.Modified;
                await _db.SaveChangesAsync();
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
                var guest = await _db.Guests.FirstOrDefaultAsync(c => c.Id == id);
                if (guest != null)
                {
                    guest.IsTrashed = true;
                    _db.Entry(guest).State = EntityState.Modified;
                    await _db.SaveChangesAsync();
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
                if (string.IsNullOrEmpty(keyword))
                {
                    throw new Exception("Keyword cannot be empty");
                }

                var searchGuest = await _db.Guests
                    .Where(c => (c.FullName.Contains(keyword) || c.Email.Contains(keyword) || c.PhoneNumber.Contains(keyword) || c.Street.Contains(keyword) || c.City.Contains(keyword) || c.State.Contains(keyword) || c.Country.Contains(keyword) || c.GuestId.Contains(keyword) || c.Company.Contains(keyword)) && c.IsTrashed == false)
                    .OrderBy(g => g.FullName)
                    .Select(guest => new GuestViewModel
                    {
                        Id = guest.Id,
                        GuestId = guest.GuestId,
                        FullName = guest.Title + " " + guest.FullName,
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
                var allGuest = await _db.Guests
                    .Where(g => g.IsTrashed == true)
                    .OrderBy(g => g.FullName)
                    .Select(guest => new GuestViewModel
                    {
                        Id = guest.Id,
                        GuestId = guest.GuestId,
                        FullName = guest.Title + " " + guest.FullName,
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

        public async Task<List<GuestBillViewModel>> GetGuestBillAsync(string guestId)
        {
            try
            {
                var guestBills = await _db.Transactions
                    .Where(t => t.GuestId == guestId && t.Status == "Un Paid")
                    .OrderBy(t => t.Date)
                    .Select(guest => new GuestBillViewModel
                    {
                        TransactionId = guest.TransactionId,
                        Guest = guest.Guest.FullName,
                        GuestPhoneNo = guest.Guest.PhoneNumber,
                        ServiceId = guest.ServiceId,
                        Date = guest.Date.ToString(),
                        Status = guest.Status,
                        Amount = guest.Amount.ToString(),
                        TotalAmount = guest.Amount,
                        Description = guest.Description,
                        Type = guest.Type,
                        BankAccount = guest.BankAccount,
                    }).ToListAsync();

                return guestBills;
            }
            catch (Exception ex)
            {
                throw new Exception("An error occurred when getting guest bills. " + ex.Message);
            }
        }


    }
}
