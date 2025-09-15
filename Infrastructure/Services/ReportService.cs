using ESMART.Application.Common.Dtos;
using ESMART.Domain.Enum;
using ESMART.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ESMART.Infrastructure.Services
{
    public class ReportService(IDbContextFactory<ApplicationDbContext> contextFactory)
    {
        private readonly IDbContextFactory<ApplicationDbContext> _contextFactory = contextFactory;

        public async Task<HouseListDto> GetHouseListReportAsync()
        {
            using var _context = await _contextFactory.CreateDbContextAsync();

            var today = DateTime.Today;
            var firstDayOfMonth = new DateTime(today.Year, today.Month, 1);
            var firstDayOfYear = new DateTime(today.Year, 1, 1);

            // Fetch all required data efficiently
            var roomBookings = await _context.RoomBookings
                .Include(rb => rb.Room)
                .Include(rb => rb.Booking)
                .Include(rb => rb.Booking.Guest)
                .Include(rb => rb.Booking.GuestTransactions)
                .Where(rb => rb.Booking.Status == BookingStatus.Active)
                .ToListAsync();

            // Map In-House Guests data
            var inHouseGuests = roomBookings.Select((rb, index) => new InHouseGuest
            {
                SerialNumber = index + 1,
                GuestName = rb.Booking.Guest.FullName,
                RoomNumber = rb.Room.Number,
                CheckInDate = rb.CheckIn,
                CheckOutDate = rb.CheckOut
            }).ToList();

            // Calculate Revenue
            var revenueToday = _context.GuestTransactions
                .Where(t => t.TransactionType == TransactionType.RoomCharge && t.Date.Date == today)
                .Sum(t => (decimal?)t.Room.Rate) ?? 0;

            var revenueThisMonth = _context.GuestTransactions
                .Where(t => t.TransactionType == TransactionType.RoomCharge && t.Date >= firstDayOfMonth)
                .Sum(t => (decimal?)t.Room.Rate) ?? 0;

            var revenueThisYear = _context.GuestTransactions
                .Where(t => t.TransactionType == TransactionType.RoomCharge && t.Date >= firstDayOfYear)
                .Sum(t => (decimal?)t.Room.Rate) ?? 0;

            // Final DTO mapping
            var houseList = new HouseListDto
            {
                InHouseGuests = inHouseGuests,
                RevenueToday = revenueToday,
                RevenueThisMonth = revenueThisMonth,
                RevenueThisYear = revenueThisYear,

                // Booking Counts
                CurrentGuestsInHouse = roomBookings.Count,
                BookingsThisMonth = _context.RoomBookings.Count(b => b.CheckIn.Month == today.Month && b.CheckIn.Year == today.Year),
                BookingsThisYear = _context.RoomBookings.Count(b => b.CheckIn.Year == today.Year)
            };

            return houseList;
        }


    }
}
