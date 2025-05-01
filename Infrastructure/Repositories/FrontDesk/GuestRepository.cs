using ESMART.Application.Common.Interface;
using ESMART.Application.Common.Models;
using ESMART.Domain.Entities.FrontDesk;
using ESMART.Domain.ViewModels.FrontDesk;
using ESMART.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System.Globalization;

namespace ESMART.Infrastructure.Repositories.FrontDesk
{
    public class GuestRepository(IDbContextFactory<ApplicationDbContext> contextFactory) : IGuestRepository
    {
        private readonly IDbContextFactory<ApplicationDbContext> _contextFactory = contextFactory;

        public async Task<GuestResult> AddGuestAsync(Guest guest)
        {
            try
            {
                using var context = _contextFactory.CreateDbContext();
                var result = await context.Guests.AddAsync(guest);
                await context.SaveChangesAsync();
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

        public async Task<GuestResult> GetGuestByIdAsync(string id)
        {
            try
            {
                using var context = _contextFactory.CreateDbContext();
                var guest = await context.Guests.FirstOrDefaultAsync(c => c.Id == id);

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
                using var context = _contextFactory.CreateDbContext();
                var guest = await context.GuestIdentities.FirstOrDefaultAsync(c => c.GuestId == id);
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
                using var context = _contextFactory.CreateDbContext();
                context.Entry(guest).State = EntityState.Modified;
                await context.SaveChangesAsync();
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
                using var context = _contextFactory.CreateDbContext();
                context.Entry(guestIdentity).State = EntityState.Modified;
                await context.SaveChangesAsync();
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
                if (string.IsNullOrEmpty(keyword))
                {
                    throw new Exception("Keyword cannot be empty");
                }

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

        //public async Task<List<GuestBillViewModel>> GetGuestBillAsync(string guestId)
        //{
        //    try
        //    {
        //        using var context = _contextFactory.CreateDbContext();
        //        var guestBills = await context.Transactions
        //            .Where(t => t.GuestId == guestId && t.TransactionItems.Status.ToString() == PaymentStatus.Pending.ToString())
        //            .OrderBy(t => t.Date)
        //            .Select(transaction => new GuestBillViewModel
        //            {
        //                TransactionId = transaction.TransactionId,
        //                Guest = transaction.Guest.FullName,
        //                GuestPhoneNo = transaction.Guest.PhoneNumber,
        //                ServiceId = transaction.ServiceId,
        //                Date = transaction.Date.ToString(),
        //                Status = transaction.Status.ToString(),
        //                Amount = transaction.Amount.ToString(),
        //                TaxAmount = transaction.TaxAmount.ToString(),
        //                ServiceCharge = transaction.ServiceCharge.ToString(),
        //                Discount = transaction.Discount.ToString(),
        //                InvoiceNumber = transaction.InvoiceNumber,
        //                CreatedBy = transaction.ApplicationUser.FirstName,
        //                TotalAmount = transaction.TotalAmount,
        //                Description = transaction.Description,
        //                Type = transaction.Type.ToString(),
        //                BankAccount = transaction.BankAccount,
        //            }).ToListAsync();

        //        return guestBills;
        //    }
        //    catch (Exception ex)
        //    {
        //        throw new Exception("An error occurred when getting guest bills. " + ex.Message);
        //    }
        //}

        //public async Task<List<GuestBillViewModel>> GetGuestBillByDateAsync(string guestId, DateTime startDate, DateTime endDate)
        //{
        //    try
        //    {
        //        using var context = _contextFactory.CreateDbContext();
        //        var guestBills = await context.Transactions
        //            .Where(t => t.GuestId == guestId && t.Date >= startDate && t.Date <= endDate && t.Status.ToString() == PaymentStatus.Pending.ToString())
        //            .OrderBy(t => t.Date)
        //            .Select(transaction => new GuestBillViewModel
        //            {
        //                TransactionId = transaction.TransactionId,
        //                Guest = transaction.Guest.FullName,
        //                GuestPhoneNo = transaction.Guest.PhoneNumber,
        //                ServiceId = transaction.ServiceId,
        //                Date = transaction.Date.ToString(),
        //                Status = transaction.Status.ToString(),
        //                Amount = transaction.Amount.ToString(),
        //                TaxAmount = transaction.TaxAmount.ToString(),
        //                ServiceCharge = transaction.ServiceCharge.ToString(),
        //                Discount = transaction.Discount.ToString(),
        //                InvoiceNumber = transaction.InvoiceNumber,
        //                CreatedBy = transaction.ApplicationUser.FirstName,
        //                TotalAmount = transaction.TotalAmount,
        //                Description = transaction.Description,
        //                Type = transaction.Type.ToString(),
        //                BankAccount = transaction.BankAccount,
        //            }).ToListAsync();
        //        return guestBills;
        //    }
        //    catch (Exception ex)
        //    {
        //        throw new Exception("An error occurred when getting guest bills by date. " + ex.Message);
        //    }
        //}

    }
}
