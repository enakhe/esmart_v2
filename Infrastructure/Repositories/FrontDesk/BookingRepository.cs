#nullable disable

using ESMART.Application.Common.Interface;
using ESMART.Domain.Entities.FrontDesk;
using ESMART.Domain.ViewModels.FrontDesk;
using ESMART.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace ESMART.Infrastructure.Repositories.FrontDesk
{
    public class BookingRepository(IDbContextFactory<ApplicationDbContext> contextFactory) : IBookingRepository
    {
        private readonly IDbContextFactory<ApplicationDbContext> _contextFactory = contextFactory;

        public async Task AddBooking(Booking booking)
        {
            try
            {
                using var context = _contextFactory.CreateDbContext();
                await context.Bookings.AddAsync(booking);
                await context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                throw new Exception("An error occurred when adding Booking. " + ex.Message);
            }
        }

        public async Task<List<BookingViewModel>> GetAllBookingsAsync()
        {
            try
            {
                using var context = _contextFactory.CreateDbContext();
                var allBookings = await context.Bookings
                                .Where(r => r.IsTrashed == false)
                                .Select(b => new BookingViewModel
                                {
                                    Id = b.Id,
                                    Guest = b.Guest.FullName,
                                    PhoneNumber = b.Guest.PhoneNumber,
                                    Room = b.Room.Number,
                                    CheckIn = b.CheckIn,
                                    CheckOut = b.CheckOut,
                                    PaymentMethod = b.PaymentMethod.ToString(),
                                    Duration = b.Duration.ToString(),
                                    Status = b.Status.ToString(),
                                    TotalAmount = b.TotalAmount.ToString("N2"),
                                    CreatedBy = b.ApplicationUser.FullName,
                                    DateCreated = b.DateCreated,
                                    DateModified = b.DateModified,
                                })
                                .OrderByDescending(r => r.DateCreated)
                                .ToListAsync();
                return allBookings;
            }
            catch (Exception ex)
            {
                throw new Exception("An error occurred when retrieving booking. " + ex.Message);
            }
        }

        public async Task<List<BookingViewModel>> IssueCard(string id)
        {
            try
            {
                using var context = _contextFactory.CreateDbContext();
                var allBookings = await context.Bookings
                                    .Include(b => b.ApplicationUser)
                                    .Include(b => b.Guest)
                                    .Include(b => b.Room)
                                    .Include(b => b.Transactions)
                                .Where(b => b.Id == id)
                                .Select(b => new BookingViewModel
                                {
                                    Id = b.Id,
                                    Guest = b.Guest.FullName,
                                    PhoneNumber = b.Guest.PhoneNumber,
                                    CheckIn = b.CheckIn,
                                    CheckOut = b.CheckOut,
                                    PaymentMethod = b.PaymentMethod.ToString(),
                                    Duration = b.Duration.ToString(),
                                    Status = b.Status.ToString(),
                                    TotalAmount = b.TotalAmount.ToString("N2"),
                                    CreatedBy = b.ApplicationUser.FullName,
                                    DateCreated = b.DateCreated,
                                    DateModified = b.DateModified,
                                })
                                .ToListAsync();
                return allBookings;
            }
            catch (Exception ex)
            {
                throw new Exception("An error occurred when retrieving booking. " + ex.Message);
            }
        }

        public async Task<List<BookingViewModel>> GetBookingsByFilterAsync(string roomTypeId, DateTime fromTime, DateTime endTime, bool IsTrashed)
        {
            try
            {
                using var context = _contextFactory.CreateDbContext();

                var bookings = await context.Bookings
                    .Include(b => b.Room)
                    .Where(b =>
                        b.Room.RoomTypeId == roomTypeId &&
                        b.CheckIn <= endTime &&
                        b.CheckOut >= fromTime &&
                        b.IsTrashed == IsTrashed
                    )
                    .Select(b => new BookingViewModel
                    {
                        Id = b.Id,
                        Guest = b.Guest.FullName,
                        PhoneNumber = b.Guest.PhoneNumber,
                        Room = b.Room.Number,
                        CheckIn = b.CheckIn,
                        CheckOut = b.CheckOut,
                        PaymentMethod = b.PaymentMethod.ToString(),
                        Duration = b.Duration.ToString(),
                        Status = b.Status.ToString(),
                        TotalAmount = b.TotalAmount.ToString("N2"),
                        CreatedBy = b.ApplicationUser.FullName,
                        DateCreated = b.DateCreated,
                        DateModified = b.DateModified,
                    })
                    .OrderBy(b => b.DateCreated)
                    .ToListAsync();

                return bookings;
            }
            catch (Exception ex)
            {
                throw new Exception("Failed to retrieve bookings by filter: " + ex.Message, ex);
            }
        }

        public async Task<List<BookingViewModel>> GetBookingByDate(DateTime fromTime, DateTime endTime, bool IsTrashed)
        {
            try
            {
                using var context = _contextFactory.CreateDbContext();

                var bookings = await context.Bookings
                    .Include(b => b.Room)
                    .Where(b =>
                        b.CheckIn <= endTime &&
                        b.CheckOut >= fromTime &&
                        b.IsTrashed == IsTrashed
                    )
                    .Select(b => new BookingViewModel
                    {
                        Id = b.Id,
                        Guest = b.Guest.FullName,
                        PhoneNumber = b.Guest.PhoneNumber,
                        Room = b.Room.Number,
                        CheckIn = b.CheckIn,
                        CheckOut = b.CheckOut,
                        PaymentMethod = b.PaymentMethod.ToString(),
                        Duration = b.Duration.ToString(),
                        Status = b.Status.ToString(),
                        TotalAmount = b.TotalAmount.ToString("N2"),
                        CreatedBy = b.ApplicationUser.FullName,
                        DateCreated = b.DateCreated,
                        DateModified = b.DateModified,
                    })
                    .OrderBy(b => b.DateCreated)
                    .ToListAsync();

                return bookings;
            }
            catch (Exception ex)
            {
                throw new Exception("Failed to retrieve bookings by date: " + ex.Message, ex);
            }
        }

        public async Task<List<BookingViewModel>> GetAllBookingByDate(DateTime fromTime, DateTime endTime)
        {
            try
            {
                using var context = _contextFactory.CreateDbContext();

                var bookings = await context.Bookings
                    .Include(b => b.Room)
                    .Where(b =>
                        b.CheckIn <= endTime &&
                        b.CheckOut >= fromTime
                    )
                    .Select(b => new BookingViewModel
                    {
                        Id = b.Id,
                        Guest = b.Guest.FullName,
                        PhoneNumber = b.Guest.PhoneNumber,
                        Room = b.Room.Number,
                        CheckIn = b.CheckIn,
                        CheckOut = b.CheckOut,
                        PaymentMethod = b.PaymentMethod.ToString(),
                        Duration = b.Duration.ToString(),
                        Status = b.Status.ToString(),
                        TotalAmount = b.TotalAmount.ToString("N2"),
                        CreatedBy = b.ApplicationUser.FullName,
                        DateCreated = b.DateCreated,
                        DateModified = b.DateModified,
                    })
                    .OrderBy(b => b.DateCreated)
                    .ToListAsync();

                return bookings;
            }
            catch (Exception ex)
            {
                throw new Exception("Failed to retrieve all bookings by date: " + ex.Message, ex);
            }
        }

        public async Task<List<BookingViewModel>> GetCheckedOutBookingByDate(DateTime fromTime, DateTime endTime)
        {
            try
            {
                using var context = _contextFactory.CreateDbContext();

                var bookings = await context.Bookings
                    .Include(b => b.Room)
                    .Where(b =>
                        b.CheckIn <= endTime &&
                        b.CheckOut >= fromTime &&
                        b.IsTrashed
                    )
                    .Select(b => new BookingViewModel
                    {
                        Id = b.Id,
                        Guest = b.Guest.FullName,
                        PhoneNumber = b.Guest.PhoneNumber,
                        Room = b.Room.Number,
                        CheckIn = b.CheckIn,
                        CheckOut = b.CheckOut,
                        PaymentMethod = b.PaymentMethod.ToString(),
                        Duration = b.Duration.ToString(),
                        Status = b.Status.ToString(),
                        TotalAmount = b.TotalAmount.ToString("N2"),
                        CreatedBy = b.ApplicationUser.FullName,
                        DateCreated = b.DateCreated,
                        DateModified = b.DateModified,
                    })
                    .OrderBy(b => b.DateCreated)
                    .ToListAsync();

                return bookings;
            }
            catch (Exception ex)
            {
                throw new Exception("Failed to retrieve checked out bookings by date: " + ex.Message, ex);
            }
        }

        public async Task<List<BookingViewModel>> GetRoomTypeBookingByFilter(string roomTypeId, DateTime fromTime, DateTime endTime)
        {
            try
            {
                using var context = _contextFactory.CreateDbContext();

                var bookings = await context.Bookings
                    .Include(b => b.Room)
                    .Where(b =>
                        b.Room.RoomTypeId == roomTypeId &&
                        b.CheckIn <= endTime &&
                        b.CheckOut >= fromTime
                    )
                    .Select(b => new BookingViewModel
                    {
                        Id = b.Id,
                        Guest = b.Guest.FullName,
                        PhoneNumber = b.Guest.PhoneNumber,
                        Room = b.Room.Number,
                        CheckIn = b.CheckIn,
                        CheckOut = b.CheckOut,
                        PaymentMethod = b.PaymentMethod.ToString(),
                        Duration = b.Duration.ToString(),
                        Status = b.Status.ToString(),
                        TotalAmount = b.TotalAmount.ToString("N2"),
                        CreatedBy = b.ApplicationUser.FullName,
                        DateCreated = b.DateCreated,
                        DateModified = b.DateModified,
                    })
                    .OrderBy(b => b.DateCreated)
                    .ToListAsync();

                return bookings;
            }
            catch (Exception ex)
            {
                throw new Exception("Failed to retrieve all bookings by date: " + ex.Message, ex);
            }
        }

        public async Task<Booking> GetBookingById(string id)
        {
            try
            {
                using var context = _contextFactory.CreateDbContext();
                var booking = await context.Bookings
                    .Include(b => b.Guest)
                    .Include(b => b.Room)
                    .Include(b => b.ApplicationUser)
                    .FirstOrDefaultAsync(b => b.Id == id);

                return booking;
            }
            catch (Exception ex)
            {
                throw new Exception("An error occurred when retrieving a booking with the provided ID. " + ex.Message);
            }
        }

        public async Task<BookingViewModel> GetBookingByIdViewModel(string id)
        {
            try
            {
                using var context = _contextFactory.CreateDbContext();
                var booking = await context.Bookings.Where(b => b.Id == id)
                    .Select(b => new BookingViewModel
                    {
                        Id = b.Id,
                        Guest = b.Guest.FullName,
                        PhoneNumber = b.Guest.PhoneNumber,
                        Room = b.Room.Number,
                        CheckIn = b.CheckIn,
                        CheckOut = b.CheckOut,
                        PaymentMethod = b.PaymentMethod.ToString(),
                        Duration = b.Duration.ToString(),
                        Status = b.Status.ToString(),
                        TotalAmount = b.TotalAmount.ToString("N2"),
                        CreatedBy = b.ApplicationUser.FullName,
                        DateCreated = b.DateCreated,
                        DateModified = b.DateModified,
                    }).FirstOrDefaultAsync();

                return booking;
            }
            catch (Exception ex)
            {
                throw new Exception("An error occurred when retrieving a booking with the provided ID. " + ex.Message);
            }
        }

        public async Task UpdateBooking(Booking booking)
        {
            try
            {
                using var context = _contextFactory.CreateDbContext();
                context.Entry(booking).State = EntityState.Modified;
                await context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                throw new Exception("An error occurred when updating booking. " + ex.Message);
            }
        }

        public async Task DeleteBooking(string id)
        {
            try
            {
                using var context = _contextFactory.CreateDbContext();
                var booking = await context.Bookings.FirstOrDefaultAsync(r => r.Id == id);
                booking.IsTrashed = true;

                await UpdateBooking(booking);
            }
            catch (Exception ex)
            {
                throw new Exception("An error occurred when deleting a room" + ex.Message);
            }
        }

        public async Task<List<BookingViewModel>> GetRecycledBookings()
        {
            try
            {
                using var context = _contextFactory.CreateDbContext();

                var bookings = await context.Bookings
                    .Include(b => b.Room)
                    .Where(b =>
                        b.IsTrashed
                    )
                    .Select(b => new BookingViewModel
                    {
                        Id = b.Id,
                        Guest = b.Guest.FullName,
                        PhoneNumber = b.Guest.PhoneNumber,
                        Room = b.Room.Number,
                        CheckIn = b.CheckIn,
                        CheckOut = b.CheckOut,
                        PaymentMethod = b.PaymentMethod.ToString(),
                        Duration = b.Duration.ToString(),
                        Status = b.Status.ToString(),
                        TotalAmount = b.TotalAmount.ToString("N2"),
                        CreatedBy = b.ApplicationUser.FullName,
                        DateCreated = b.DateCreated,
                        DateModified = b.DateModified,
                    })
                    .OrderBy(b => b.DateCreated)
                    .ToListAsync();

                return bookings;
            }
            catch (Exception ex)
            {
                throw new Exception("Failed to retrieve all recycled bookings: " + ex.Message, ex);
            }
        }

        public async Task<List<BookingViewModel>> SearchBooking(string keyword)
        {
            try
            {
                using var context = _contextFactory.CreateDbContext();

                var bookings = await context.Bookings
                    .Include(b => b.Room).Include(b => b.Guest)
                    .Where(b =>
                        !b.IsTrashed &&
                        b.Room.Number.Contains(keyword) ||
                        b.Guest.FullName.Contains(keyword) ||
                        b.Room.RoomType.Name.Contains(keyword)
                    )
                    .Select(b => new BookingViewModel
                    {
                        Id = b.Id,
                        Guest = b.Guest.FullName,
                        PhoneNumber = b.Guest.PhoneNumber,
                        Room = b.Room.Number,
                        CheckIn = b.CheckIn,
                        CheckOut = b.CheckOut,
                        PaymentMethod = b.PaymentMethod.ToString(),
                        Duration = b.Duration.ToString(),
                        Status = b.Status.ToString(),
                        TotalAmount = b.TotalAmount.ToString("N2"),
                        CreatedBy = b.ApplicationUser.FullName,
                        DateCreated = b.DateCreated,
                        DateModified = b.DateModified,
                    })
                    .OrderBy(b => b.DateCreated)
                    .ToListAsync();

                return bookings;
            }
            catch (Exception ex)
            {
                throw new Exception("Failed to retrieve all recycled bookings: " + ex.Message, ex);
            }
        }

        public int GetGuestBooking(string id)
        {
            try
            {
                using var context = _contextFactory.CreateDbContext();
                return context.Bookings.Where(b => b.GuestId == id).ToList().Count();
            }
            catch (Exception ex)
            {
                throw new Exception("Error retriving total number of Guest Booking", ex);
            }
        }

        public async Task<int> GetBookingNumber()
        {
            using var context = _contextFactory.CreateDbContext();
            return await context.Bookings.Where(b => b.IsTrashed == false).CountAsync();
        }
    }
}
