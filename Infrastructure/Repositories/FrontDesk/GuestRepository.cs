using ESMART.Domain.Entities;
using ESMART.Domain.ViewModels.FrontDesk;
using ESMART.Infrastructure.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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

        public s void AddGuest(Guest guest)
        {
            try
            {
                _db.Guests.Add(guest);
                _db.SaveChanges();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Exception Error", MessageBoxButtons.OK,
                            MessageBoxIcon.Error);
            }
        }

        public List<GuestViewModel> GetAllGuests()
        {
            try
            {
                var allGuest = from guest in _db.Guests.Where(g => g.IsTrashed == false).OrderBy(g => g.FullName)
                               select new GuestViewModel
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
                               };
                return allGuest.ToList();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Exception Error", MessageBoxButtons.OK,
                            MessageBoxIcon.Error);
            }
            return null;
        }

        public Guest GetGuestById(string Id)
        {
            try
            {
                return _db.Guests.FirstOrDefault(c => c.Id == Id);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Exception Error", MessageBoxButtons.OK,
                            MessageBoxIcon.Error);
            }
            return null;
        }

        public void UpdateGuest(Guest guest)
        {
            try
            {
                _db.Entry(guest).State = System.Data.Entity.EntityState.Modified;
                _db.SaveChanges();
                MessageBox.Show("Successfully edited guest information", "Success", MessageBoxButtons.OK,
                            MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Exception Error", MessageBoxButtons.OK,
                            MessageBoxIcon.Error);
            }
        }

        public void DeleteGuest(string Id)
        {
            try
            {
                var guest = _db.Guests.FirstOrDefault(c => c.Id == Id);
                if (guest != null)
                {
                    guest.IsTrashed = true;
                    _db.Entry(guest).State = System.Data.Entity.EntityState.Modified;
                    _db.SaveChanges();
                }
                else
                {
                    MessageBox.Show("Guest not found", "Not Found", MessageBoxButtons.OK,
                            MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Exception Error", MessageBoxButtons.OK,
                            MessageBoxIcon.Error);
            }
        }

        public List<GuestViewModel> SearchGuest(string keyword)
        {
            try
            {
                if (string.IsNullOrEmpty(keyword))
                {
                    MessageBox.Show("Keyword cannot be empty", "Invalid Entry", MessageBoxButtons.OK,
                            MessageBoxIcon.Error);
                }

                var searchGuest = from guest in _db.Guests.Where(c => c.FullName.Contains(keyword) || c.Email.Contains(keyword) || c.PhoneNumber.Contains(keyword) || c.Street.Contains(keyword) || c.City.Contains(keyword) || c.State.Contains(keyword) || c.Country.Contains(keyword) || c.GuestId.Contains(keyword) || c.Company.Contains(keyword) && c.IsTrashed == false).OrderBy(g => g.FullName)
                                  select new GuestViewModel
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
                                  };

                return searchGuest.ToList();
            }
            catch (Exception ex)
            {
                throw new Exception("An error occured when retrieving guest details", ex);
            }
        }

        public List<GuestViewModel> GetDeletedGuest()
        {
            try
            {
                var allGuest = from guest in _db.Guests.Where(g => g.IsTrashed == true).OrderBy(g => g.FullName)
                               select new GuestViewModel
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
                               };
                return allGuest.ToList();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Exception Error", MessageBoxButtons.OK,
                            MessageBoxIcon.Error);
            }
            return null;
        }

        public List<GuestBillViewModel> GetGuestBill(string guestId)
        {
            try
            {
                var guestBills = from guest in _db.Transactions.Where(t => t.GuestId == guestId && t.Status == "Un Paid").OrderBy(t => t.Date)
                                 select new GuestBillViewModel
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
                                 };

                return guestBills.ToList();
            }
            catch (Exception ex)
            {
                throw new Exception("An error occured when getting guest bills. " + ex.Message);
            }
        }
    }
}
