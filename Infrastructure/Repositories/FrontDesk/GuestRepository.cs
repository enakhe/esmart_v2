using ESMART.Application.Common.Models;
using ESMART.Application.Interface;
using ESMART.Domain.Entities.FrontDesk;
using ESMART.Domain.Enum;
using ESMART.Domain.ViewModels.FrontDesk;
using ESMART.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System.Globalization;

namespace ESMART.Infrastructure.Repositories.FrontDesk
{
    public class GuestRepository : IGuestRepository
    {
        private readonly ApplicationDbContext _db;
        public GuestRepository(ApplicationDbContext db)
        {
            _db = db;
        }

        public async Task<GuestResult> AddGuestAsync(Guest guest)
        {
            try
            {
                var result = await _db.Guests.AddAsync(guest);
                await _db.SaveChangesAsync();
                return GuestResult.Success(guest);
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
                await _db.GuestIdentities.AddAsync(guestIdentity);
                await _db.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                throw new Exception("An error occurred when adding guest identity information. " + ex.Message);
            }
        }

        public async Task<List<GuestViewModel>> GetAllGuestsAsync()
        {
            try
            {
                var allGuest = await _db.Guests
                    .Where(g => g.IsTrashed == false)
                    .OrderBy(g => g.FirstName)
                    .Select(guest => new GuestViewModel
                    {
                        Id = guest.Id,
                        GuestId = guest.GuestId,
                        FullName = $"{guest.FirstName} {guest.MiddleName} {guest.LastName}",
                        GuestImage = guest.GuestImage,
                        Email = guest.Email,
                        Status = guest.Status,
                        PhoneNumber = guest.PhoneNumber,
                        City = guest.City,
                        State = guest.State,
                        Country = guest.Country,
                        CreatedBy = guest.ApplicationUser.FullName,
                        DateCreated = guest.DateCreated.ToString("ddd d MMMM, yyyy", CultureInfo.InvariantCulture),
                        DateModified = guest.DateModified.ToString("ddd d MMMM, yyyy", CultureInfo.InvariantCulture),
                    }).ToListAsync();

                return allGuest;
            }
            catch (Exception ex)
            {
                throw new Exception("An error occurred when retrieving guests. " + ex.Message);
            }
        }

        public async Task<GuestResult> GetGuestByIdAsync(string id)
        {
            try
            {
                var guest = await _db.Guests.FirstOrDefaultAsync(c => c.Id == id);

                if (guest != null)
                    return GuestResult.Success(guest);

                return GuestResult.Failure(["Unable to find a guest with the provided ID"]);
            }
            catch (Exception ex)
            {
                throw new Exception("An error occurred when retrieving guest by ID. " + ex.Message);
            }
        }

        public async Task<GuestIdenityResult> GetGuestIdentityByGuestIdAsync(string id)
        {
            try
            {
                var guest = await _db.GuestIdentities.FirstOrDefaultAsync(c => c.GuestId == id);
                if (guest != null)
                    return GuestIdenityResult.Success(guest);
                IEnumerable<string> errors = new List<string> { "Unable to find a guest identity with the provided ID" };
                return GuestIdenityResult.Failure(errors);
            }
            catch (Exception ex)
            {
                throw new Exception("An error occurred when retrieving guest identity by ID. " + ex.Message);
            }
        }

        public async Task<GuestResult> UpdateGuestAsync(Guest guest)
        {
            try
            {
                _db.Entry(guest).State = EntityState.Modified;
                await _db.SaveChangesAsync();
                return GuestResult.Success(guest);
            }
            catch (Exception ex)
            {
                throw new Exception("An error occurred when updating guest. " + ex.Message);
            }
        }

        public async Task<GuestIdenityResult> UpdateGuestIdentityAsync(GuestIdentity guestIdentity)
        {
            try
            {
                _db.Entry(guestIdentity).State = EntityState.Modified;
                await _db.SaveChangesAsync();
                return GuestIdenityResult.Success(guestIdentity);
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
                        DateCreated = guest.DateCreated.ToString("ddd d MMMM, yyyy", CultureInfo.InvariantCulture),
                        DateModified = guest.DateModified.ToString("ddd d MMMM, yyyy", CultureInfo.InvariantCulture),
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
                        FullName = guest.FullName,
                        Email = guest.Email,
                        PhoneNumber = guest.PhoneNumber,
                        City = guest.City,
                        State = guest.State,
                        CreatedBy = guest.ApplicationUser.FullName,
                        DateCreated = guest.DateCreated.ToString("ddd d MMMM, yyyy", CultureInfo.InvariantCulture),
                        DateModified = guest.DateModified.ToString("ddd d MMMM, yyyy", CultureInfo.InvariantCulture),
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
                    .Where(t => t.GuestId == guestId && t.Status.ToString() == PaymentStatus.Pending.ToString())
                    .OrderBy(t => t.Date)
                    .Select(transaction => new GuestBillViewModel
                    {
                        TransactionId = transaction.TransactionId,
                        Guest = transaction.Guest.FullName,
                        GuestPhoneNo = transaction.Guest.PhoneNumber,
                        ServiceId = transaction.ServiceId,
                        Date = transaction.Date.ToString(),
                        Status = transaction.Status.ToString(),
                        Amount = transaction.Amount.ToString(),
                        TaxAmount = transaction.TaxAmount.ToString(),
                        ServiceCharge = transaction.ServiceCharge.ToString(),
                        Discount = transaction.Discount.ToString(),
                        InvoiceNumber = transaction.InvoiceNumber,
                        CreatedBy = transaction.ApplicationUser.FirstName,
                        TotalAmount = transaction.TotalAmount,
                        Description = transaction.Description,
                        Type = transaction.Type.ToString(),
                        BankAccount = transaction.BankAccount,
                    }).ToListAsync();

                return guestBills;
            }
            catch (Exception ex)
            {
                throw new Exception("An error occurred when getting guest bills. " + ex.Message);
            }
        }

        public async Task<List<GuestBillViewModel>> GetGuestBillByDateAsync(string guestId, DateTime startDate, DateTime endDate)
        {
            try
            {
                var guestBills = await _db.Transactions
                    .Where(t => t.GuestId == guestId && t.Date >= startDate && t.Date <= endDate && t.Status.ToString() == PaymentStatus.Pending.ToString())
                    .OrderBy(t => t.Date)
                    .Select(transaction => new GuestBillViewModel
                    {
                        TransactionId = transaction.TransactionId,
                        Guest = transaction.Guest.FullName,
                        GuestPhoneNo = transaction.Guest.PhoneNumber,
                        ServiceId = transaction.ServiceId,
                        Date = transaction.Date.ToString(),
                        Status = transaction.Status.ToString(),
                        Amount = transaction.Amount.ToString(),
                        TaxAmount = transaction.TaxAmount.ToString(),
                        ServiceCharge = transaction.ServiceCharge.ToString(),
                        Discount = transaction.Discount.ToString(),
                        InvoiceNumber = transaction.InvoiceNumber,
                        CreatedBy = transaction.ApplicationUser.FirstName,
                        TotalAmount = transaction.TotalAmount,
                        Description = transaction.Description,
                        Type = transaction.Type.ToString(),
                        BankAccount = transaction.BankAccount,
                    }).ToListAsync();
                return guestBills;
            }
            catch (Exception ex)
            {
                throw new Exception("An error occurred when getting guest bills by date. " + ex.Message);
            }
        }

    }
}
