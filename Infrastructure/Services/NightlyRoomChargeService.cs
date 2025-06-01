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

            var activeRoomBookings = await context.RoomBookings
                .Include(rb => rb.Booking)
                .Where(rb => rb.Booking.Status == BookingStatus.Active)
                .ToListAsync();


            foreach (var roomBooking in activeRoomBookings)
            {
                var lastChargedNight = await context.RoomNightCharges
                    .Where(rnc => rnc.RoomBookingId == roomBooking.Id)
                    .OrderByDescending(rnc => rnc.Night)
                    .Select(rnc => rnc.Night)
                    .FirstOrDefaultAsync();

                var nextChargeNight = lastChargedNight == default ? roomBooking.CheckIn.Date : lastChargedNight.AddDays(1);

                if (nextChargeNight < today)
                {
                    var guestTransaction = new GuestTransactionDto
                    {
                        Amount = roomBooking.Rate,
                        ApplicationUserId = roomBooking.Booking.ApplicationUserId,
                        GuestAccountId = roomBooking.Booking.GuestAccountId,
                        BankAccountId = roomBooking.Booking.BankAccountId,
                        Discount = roomBooking.Discount,
                        GuestId = roomBooking.Booking.GuestId,
                        BookingId = roomBooking.Booking.Id,
                        RoomId = roomBooking.RoomId,
                        Consumer = roomBooking.OccupantName,
                        PaymentMethod = roomBooking.Booking.PaymentMethod,
                        TransactionType = TransactionType.RoomCharge,
                        Tax = roomBooking.Tax,
                        Description = $"Room Charge (Inclusive of Inclusions)"
                    };

                    await _guestAccountService.AddRoomChargeAsync(roomBooking.Booking.GuestId, roomBooking.Rate, roomBooking.Discount, roomBooking.Tax, roomBooking.ServiceCharge);
                    await _guestAccountService.AddTransaction(roomBooking.Booking.GuestId, guestTransaction);
                    context.RoomNightCharges.Add(new RoomNightCharge { RoomBookingId = roomBooking.Id, Night = nextChargeNight });

                    _logger.LogInformation($"Nightly charge posted for {nextChargeNight:dd MMM} (Booking ID: {roomBooking.Booking.Id})");
                }
            }
            await context.SaveChangesAsync();
        }
    }

}