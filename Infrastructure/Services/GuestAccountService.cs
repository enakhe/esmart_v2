using ESMART.Application.Common.Dtos;
using ESMART.Application.Common.Interface;
using ESMART.Application.Common.Models;
using ESMART.Application.Common.Utils;
using ESMART.Domain.Entities.FrontDesk;
using ESMART.Domain.Entities.StoreKeeping;
using ESMART.Domain.Entities.Transaction;
using ESMART.Domain.Enum;
using ESMART.Domain.ViewModels.FrontDesk;
using ESMART.Infrastructure.Data;
using ESMART.Infrastructure.Repositories.StockKeeping;
using Google.Apis.Drive.v3.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ESMART.Infrastructure.Services
{
    public class GuestAccountService(IDbContextFactory<ApplicationDbContext> contextFactory, IRoomRepository roomRepository)
    {
        private readonly IDbContextFactory<ApplicationDbContext> _contextFactory = contextFactory;
        private readonly IRoomRepository _roomRepository = roomRepository;

        // OpenOrGetActiveGuestAccountAsync
        public async Task<GuestAccount> OpenOrGetActiveGuestAccountAsync(
            string guestId)
        {
            using var context = await _contextFactory.CreateDbContextAsync();
            var guestAccount = await GetAccountAsync(guestId);

            if (guestAccount == null)
            {
                guestAccount = new GuestAccount
                {
                    GuestId = guestId,
                    FundedBalance = 0,
                    TopUps = 0,
                    DirectPayments = 0,
                    Discount = 0,
                    Tax = 0,
                    OtherCharges = 0,
                    LastFunded = DateTime.UtcNow,
                    IsClosed = false,
                    AllowBarAndRes = true,
                    AllowLaundry = true,
                    Invoice = Helper.GenerateInvoiceNumber("GA")
                };

                context.GuestAccounts.Add(guestAccount);
                await context.SaveChangesAsync();
            }

            return guestAccount;
        }


        public async Task<GuestTransaction> AddTransaction(
            string guestId, GuestTransactionDto dto)
        {
            using var context = await _contextFactory.CreateDbContextAsync();
            var guestAccount = await GetAccountAsync(guestId);

            var guestTransaction = new GuestTransaction
            {
                Date = DateTime.Now,
                Description = dto.Description,
                Invoice = guestAccount.Invoice,
                Discount = dto.Discount,
                BillPosts = dto.Amount,
                Consumer = dto.Consumer,
                RoomId = dto.RoomId,
                GuestAccountId = dto.GuestAccountId,
                GuestId = guestAccount.GuestId,
                PaymentMethod = dto.PaymentMethod,
                BankAccountId = dto.BankAccountId,
                TransactionType = dto.TransactionType,
                ApplicationUserId = dto.ApplicationUserId
            };

            context.GuestTransactions.Add(guestTransaction);
            await context.SaveChangesAsync();

            return guestTransaction;
        }

        //public async Task<GuestTransaction> AddOtherChargeTransaction(
        //    string guestId, GuestTransactionDto dto)
        //{
        //    using var context = await _contextFactory.CreateDbContextAsync();
        //    var guestAccount = await GetAccountAsync(guestId);

        //    var guestTransaction = new GuestTransaction
        //    {
        //        Date = DateTime.Now,
        //        Description = dto.Description,
        //        Invoice = guestAccount.Invoice,
        //        Discount = dto.Discount,
        //        BillPosts = dto.Amount,
        //        Consumer = dto.Consumer,
        //        RoomId = dto.RoomId,
        //        GuestAccountId = dto.GuestAccountId,
        //        GuestId = guestAccount.GuestId,
        //        PaymentMethod = dto.PaymentMethod,
        //        BankAccountId = dto.BankAccountId,
        //        TransactionType = dto.TransactionType,
        //        ApplicationUserId = dto.ApplicationUserId
        //    };

        //    context.GuestTransactions.Add(guestTransaction);
        //    await context.SaveChangesAsync();

        //    return guestTransaction;
        //}


        public async Task<GuestAccount> GetAccountAsync(
            string guestId)
        {
            using var context = await _contextFactory.CreateDbContextAsync();
            var guestAccount = await context.GuestAccounts
                .Include(account => account.Guest)
                .Include(account => account.Transactions)
                .Include(account => account.BookingDetails)
                .FirstOrDefaultAsync(x => x.GuestId == guestId && !x.IsClosed);

            return guestAccount!;
        }


        // Get all guest accounts for a specific guest
        public async Task<IEnumerable<GuestAccount>> GetAllAccountsAsync(
            string guestId)
        {
            using var context = await _contextFactory.CreateDbContextAsync();
            var guestAccounts = await context.GuestAccounts
                .Where(x => x.GuestId == guestId)
                .Include(account => account.Guest)
                .Include(account => account.Transactions)
                .Include(account => account.BookingDetails)
                .ToListAsync();

            return guestAccounts;
        }


        // Get all guest accounts for a specific guest, including closed accounts
        public async Task<IEnumerable<GuestAccount>> GetAllAccountsIncludingClosedAsync(
            string guestId)
        {
            using var context = await _contextFactory.CreateDbContextAsync();
            var guestAccounts = await context.GuestAccounts
                .Where(x => x.GuestId == guestId)
                .Include(account => account.Guest)
                .Include(account => account.Transactions)
                .Include(account => account.BookingDetails)
                .ToListAsync();
            return guestAccounts;
        }


        // Get a guest account by ID
        public async Task<GuestAccount> GetAccountByIdAsync(
            string accountId)
        {
            using var context = await _contextFactory.CreateDbContextAsync();
            var guestAccount = await context.GuestAccounts
                .Include(accountId => accountId.Guest)
                .Include(accountId => accountId.Transactions)
                .Include(accountId => accountId.BookingDetails)
                .Where(accountId => !accountId.IsClosed)
                .FirstOrDefaultAsync(x => x.Id == accountId);

            return guestAccount ?? throw new Exception("Guest account not found");
        }


        // Get a guest account by ID, including closed accounts
        public async Task<GuestAccount> GetAccountByIdIncludingClosedAsync(
            string accountId)
        {
            using var context = await _contextFactory.CreateDbContextAsync();
            var guestAccount = await context.GuestAccounts
                .Include(accountId => accountId.Guest)
                .Include(accountId => accountId.Transactions)
                .Include(accountId => accountId.BookingDetails)
                .FirstOrDefaultAsync(x => x.Id == accountId);
            return guestAccount ?? throw new Exception("Guest account not found");
        }


        // Get guest account by invoice number
        public async Task<GuestAccount> GetAccountByInvoiceAsync(
            string invoiceNumber)
        {
            using var context = await _contextFactory.CreateDbContextAsync();
            var guestAccount = await context.GuestAccounts
                .Include(account => account.Guest)
                .Include(account => account.Transactions)
                .Include(account => account.BookingDetails)
                .FirstOrDefaultAsync(x => x.Invoice == invoiceNumber && !x.IsClosed);
            return guestAccount ?? throw new Exception("Guest account not found");
        }


        // Get guest account by invoice number, including closed accounts
        public async Task<GuestAccount> GetAccountByInvoiceIncludingClosedAsync(
            string invoiceNumber)
        {
            using var context = await _contextFactory.CreateDbContextAsync();
            var guestAccount = await context.GuestAccounts
                .Include(account => account.Guest)
                .Include(account => account.Transactions)
                .Include(account => account.BookingDetails)
                .FirstOrDefaultAsync(x => x.Invoice == invoiceNumber);
            return guestAccount ?? throw new Exception("Guest account not found");
        }


        // Top up the guest account
        public async Task ToUpAsync(
            string guestId, decimal amount, PaymentMethod paymentMethod, string bankAccountId, string userId)
        {
            using var context = await _contextFactory.CreateDbContextAsync();
            using var transaction = await context.Database.BeginTransactionAsync();

            if (amount < 0)
                throw new ArgumentException("Amount must be a positive value.", nameof(amount));

            var guestAccount = await GetAccountAsync(guestId); // Ensure this is tracked by context

            try
            {
                guestAccount.TopUps += amount;
                guestAccount.FundedBalance += amount;
                guestAccount.LastFunded = DateTime.UtcNow;

                var guestTransaction = new GuestTransaction
                {
                    Payment = amount,
                    Invoice = guestAccount.Invoice,
                    ApplicationUserId = userId,
                    GuestAccountId = guestAccount.Id,
                    BankAccountId = bankAccountId,
                    GuestId = guestAccount.GuestId,
                    Consumer = guestAccount.Guest.FullName,
                    PaymentMethod = paymentMethod,
                    TransactionType = Domain.Enum.TransactionType.Payment,
                    Description = $"Additional payment is ₦ {amount:N2}. Paid through {paymentMethod}",
                    Date = DateTime.Now,
                };

                context.GuestTransactions.Add(guestTransaction);
                context.GuestAccounts.Update(guestAccount);      

                await context.SaveChangesAsync();                

                await transaction.CommitAsync();                
            }
            catch
            {
                await transaction.RollbackAsync();          
                throw;
            }
        }


        // Add a charge to the guest account
        public async Task AddChargeAsync(
            string guestId,
            decimal amount)
        {
            using var context = await _contextFactory.CreateDbContextAsync();
            using var transaction = await context.Database.BeginTransactionAsync();

            var guestAccount = await GetAccountAsync(guestId);

            if (amount <= 0)
            {
                throw new ArgumentException("Amount must be a positive value.", nameof(amount));
            }

            try
            {
                guestAccount.OtherCharges += amount;
                guestAccount.FundedBalance -= amount;
                guestAccount.LastFunded = DateTime.Now;

                context.GuestAccounts.Update(guestAccount);
                await context.SaveChangesAsync();
                await transaction.CommitAsync();
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                throw new InvalidOperationException("Failed to add charge to the guest account.", ex);
            }
        }


        public async Task AddRoomChargeAsync(
            string guestId,
            decimal amount,
            decimal discount,
            decimal tax,
            decimal serviceCharge)
        {
            using var context = await _contextFactory.CreateDbContextAsync();
            using var transaction = await context.Database.BeginTransactionAsync();

            var guestAccount = await GetAccountAsync(guestId);

            if (amount <= 0)
            {
                throw new ArgumentException("Amount must be a positive value.", nameof(amount));
            }

            try
            {
                guestAccount.FundedBalance -= amount;
                guestAccount.Amount += amount;
                guestAccount.Tax += tax;
                guestAccount.Discount += discount;
                guestAccount.ServiceCharge += serviceCharge;

                guestAccount.LastFunded = DateTime.Now;

                context.GuestAccounts.Update(guestAccount);
                await context.SaveChangesAsync();
                await transaction.CommitAsync();
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                throw new InvalidOperationException("Failed to add charge to the guest account.", ex);
            }
        }


        // Close the guest account
        public async Task CloseAccountAsync(
            string guestId)
        {
            using var context = await _contextFactory.CreateDbContextAsync();
            using var transaction = await context.Database.BeginTransactionAsync();

            var guestAccount = await GetAccountAsync(guestId);

            try
            {
                guestAccount.IsClosed = true;
                context.GuestAccounts.Update(guestAccount);
                await context.SaveChangesAsync();
                await transaction.CommitAsync();
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                throw new InvalidOperationException("Failed to close the guest account.", ex);
            }
        }


        // Add a direct payment to the guest account
        public async Task AddDirectPaymentAsync(string guestId,
            decimal amount,
            decimal vat,
            decimal dicount,
            decimal serviceCharge)
        {
            using var context = await _contextFactory.CreateDbContextAsync();
            using var transaction = await context.Database.BeginTransactionAsync();

            var guestAccount = await GetAccountAsync(guestId);

            if (amount <= 0)
            {
                throw new ArgumentException("Amount must be a positive value.", nameof(amount));
            }

            try
            {
                guestAccount.DirectPayments += amount;
                guestAccount.FundedBalance += amount;
                guestAccount.LastFunded = DateTime.UtcNow;

                var guestTransaction = new GuestTransaction
                {
                    GuestId = guestAccount.GuestId,
                    Amount = amount,
                    Tax = vat + serviceCharge,
                    Discount = dicount,
                    Description = "Direct Payment",
                    Date = DateTime.UtcNow,
                    Invoice = guestAccount.Invoice,
                    TransactionType = Domain.Enum.TransactionType.Payment
                };

                context.GuestTransactions.Add(guestTransaction);
                context.GuestAccounts.Update(guestAccount);
                await context.SaveChangesAsync();
                await transaction.CommitAsync();
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                throw new InvalidOperationException("Failed to add direct payment to the guest account.", ex);
            }
        }


        // Add a refund to the guest account
        public async Task AddRefundAsync(
            string guestId,
            decimal amount,
            string description = "Refund")
        {
            using var context = await _contextFactory.CreateDbContextAsync();
            using var transaction = await context.Database.BeginTransactionAsync();

            var guestAccount = await GetAccountAsync(guestId);

            if (amount <= 0)
            {
                throw new ArgumentException("Amount must be a positive value.", nameof(amount));
            }

            try
            {
                //guestAccount.Refunds += amount;
                guestAccount.FundedBalance -= amount;
                guestAccount.LastFunded = DateTime.UtcNow;

                var guestTransaction = new GuestTransaction
                {
                    GuestId = guestAccount.GuestId,
                    Amount = -amount, // Negative for refund
                    Description = description,
                    Date = DateTime.UtcNow,
                    Invoice = guestAccount.Invoice,
                    TransactionType = Domain.Enum.TransactionType.Refund
                };

                context.GuestTransactions.Add(guestTransaction);
                context.GuestAccounts.Update(guestAccount);
                await context.SaveChangesAsync();
                await transaction.CommitAsync();
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                throw new InvalidOperationException("Failed to add refund to the guest account.", ex);
            }
        }


        // Open the guest account if it is closed
        public async Task OpenAccountAsync(
            string guestId)
        {
            using var context = await _contextFactory.CreateDbContextAsync();
            using var transaction = await context.Database.BeginTransactionAsync();

            var guestAccount = await GetAccountAsync(guestId);

            try
            {
                guestAccount.IsClosed = false;
                guestAccount.LastFunded = DateTime.UtcNow;

                context.GuestAccounts.Update(guestAccount);
                await context.SaveChangesAsync();
                await transaction.CommitAsync();
            }

            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                throw new InvalidOperationException("Failed to open the guest account.", ex);
            }
        }


        // Check if the guest account is settled
        public async Task<bool> IsSettled(
            string guestId)
        {
            using var context = await _contextFactory.CreateDbContextAsync();
            using var transaction = await context.Database.BeginTransactionAsync();

            var guestAccount = await GetAccountAsync(guestId);

            return guestAccount.Balance <= 0;
        }


        // Get a summary of the guest account
        public async Task<GuestAccountSummaryDto> GetGuestAccountSummaryAsync(
            string guestId)
        {
            using var _context = await _contextFactory.CreateDbContextAsync();

            var account = await GetAccountAsync(guestId);

            // Bookings
            var bookings = await _context.RoomBookings
                .Include(b => b.Booking)
                .Include(b => b.Booking.GuestAccount)
                .Include(b => b.Room)
                .Where(b => 
                    b.Booking.GuestId == guestId && 
                    b.Booking.GuestAccountId == account.Id && 
                    b.Booking.Status != BookingStatus.CheckedOut)
                .Select(b => new BookingSummaryDto
                {
                    Guest = b.OccupantName,
                    BookingBookingId = b.Booking.BookingId,
                    BookingId = b.Id,
                    Invoice = b.Booking.GuestAccount.Invoice,
                    RoomNumber = b.Room.Number,
                    RoomId = b.RoomId,
                    RoomType = b.Room.RoomType.Name,
                    CheckIn = b.CheckIn,
                    CheckOut = b.CheckOut,
                    Status = b.Booking.Status.ToString()
                })
                .OrderBy(b => b.RoomNumber)
                .ToListAsync();

            // Recent Transactions
            var recentTransactions = await _context.GuestTransactions
                .Where(t => 
                    t.GuestId == guestId && 
                    t.TransactionType == TransactionType.RoomCharge &&
                    t.Invoice == account.Invoice)
                .OrderBy(t => t.Date)
                .Select(t => new TransactionSummaryDto
                {
                    Date = t.Date,
                    Description = t.Description,
                    Invoice = t.Invoice,
                    Consumer = t.Consumer,
                    Discount = t.Discount,
                    BillPosts = t.BillPosts,
                    Payment = t.Payment,
                    Amount = t.Amount,
                    RoomId = t.RoomId,
                })
                .ToListAsync();

            // Service Consumptions
            var serviceConsumptions = await _context.GuestTransactions
                .Where(t => 
                    t.GuestId == guestId &&
                    t.Invoice == account.Invoice &&
                    (t.TransactionType == TransactionType.BarRestaurantOrder ||
                    t.TransactionType == TransactionType.Laundry))
                .OrderBy(t => t.Date)
                .Select(t => new TransactionSummaryDto
                {
                    Date = t.Date,
                    Description = t.Description,
                    Invoice = t.Invoice,
                    Discount = t.Discount,
                    BillPosts = t.BillPosts,
                    Payment = t.Payment,
                    Amount = t.Amount,
                })
                .ToListAsync();

            var payments = await _context.GuestTransactions
                .Where(t =>
                    t.GuestId == guestId &&
                    t.TransactionType == TransactionType.Payment &&
                    t.Invoice == account.Invoice)
                .OrderBy(t => t.Date)
                .Select(t => new TransactionSummaryDto
                {
                    Date = t.Date,
                    Description = t.Description,
                    Invoice = t.Invoice,
                    Discount = t.Discount,
                    BillPosts = t.BillPosts,
                    Payment = t.Payment,
                    Amount = t.Amount,
                })
                .ToListAsync();

            var groupedBookings = new List<BookingTransactionGroupDto>();

            foreach (var booking in bookings)
            {
                var bookingId = booking.BookingId;

                var recent = recentTransactions
                    .Where(t => t.Invoice == booking.Invoice && t.RoomId == booking.RoomId) // Adjust filtering
                    .ToList();

                groupedBookings.Add(new BookingTransactionGroupDto
                {
                    BookingId = bookingId,
                    GuestName = $"Guest: {booking.Guest} - {booking.RoomType} Room ({booking.RoomNumber})",
                    Summary = $"{booking.BookingBookingId} - Arri: {booking.CheckIn:MM/dd/yyyy} to Dept: {booking.CheckOut:MM/dd/yyyy} - Total = ₦ {recent.Sum(r => r.BillPosts):N2}",
                    RoomNumber = booking.RoomNumber,
                    RoomType = booking.RoomType,
                    CheckIn = booking.CheckIn,
                    CheckOut = booking.CheckOut,
                    Status = booking.Status,

                    RecentTransactions = recent,
                });
            }

            return new GuestAccountSummaryDto
            {
                GuestId = guestId,
                AccountId = account.Id,
                GuestName = account.Guest?.FullName,
                Invoice = account.Invoice,
                Amount = account.Amount,
                Discount = account.Discount,
                VAT = account.Tax,
                ServiceCharge = account.ServiceCharge,
                CheckIn = bookings.FirstOrDefault()!.CheckIn,
                CheckOut = bookings.LastOrDefault()!.CheckOut,
                Tax = account.Tax + account.ServiceCharge,
                OtherCharges = account.OtherCharges,
                Paid = account.Paid,
                Refunds = account.Refunds,
                Balance = account.Balance,
                FundedBalance = account.FundedBalance,
                OutstandingBalance = account.OutstandingBalance,

                BookingGroups = groupedBookings,
                Payments = payments,
                ServiceConsumptions = serviceConsumptions
            };

        }


        // Create guest booking
        public async Task<string> CreateGuestBookingAsync(
            MultiRoomBookingDto dto)
        {
            using var _context = await _contextFactory.CreateDbContextAsync();

            var guest = await _context.Guests.FindAsync(dto.GuestId) ?? throw new Exception("Guest not found.");

            var guestAccount = await OpenOrGetActiveGuestAccountAsync(dto.GuestId);

            var booking = new Booking
            {
                GuestId = dto.GuestId,
                GuestAccountId = guestAccount.Id,
                BookingId = $"BK-{Guid.NewGuid().ToString().Substring(0, 8).ToUpper()}",
                CheckIn = dto.CheckIn,
                CheckOut = dto.CheckOut,
                Discount = dto.Discount,
                VAT = dto.VAT,
                ServiceCharge = dto.ServiceCharge,
                ApplicationUserId = dto.ApplicationUserId,
                Status = BookingStatus.Completed,
                PaymentMethod = dto.PaymentMethod,
                BankAccountId = dto.AccountNumber,
                Amount = 0,
                DateCreated = DateTime.UtcNow
            };

            _context.Bookings.Add(booking);
            await _context.SaveChangesAsync();

            return booking.Id;
        }


        public async Task<RoomBooking> GetRoomBookingByRoomIdAsync(string roomId)
        {
            using var _context = await _contextFactory.CreateDbContextAsync();

            var room = await _context.Rooms.FindAsync(roomId)
                    ?? throw new Exception($"Room not found.");

            var roomBooking = await _context.RoomBookings.
                FirstOrDefaultAsync(r => r.RoomId == roomId);

            return roomBooking ?? throw new Exception("Room Booking nor found");
        }


        public async Task AssignRoomsToBookingAsync(
            string bookingId, 
            string userId, 
            List<RoomBooking> roomBookings)
        {
            using var _context = await _contextFactory.CreateDbContextAsync();

            var booking = await _context.Bookings.Include(b => b.RoomBookings).FirstOrDefaultAsync(b => b.Id == bookingId)
                ?? throw new Exception("Booking not found.");

            var guest = await _context.Guests.FindAsync(booking.GuestId) ?? throw new Exception("Guest not found.");

            foreach (var roomBookingDto in roomBookings)
            {
                var room = await _context.Rooms.FindAsync(roomBookingDto.RoomId)
                    ?? throw new Exception($"Room with ID {roomBookingDto.RoomId} not found.");

                var roomRate = room.Rate;
                var occupantName = !string.IsNullOrWhiteSpace(roomBookingDto.OccupantName)
                    ? roomBookingDto.OccupantName
                    : guest.FullName;

                var roomBooking = new RoomBooking
                {
                    Id = Guid.NewGuid().ToString(),
                    RoomId = roomBookingDto.RoomId,
                    CheckIn = roomBookingDto.CheckIn,
                    CheckOut = roomBookingDto.CheckOut,
                    Rate = roomBookingDto.Rate,
                    Tax = roomBookingDto.Tax,
                    ServiceCharge = roomBookingDto.ServiceCharge,
                    Discount = roomBookingDto.Discount,
                    Date = DateTime.Now,
                    OccupantName = occupantName,
                    OccupantPhoneNumber = roomBookingDto.OccupantPhoneNumber,
                    BookingId = booking.Id
                };

                booking.RoomBookings.Add(roomBooking);

                var bookedRoom = await _roomRepository.GetRoomById(roomBooking.RoomId);
                bookedRoom.Status = Domain.Entities.RoomSettings.RoomStatus.Booked;
                await _roomRepository.UpdateRoom(bookedRoom);

                var nights = (decimal)(roomBookingDto.CheckOut - roomBookingDto.CheckIn).TotalDays;
                booking.Amount += roomRate * nights;

                var guestTransactionDto = new GuestTransactionDto()
                {
                    Amount = roomBooking.Rate,
                    ApplicationUserId = userId,
                    GuestAccountId = booking.GuestAccountId,
                    BankAccountId = booking.BankAccountId,
                    Discount = roomBooking.Discount,
                    GuestId = booking.GuestId,
                    RoomId = roomBooking.RoomId,
                    Consumer = roomBooking.OccupantName,
                    PaymentMethod = booking.PaymentMethod,
                    TransactionType = TransactionType.RoomCharge,
                    Tax = roomBooking.Tax,
                    Description = "Room Charge (Inclusive of Inclusions)"
                };

                await AddRoomChargeAsync(
                    booking.GuestId, 
                    roomBooking.Rate, 
                    roomBooking.Discount, 
                    roomBooking.Tax, 
                    roomBooking.ServiceCharge);

                await AddTransaction(
                    booking.GuestId, 
                    guestTransactionDto);
            }

            var vatAmount = booking.Amount * (booking.VAT / 100m);
            var serviceChargeAmount = booking.Amount * (booking.ServiceCharge / 100m);
            var discountAmount = booking.Amount * (booking.Discount / 100m);

            booking.Amount = (booking.Amount + vatAmount + serviceChargeAmount) - discountAmount;

            await _context.SaveChangesAsync();
        }


        public async Task<List<GuestBookingSummaryDto>> GetGuestBookingsAsync(
            string guestId)
        {
            using var _context = await _contextFactory.CreateDbContextAsync();
            var bookings = await _context.Bookings
                .Include(b => b.RoomBookings)
                    .ThenInclude(rb => rb.Room)
                .Include(b => b.Guest)
                .Where(b => b.GuestId == guestId && !b.IsTrashed)
                .OrderByDescending(b => b.DateCreated)
                .Select(b => new GuestBookingSummaryDto
                {
                    BookingId = b.BookingId,
                    CheckIn = b.CheckIn,
                    CheckOut = b.CheckOut,
                    Duration = b.Duration,
                    Amount = b.Amount,
                    Status = b.Status,
                    Rooms = b.RoomBookings,
                    DateCreated = b.DateCreated,

                    Guest = b.Guest,
                })
                .ToListAsync();

            return bookings;
        }


        // Extend guest booking
        public async Task<GuestServiceResult> ExtendBookingAsync(
            string bookingId, 
            DateTime newCheckoutDate, 
            string modifiedBy)
        {
            using var _context = await _contextFactory.CreateDbContextAsync();
            var booking = await _context.Bookings
                .Include(b => b.RoomBookings)
                .FirstOrDefaultAsync(b => b.Id == bookingId);

            if (booking == null)
                return GuestServiceResult.Failure(["Booking not found."]);

            if (newCheckoutDate <= booking.CheckOut)
                return GuestServiceResult.Failure(["New checkout date must be later than current checkout."]);

            // Check if room is available for the extended period
            foreach (var roomBooking in booking.RoomBookings)
            {
                var isRoomAvailable = !_context.RoomBookings.Any(rb =>
                    rb.RoomId == roomBooking.RoomId &&
                    rb.BookingId != booking.Id &&
                    rb.CheckIn < newCheckoutDate &&
                    rb.CheckOut > booking.CheckOut);

                if (!isRoomAvailable)
                    return GuestServiceResult.Failure([$"Room '{roomBooking.Room?.Number ?? roomBooking.RoomId}' is not available for extension."]);
            }

            // Apply extension
            booking.CheckOut = newCheckoutDate;
            booking.DateModified = DateTime.Now;
            booking.UpdatedBy = modifiedBy;

            // Fetch room rate from the associated Room (you can adjust this logic based on your system)
            var roomRate = booking.Room?.Rate ?? 0;

            // Calculate the number of nights (or days)
            var newDuration = (newCheckoutDate.Date - booking.CheckIn.Date).Days;
            if (newDuration < 1)
            {
                return GuestServiceResult.Failure(["Checkout date must be at least one day after check-in."]);
            }

            // Recalculate the base amount
            var baseAmount = roomRate * newDuration;

            // Apply tax, service charge, and discount
            var vatRate = booking.VAT / 100;
            var serviceChargeRate = booking.ServiceCharge / 100;
            var discountRate = booking.Discount / 100;

            var multiplier = (1 + vatRate + serviceChargeRate) * (1 - discountRate);
            var finalAmount = baseAmount * multiplier;

            // Update booking amount and dates
            booking.Amount = Math.Round(finalAmount, 2);
            booking.CheckOut = newCheckoutDate;
            booking.DateModified = DateTime.Now;

            await _context.SaveChangesAsync();
            return GuestServiceResult.Success("Booking successfully extended.");
        }


        public async Task<string> CreateGuestAsync(
            CreateGuestDto dto)
        {
            using var _context = await _contextFactory.CreateDbContextAsync();

            var existingGuest = await _context.Guests
                .FirstOrDefaultAsync(g => g.IsTrashed == false && (g.Email == dto.Email || g.PhoneNumber == dto.PhoneNumber));

            if (existingGuest != null)
                throw new Exception("A guest with the same email or phone number already exists.");

            var guest = new Guest
            {
                GuestId = $"GST-{Guid.NewGuid().ToString().Substring(0, 8).ToUpper()}",
                GuestImage = dto.GuestImage,
                FirstName = dto.FirstName,
                LastName = dto.LastName,
                MiddleName = dto.MiddleName,
                Email = dto.Email,
                PhoneNumber = dto.PhoneNumber,
                Gender = dto.Gender,
                Street = dto.Street,
                City = dto.City,
                State = dto.State,
                Country = dto.Country,
                ApplicationUserId = dto.ApplicationUserId,
                Status = "Inactive",
                DateCreated = DateTime.UtcNow,
                DateModified = DateTime.UtcNow
            };

            _context.Guests.Add(guest);
            await _context.SaveChangesAsync();

            await OpenOrGetActiveGuestAccountAsync(guest.Id);

            return guest.Id; // or return a DTO
        }


        public async Task<string> CreateOrder(
            CreateOrderDto createOrderDto)
        {
            using var _context = await _contextFactory.CreateDbContextAsync();

            var guestAccount = await GetAccountAsync(createOrderDto.GuestId);

            var order = new Domain.Entities.StoreKeeping.Order
            {
                Invoice = guestAccount.Invoice,
                BookingId = createOrderDto.BookingId,
                RoomBookingId = createOrderDto.RoomBookingId,
                GuestAccountId = guestAccount.Id,
                OrderId = createOrderDto.OrderId,
                OrderItems = createOrderDto.OrderItems,
                Amount = createOrderDto.Amount
            };

            _context.Orders.Add(order);

            var guestTransactionDto = new GuestTransactionDto()
            {
                Amount = createOrderDto.Amount,
                ApplicationUserId = createOrderDto.ApplicationUserId,
                GuestAccountId = createOrderDto.GuestAccountId,
                BankAccountId = createOrderDto.BankAccountId,
                Discount = 0,
                GuestId = createOrderDto.GuestId,
                RoomId = createOrderDto.RoomId,
                Consumer = createOrderDto.Consumer,
                PaymentMethod = createOrderDto.PaymentMethod,
                TransactionType = createOrderDto.TransactionType,
                Tax = 0,
                Description = $"Room Service ({order.OrderId})"
            };

            await AddChargeAsync(guestTransactionDto.GuestId, guestTransactionDto.Amount);
            await AddTransaction(guestTransactionDto.GuestId, guestTransactionDto);

            await _context.SaveChangesAsync();

            return order.Id;
        }


        public async Task<List<Guest>> GetInHouseGuestAsync()
        {
            using var context = _contextFactory.CreateDbContext();

            var allBookings = await context.RoomBookings
                            .Include(b => b.Booking)
                            .Include(b => b.Room)
                            .Include(b => b.Booking.Guest)
                            .Where(r => r.Booking.IsTrashed == false)
                            .OrderByDescending(r => r.Date)
                            .ToListAsync();

            return [.. allBookings
                    .Select(b => b.Booking.Guest)];
        }

        public async Task<List<RoomBooking>> GetCurrentBooking()
        {
            using var context = _contextFactory.CreateDbContext();

            var allCurrentBooking = await context.RoomBookings
                .Include(b => b.Booking)
                .Include(b => b.Room)
                .OrderBy(b => b.Room.Number)
                .ToListAsync();

            return allCurrentBooking;
        }
    }
}
