using ESMART.Application.Common.Dtos;
using ESMART.Domain.Entities.FrontDesk;
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
    public class GuestAccountService(IDbContextFactory<ApplicationDbContext> contextFactory)
    {
        private readonly IDbContextFactory<ApplicationDbContext> _contextFactory = contextFactory;

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
                    Invoice = GenerateGuestAccountInvoiceNumber()
                };

                context.GuestAccounts.Add(guestAccount);
                await context.SaveChangesAsync();
            }

            return guestAccount;
        }

        public async Task<GuestAccount> GetAccountAsync(
            string guestId)
        {
            using var context = await _contextFactory.CreateDbContextAsync();
            var guestAccount = await context.GuestAccounts
                .Include(account => account.Guest)
                .Include(account => account.Transactions)
                .Include(account => account.BookingDetails)
                .FirstOrDefaultAsync(x => x.GuestId == guestId && !x.IsClosed);

            return guestAccount ?? throw new Exception("Guest account not found");
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
            string guestId,
            decimal amount)
        {
            using var context = await _contextFactory.CreateDbContextAsync();
            using var transaction = await context.Database.BeginTransactionAsync();

            var guestAccount = await GetAccountAsync(guestId);

            if (amount < 0)
            {
                throw new ArgumentException("Amount must be a positive value.", nameof(amount));
            }

            try
            {
                guestAccount.TopUps += amount;
                guestAccount.FundedBalance += amount;
                guestAccount.LastFunded = DateTime.UtcNow;

                var guestTransaction = new GuestTransaction
                {
                    GuestId = guestAccount.GuestId,
                    Payment = amount,
                    Description = "Top Up",
                    Date = DateTime.UtcNow,
                    Invoice = guestAccount.Invoice,
                    TransactionType = Domain.Enum.TransactionType.Payment,
                };

                context.GuestAccounts.Update(guestAccount);
                await context.SaveChangesAsync();


            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                throw new InvalidOperationException("Failed to top up the guest account.", ex);
            }
        }

        // Add a charge to the guest account
        public async Task AddChargeAsync(
            string guestId,
            decimal amount,
            TransactionType transactionType,
            string description = "Room Service")
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
                guestAccount.LastFunded = DateTime.UtcNow;

                var guestTransaction = new GuestTransaction
                {
                    GuestId = guestAccount.GuestId,
                    Amount = amount,
                    Description = description,
                    Date = DateTime.UtcNow,
                    Invoice = guestAccount.Invoice,
                    TransactionType = transactionType
                };

                context.GuestTransactions.Add(guestTransaction);
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
                guestAccount.Refunds += amount;
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
        public async Task<GuestAccountSummaryDto> GetGuestAccountSummaryAsync(string guestId)
        {
            using var _context = await _contextFactory.CreateDbContextAsync();

            var account = await GetAccountAsync(guestId);

            // Bookings
            var bookings = await _context.Bookings
                .Include(b => b.Guest)
                .Include(b => b.ApplicationUser)
                .Include(b => b.Room)
                .Where(b => 
                    b.GuestId == guestId && 
                    b.GuestAccountId == account.Id && 
                    b.Status != BookingStatus.CheckedOut)
                .Select(b => new BookingSummaryDto
                {
                    BookingId = b.Id,
                    RoomNumber = b.Room.Number,
                    RoomType = b.Room.RoomType.Name,
                    CheckIn = b.CheckIn,
                    CheckOut = b.CheckOut,
                    Status = b.Status.ToString()
                })
                .ToListAsync();

            // Recent Transactions
            var recentTransactions = await _context.GuestTransactions
                .Where(t => 
                    t.GuestId == guestId && 
                    t.TransactionType == TransactionType.RoomCharge &&
                    t.Invoice == account.Invoice &&
                    t.Date.Date < DateTime.Today)
                .OrderBy(t => t.Date)
                .Select(t => new TransactionSummaryDto
                {
                    TransactionId = t.TransactionId,
                    Date = t.Date,
                    Description = t.Description,
                    Invoice = t.Invoice,
                    Discount = t.Discount,
                    BillPosts = t.BillPosts,
                    Payment = t.Payment,
                    Amount = t.Amount,
                })
                .ToListAsync();

            // Service Consumptions
            var serviceConsumptions = await _context.GuestTransactions
                .Where(t => 
                    t.GuestId == guestId && 
                    t.TransactionType == TransactionType.BarOrder &&
                    t.TransactionType == TransactionType.RestaurantOrder &&
                    t.TransactionType == TransactionType.Laundry &&
                    t.Invoice == account.Invoice &&
                    t.Date.Date < DateTime.Today)
                .OrderBy(t => t.Date)
                .Select(t => new TransactionSummaryDto
                {
                    TransactionId = t.TransactionId,
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
                    t.Invoice == account.Invoice &&
                    t.Date.Date < DateTime.Today)
                .OrderBy(t => t.Date)
                .Select(t => new TransactionSummaryDto
                {
                    TransactionId = t.TransactionId,
                    Date = t.Date,
                    Description = t.Description,
                    Invoice = t.Invoice,
                    Discount = t.Discount,
                    BillPosts = t.BillPosts,
                    Payment = t.Payment,
                    Amount = t.Amount,
                })
                .ToListAsync();

            return new GuestAccountSummaryDto
            {
                GuestId = guestId,
                GuestName = account.Guest?.FullName,
                AccountId = account.Id,
                FundedBalance = account.FundedBalance,
                OutstandingBalance = account.OutstandingBalance,
                Paid = account.Paid,
                Refunds = account.Refunds,
                Bookings = bookings,
                RecentTransactions = recentTransactions,
                ServiceConsumptions = serviceConsumptions,
                Payments = payments,
            };
        }

        // Generate a unique invoice number for guest accounts
        private string GenerateGuestAccountInvoiceNumber()
        {
            return $"GA-{Guid.NewGuid().ToString().Split('-')[0].ToUpper().AsSpan(0, 5)}";
        }

    }
}
