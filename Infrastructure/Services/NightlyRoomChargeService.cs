using ESMART.Application.Common.Dtos;
using ESMART.Domain.Entities.FrontDesk;
using ESMART.Domain.Enum;
using ESMART.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ESMART.Infrastructure.Services
{
    public class NightlyRoomChargeService(IDbContextFactory<ApplicationDbContext> contextFactory, ILogger<NightlyRoomChargeService> logger, GuestAccountService guestAccountService)
    {
        private readonly IDbContextFactory<ApplicationDbContext> _contextFactory = contextFactory;
        private readonly ILogger<NightlyRoomChargeService> _logger = logger;
        private readonly GuestAccountService _guestAccountService = guestAccountService;
        public async Task PostNightlyRoomChargesAsync()
        {
            using var context = await _contextFactory.CreateDbContextAsync();
            var today = DateTime.Today;

            var activeBookings = await context.Bookings
                .Include(b => b.RoomBookings)
                .Where(b => b.CheckOut > today && b.Status != BookingStatus.Completed)
                .ToListAsync();

            foreach (var booking in activeBookings)
            {
                foreach (var roomBooking in booking.RoomBookings)
                {
                    var nightsStayed = (today - roomBooking.CheckIn.Date).Days;
                    if (nightsStayed <= 0) continue;

                    var chargedNights = await context.RoomNightCharges
                        .Where(x => x.RoomBookingId == roomBooking.Id)
                        .Select(x => x.Night)
                        .ToListAsync();

                    for (int i = 0; i < nightsStayed; i++)
                    {
                        var night = roomBooking.CheckIn.Date.AddDays(i);
                        if (chargedNights.Contains(night)) continue;

                        var guestTransaction = new GuestTransactionDto
                        {
                            Amount = roomBooking.Rate,
                            ApplicationUserId = "system",
                            GuestAccountId = booking.GuestAccountId,
                            BankAccountId = booking.BankAccountId,
                            Discount = roomBooking.Discount,
                            GuestId = booking.GuestId,
                            RoomId = roomBooking.RoomId,
                            Consumer = roomBooking.OccupantName,
                            PaymentMethod = booking.PaymentMethod,
                            TransactionType = TransactionType.RoomCharge,
                            Tax = roomBooking.Tax,
                            Description = $"Room Charge (Inclusive of Inclusions)"
                        };

                        // NOTE: Replace these with proper DI-based calls if needed
                        await _guestAccountService.AddRoomChargeAsync(booking.GuestId, roomBooking.Rate, roomBooking.Discount, roomBooking.Tax, roomBooking.ServiceCharge);
                        await _guestAccountService.AddTransaction(booking.GuestId, guestTransaction);

                        context.RoomNightCharges.Add(new RoomNightCharge
                        {
                            RoomBookingId = roomBooking.Id,
                            Night = night
                        });

                        _logger.LogInformation($"Posted nightly charge for {night:dd MMM} on booking {booking.Id}");
                    }

                    await context.SaveChangesAsync();
                }
            }
        }
    }

}
