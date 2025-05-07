#nullable disable

using ESMART.Application.Common.Interface;
using ESMART.Domain.Entities.FrontDesk;
using ESMART.Domain.Enum;
using ESMART.Domain.ViewModels.FrontDesk;
using ESMART.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ESMART.Infrastructure.Repositories.FrontDesk
{
    public class ReservationRepository(IDbContextFactory<ApplicationDbContext> contextFactory) : IReservationRepository
    {
        private readonly IDbContextFactory<ApplicationDbContext> _contextFactory = contextFactory;

        // Add Reservation
        public async Task AddReservationAsync(Reservation reservation)
        {
            try
            {
                using var context = _contextFactory.CreateDbContext();
                await context.Reservations.AddAsync(reservation);
                await context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                throw new Exception("An error occurred when adding reservation. " + ex.Message);
            }
        }

        // Get Reservation by Id
        public async Task<Reservation> GetReservationByIdAsync(string id)
        {
            try
            {
                using var context = _contextFactory.CreateDbContext();
                return await context.Reservations
                    .Include(r => r.Guest)
                    .Include(r => r.Room)
                    .FirstOrDefaultAsync(r => r.Id == id);
            }
            catch (Exception ex)
            {
                throw new Exception("An error occurred when retrieving reservation. " + ex.Message);
            }
        }

        // Get All Reservations
        public async Task<List<ReservationViewModel>> GetAllReservationsAsync()
        {
            try
            {
                using var context = _contextFactory.CreateDbContext();
                return await context.Reservations
                    .Include(r => r.Guest)
                    .Include(r => r.Room)
                    .Where(r => r.Status != ReservationStatus.Cancelled)
                    .Select(r => new ReservationViewModel
                    {
                        Id = r.Id,
                        Guest = r.Guest.FullName,
                        PhoneNumber = r.Guest.PhoneNumber,
                        Room = r.Room.Number,
                        ArrivalDate = r.ArrivateDate,
                        DepartureDate = r.DepartureDate,
                        PaymentMethod = r.PaymentMethod.ToString(),
                        Duration = r.Duration,
                        Status = r.Status.ToString(),
                        PaymentStatus = r.TransactionStatus.ToString(),
                        TotalAmount = r.TotalAmount,
                        AmountPaid = r.AmountPaid,
                        Receivables = r.TotalAmount - r.AmountPaid,
                        CreatedBy = r.ApplicationUser.UserName,
                        DateCreated = r.DateAdded,
                        DateModified = r.DateModified
                    })
                    .OrderBy(r => r.DateCreated)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                throw new Exception("An error occurred when retrieving reservations. " + ex.Message);
            }
        }

        // Update Reservation
        public async Task UpdateReservationAsync(Reservation reservation)
        {
            try
            {
                using var context = _contextFactory.CreateDbContext();
                context.Reservations.Update(reservation);
                await context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                throw new Exception("An error occurred when updating reservation. " + ex.Message);
            }
        }

        // Delete Reservation
        public async Task DeleteReservationAsync(string id)
        {
            try
            {
                using var context = _contextFactory.CreateDbContext();
                var reservation = await context.Reservations.FindAsync(id);
                if (reservation != null)
                {
                    context.Reservations.Remove(reservation);
                    await context.SaveChangesAsync();
                }
            }
            catch (Exception ex)
            {
                throw new Exception("An error occurred when deleting reservation. " + ex.Message);
            }
        }

        // Get Reservations by Guest Id
        public async Task<List<ReservationViewModel>> GetReservationsByGuestIdAsync(string guestId)
        {
            try
            {
                using var context = _contextFactory.CreateDbContext();
                return await context.Reservations
                    .Where(r => r.GuestId == guestId && r.Status != ReservationStatus.Cancelled)
                    .Select(r => new ReservationViewModel
                    {
                        Id = r.Id,
                        Guest = r.Guest.FullName,
                        PhoneNumber = r.Guest.PhoneNumber,
                        Room = r.Room.Number,
                        ArrivalDate = r.ArrivateDate,
                        DepartureDate = r.DepartureDate,
                        PaymentMethod = r.PaymentMethod.ToString(),
                        Duration = r.Duration,
                        Status = r.Status.ToString(),
                        PaymentStatus = r.TransactionStatus.ToString(),
                        TotalAmount = r.TotalAmount,
                        AmountPaid = r.AmountPaid,
                        Receivables = r.TotalAmount - r.AmountPaid,
                        CreatedBy = r.ApplicationUser.UserName,
                        DateCreated = r.DateAdded,
                        DateModified = r.DateModified
                    })
                    .OrderBy(r => r.DateCreated)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                throw new Exception("An error occurred when retrieving reservations by guest ID. " + ex.Message);
            }
        }

        // Get Reservations by Room Id
        public async Task<List<ReservationViewModel>> GetReservationsByRoomIdAsync(string roomId)
        {
            try
            {
                using var context = _contextFactory.CreateDbContext();
                return await context.Reservations
                    .Where(r => r.RoomId == roomId && r.Status != ReservationStatus.Cancelled)
                    .Select(r => new ReservationViewModel
                    {
                        Id = r.Id,
                        Guest = r.Guest.FullName,
                        PhoneNumber = r.Guest.PhoneNumber,
                        Room = r.Room.Number,
                        ArrivalDate = r.ArrivateDate,
                        DepartureDate = r.DepartureDate,
                        PaymentMethod = r.PaymentMethod.ToString(),
                        Duration = r.Duration,
                        Status = r.Status.ToString(),
                        PaymentStatus = r.TransactionStatus.ToString(),
                        TotalAmount = r.TotalAmount,
                        AmountPaid = r.AmountPaid,
                        Receivables = r.TotalAmount - r.AmountPaid,
                        CreatedBy = r.ApplicationUser.UserName,
                        DateCreated = r.DateAdded,
                        DateModified = r.DateModified
                    })
                    .OrderBy(r => r.DateCreated)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                throw new Exception("An error occurred when retrieving reservations by room ID. " + ex.Message);
            }
        }

        // Get Reservations by Application User Id
        public async Task<List<ReservationViewModel>> GetReservationsByApplicationUserIdAsync(string applicationUserId)
        {
            try
            {
                using var context = _contextFactory.CreateDbContext();
                return await context.Reservations
                    .Where(r => r.ApplicationUserId == applicationUserId && r.Status != ReservationStatus.Cancelled)
                    .Select(r => new ReservationViewModel
                    {
                        Id = r.Id,
                        Guest = r.Guest.FullName,
                        PhoneNumber = r.Guest.PhoneNumber,
                        Room = r.Room.Number,
                        ArrivalDate = r.ArrivateDate,
                        DepartureDate = r.DepartureDate,
                        PaymentMethod = r.PaymentMethod.ToString(),
                        Duration = r.Duration,
                        Status = r.Status.ToString(),
                        PaymentStatus = r.TransactionStatus.ToString(),
                        TotalAmount = r.TotalAmount,
                        AmountPaid = r.AmountPaid,
                        Receivables = r.TotalAmount - r.AmountPaid,
                        CreatedBy = r.ApplicationUser.UserName,
                        DateCreated = r.DateAdded,
                        DateModified = r.DateModified
                    })
                    .OrderBy(r => r.DateCreated)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                throw new Exception("An error occurred when retrieving reservations by application user ID. " + ex.Message);
            }
        }

        // Get Reservations by Date Range
        public async Task<List<ReservationViewModel>> GetReservationsByDateRangeAsync(DateTime startDate, DateTime endDate)
        {
            try
            {
                using var context = _contextFactory.CreateDbContext();
                return await context.Reservations
                    .Where(r => r.ArrivateDate >= startDate && r.DepartureDate <= endDate && r.Status != ReservationStatus.Cancelled)
                    .Select(r => new ReservationViewModel
                    {
                        Id = r.Id,
                        Guest = r.Guest.FullName,
                        PhoneNumber = r.Guest.PhoneNumber,
                        Room = r.Room.Number,
                        ArrivalDate = r.ArrivateDate,
                        DepartureDate = r.DepartureDate,
                        PaymentMethod = r.PaymentMethod.ToString(),
                        Duration = r.Duration,
                        Status = r.Status.ToString(),
                        PaymentStatus = r.TransactionStatus.ToString(),
                        TotalAmount = r.TotalAmount,
                        AmountPaid = r.AmountPaid,
                        Receivables = r.TotalAmount - r.AmountPaid,
                        CreatedBy = r.ApplicationUser.UserName,
                        DateCreated = r.DateAdded,
                        DateModified = r.DateModified
                    })
                    .OrderBy(r => r.DateCreated)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                throw new Exception("An error occurred when retrieving reservations by date range. " + ex.Message);
            }
        }

        // Get Reservation from room no and date range
        public async Task<List<ReservationViewModel>> GetReservationsByRoomNoAndDateRangeAsync(string roomNo, DateTime startDate, DateTime endDate)
        {
            try
            {
                using var context = _contextFactory.CreateDbContext();
                return await context.Reservations
                    .Where(r => r.Room.Number == roomNo && r.ArrivateDate >= startDate && r.DepartureDate <= endDate && r.Status != ReservationStatus.Cancelled)
                    .Select(r => new ReservationViewModel
                    {
                        Id = r.Id,
                        Guest = r.Guest.FullName,
                        PhoneNumber = r.Guest.PhoneNumber,
                        Room = r.Room.Number,
                        ArrivalDate = r.ArrivateDate,
                        DepartureDate = r.DepartureDate,
                        PaymentMethod = r.PaymentMethod.ToString(),
                        Duration = r.Duration,
                        Status = r.Status.ToString(),
                        PaymentStatus = r.TransactionStatus.ToString(),
                        TotalAmount = r.TotalAmount,
                        AmountPaid = r.AmountPaid,
                        Receivables = r.TotalAmount - r.AmountPaid,
                        CreatedBy = r.ApplicationUser.UserName,
                        DateCreated = r.DateAdded,
                        DateModified = r.DateModified
                    })
                    .OrderBy(r => r.DateCreated)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                throw new Exception("An error occurred when retrieving reservations by room number and date range. " + ex.Message);
            }
        }

        // Get Reservations by Status
        public async Task<List<ReservationViewModel>> GetReservationsByStatusAsync(ReservationStatus status)
        {
            try
            {
                using var context = _contextFactory.CreateDbContext();
                return await context.Reservations
                    .Where(r => r.Status == status && r.Status != ReservationStatus.Cancelled)
                    .Select(r => new ReservationViewModel
                    {
                        Id = r.Id,
                        Guest = r.Guest.FullName,
                        PhoneNumber = r.Guest.PhoneNumber,
                        Room = r.Room.Number,
                        ArrivalDate = r.ArrivateDate,
                        DepartureDate = r.DepartureDate,
                        PaymentMethod = r.PaymentMethod.ToString(),
                        Duration = r.Duration,
                        Status = r.Status.ToString(),
                        PaymentStatus = r.TransactionStatus.ToString(),
                        TotalAmount = r.TotalAmount,
                        AmountPaid = r.AmountPaid,
                        Receivables = r.TotalAmount - r.AmountPaid,
                        CreatedBy = r.ApplicationUser.UserName,
                        DateCreated = r.DateAdded,
                        DateModified = r.DateModified
                    })
                    .OrderBy(r => r.DateCreated)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                throw new Exception("An error occurred when retrieving reservations by status. " + ex.Message);
            }
        }

        // Get Reservations by Payment Status
        public async Task<List<ReservationViewModel>> GetReservationsByPaymentStatusAsync(TransactionStatus paymentStatus)
        {
            try
            {
                using var context = _contextFactory.CreateDbContext();
                return await context.Reservations
                    .Where(r => r.TransactionStatus == paymentStatus && r.Status != ReservationStatus.Cancelled)
                    .Select(r => new ReservationViewModel
                    {
                        Id = r.Id,
                        Guest = r.Guest.FullName,
                        PhoneNumber = r.Guest.PhoneNumber,
                        Room = r.Room.Number,
                        ArrivalDate = r.ArrivateDate,
                        DepartureDate = r.DepartureDate,
                        PaymentMethod = r.PaymentMethod.ToString(),
                        Duration = r.Duration,
                        Status = r.Status.ToString(),
                        PaymentStatus = r.TransactionStatus.ToString(),
                        TotalAmount = r.TotalAmount,
                        AmountPaid = r.AmountPaid,
                        Receivables = r.TotalAmount - r.AmountPaid,
                        CreatedBy = r.ApplicationUser.UserName,
                        DateCreated = r.DateAdded,
                        DateModified = r.DateModified
                    })
                    .OrderBy(r => r.DateCreated)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                throw new Exception("An error occurred when retrieving reservations by payment status. " + ex.Message);
            }
        }

        // Get cancelled reservation
        public async Task<List<ReservationViewModel>> GetCancelledReservationsAsync()
        {
            try
            {
                using var context = _contextFactory.CreateDbContext();
                return await context.Reservations
                    .Where(r => r.Status == ReservationStatus.Cancelled)
                    .Select(r => new ReservationViewModel
                    {
                        Id = r.Id,
                        Guest = r.Guest.FullName,
                        PhoneNumber = r.Guest.PhoneNumber,
                        Room = r.Room.Number,
                        ArrivalDate = r.ArrivateDate,
                        DepartureDate = r.DepartureDate,
                        PaymentMethod = r.PaymentMethod.ToString(),
                        Duration = r.Duration,
                        Status = r.Status.ToString(),
                        PaymentStatus = r.TransactionStatus.ToString(),
                        TotalAmount = r.TotalAmount,
                        AmountPaid = r.AmountPaid,
                        Receivables = r.TotalAmount - r.AmountPaid,
                        CreatedBy = r.ApplicationUser.UserName,
                        DateCreated = r.DateAdded,
                        DateModified = r.DateModified
                    })
                    .OrderBy(r => r.DateCreated)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                throw new Exception("An error occurred when retrieving cancelled reservations. " + ex.Message);
            }
        }
    }
}
