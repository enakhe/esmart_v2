#nullable disable

using ESMART.Application.Common.Interface;
using ESMART.Domain.Entities.Transaction;
using ESMART.Domain.Enum;
using ESMART.Domain.ViewModels.Transaction;
using ESMART.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
namespace ESMART.Infrastructure.Repositories.Transaction
{
    public class TransactionRepository(IDbContextFactory<ApplicationDbContext> contextFactory) : ITransactionRepository
    {
        private readonly IDbContextFactory<ApplicationDbContext> _contextFactory = contextFactory;

        public async Task AddTransactionAsync(Domain.Entities.Transaction.Transaction transaction)
        {
            try
            {
                using var context = _contextFactory.CreateDbContext();

                await context.Transactions.AddAsync(transaction);
                await context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                throw new Exception("An error occurred when adding transaction. " + ex.Message);
            }
        }

        public async Task<List<TransactionViewModel>> GetAllTransactionsAsync()
        {
            try
            {
                using var context = _contextFactory.CreateDbContext();
                var allTransactions = await context.Transactions
                                      .Where(t => !t.IsTrashed)
                                      .Select(t => new TransactionViewModel
                                      {
                                          TransactionId = t.TransactionId,
                                          Guest = t.Guest.FullName,
                                          GuestPhoneNo = t.Guest.PhoneNumber,
                                          Date = t.Date,
                                          TotalRevenue = t.TransactionItems.Where(t => t.TransactionId == t.TransactionId && t.Status == TransactionStatus.Paid).Sum(t => t.Amount),
                                          TotalReceivables = t.TransactionItems.Where(t => t.TransactionId == t.TransactionId && t.Status != TransactionStatus.Paid).Sum(t => t.Amount),
                                          InvoiceNumber = t.InvoiceNumber,
                                          Description = t.Description,
                                          IssuedBy = t.ApplicationUser.FullName,
                                          TransationItem = t.TransactionItems.OrderByDescending(ti => ti.DateAdded).ToList(),
                                          DateCreated = t.CreatedAt,
                                          DateUpdated = t.UpdatedAt,
                                      })
                                      .OrderByDescending(t => t.Date).ToListAsync();
                return allTransactions;
            }
            catch (Exception ex)
            {
                throw new Exception("An error occurred when retrieving transactions. " + ex.Message);
            }
        }

        public async Task UpdateTransactionAsync(Domain.Entities.Transaction.Transaction transaction)
        {
            try
            {
                using var context = _contextFactory.CreateDbContext();

                context.Entry(transaction).State = EntityState.Modified;
                await context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                throw new Exception("An error occurred when updating transaction folio. " + ex.Message);
            }
        }

        public async Task<Domain.Entities.Transaction.Transaction> GetByBookingIdAsync(string serviceId)
        {
            try
            {
                using var context = _contextFactory.CreateDbContext();
                var transaction = await context.Transactions.FirstOrDefaultAsync(t => t.BookingId == serviceId);
                return transaction;
            }
            catch (Exception ex)
            {
                throw new Exception("An error occurred when retrieving a booking transaction" + ex.Message);
            }
        }

        public async Task<Domain.Entities.Transaction.Transaction> GetByGuestIdAsync(string id)
        {
            try
            {
                using var context = _contextFactory.CreateDbContext();
                var guestTransaction = await context.Transactions.FirstOrDefaultAsync(t => t.GuestId == id);
                return guestTransaction;
            }
            catch (Exception ex)
            {
                throw new Exception("An error occured when getting every guest transaction. " + ex.Message);
            }
        }

        public async Task<Domain.Entities.Transaction.Transaction> GetByIdAsync(string id)
        {
            try
            {
                using var context = _contextFactory.CreateDbContext();
                var transaction = await context.Transactions.FirstOrDefaultAsync(t => t.Id == id);
                return transaction;
            }
            catch (Exception ex)
            {
                throw new Exception("An error occured when getting every guest transaction. " + ex.Message);
            }
        }

        public async Task<List<TransactionViewModel>> GetByFilterDateAsync(DateTime fromTime, DateTime endTime)
        {
            try
            {
                using var context = _contextFactory.CreateDbContext();
                var allTransactions = await context.Transactions
                                          .Where(t => !t.IsTrashed &&
                                            t.Date <= endTime &&
                                            t.Date >= fromTime)
                                          .Select(t => new TransactionViewModel
                                          {
                                              TransactionId = t.TransactionId,
                                              Guest = t.Guest.FullName,
                                              GuestPhoneNo = t.Guest.PhoneNumber,
                                              Date = t.Date,
                                              TotalRevenue = t.TransactionItems.Where(t => t.TransactionId == t.TransactionId && t.Status == TransactionStatus.Paid).Sum(t => t.Amount),
                                              TotalReceivables = t.TransactionItems.Where(t => t.TransactionId == t.TransactionId && t.Status != TransactionStatus.Paid).Sum(t => t.Amount),
                                              InvoiceNumber = t.InvoiceNumber,
                                              Description = t.Description,
                                              IssuedBy = t.ApplicationUser.FullName,
                                              TransationItem = t.TransactionItems.OrderByDescending(ti => ti.DateAdded).ToList(),
                                              DateCreated = t.CreatedAt,
                                              DateUpdated = t.UpdatedAt,
                                          })
                                          .OrderByDescending(t => t.Date).ToListAsync();
                return allTransactions;
            }
            catch (Exception ex)
            {
                throw new Exception("An error occurred when retrieving a transaction by date" + ex.Message);
            }
        }

        public async Task<List<TransactionViewModel>> GetTransactionByGuestIdAsync(string guestId)
        {
            try
            {
                using var context = _contextFactory.CreateDbContext();
                var transactionItems = await context.Transactions
                    .Include(t => t.TransactionItems)
                    .Include(t => t.Guest)
                    .Include(t => t.ApplicationUser)
                    .Include(t => t.Booking)
                    .Where(t => t.GuestId == guestId)
                    .Select(t => new TransactionViewModel
                    {
                        TransactionId = t.TransactionId,
                        InvoiceNumber = t.InvoiceNumber,
                        Guest = t.Guest.FullName,
                        GuestPhoneNo = t.Guest.PhoneNumber,
                        Date = t.Date,
                        TotalRevenue = t.TransactionItems.Where(ti => ti.TransactionId == t.TransactionId && ti.Status == TransactionStatus.Paid).Sum(ti => ti.Amount),
                        TotalReceivables = t.TransactionItems.Where(ti => ti.TransactionId == t.TransactionId && ti.Status != TransactionStatus.Paid).Sum(ti => ti.Amount),
                        Description = t.Description,
                        IssuedBy = t.ApplicationUser.FullName,
                        TransationItem = t.TransactionItems.OrderByDescending(ti => ti.DateAdded).ToList(),
                        Booking = t.Booking,
                        DateCreated = t.CreatedAt,
                        DateUpdated = t.UpdatedAt,
                    })
                    .OrderByDescending(ti => ti.Date)
                    .ToListAsync();
                return transactionItems;
            }
            catch (Exception ex)
            {
                throw new Exception("An error occurred when retrieving transaction items. " + ex.Message);
            }
        }

        // Get transaction by InvoiceNumber
        public async Task<Domain.Entities.Transaction.Transaction> GetByInvoiceNumberAsync(string invoiceNumber)
        {
            try
            {
                using var context = _contextFactory.CreateDbContext();
                var transaction = await context.Transactions.FirstOrDefaultAsync(t => t.InvoiceNumber == invoiceNumber);
                return transaction;
            }
            catch (Exception ex)
            {
                throw new Exception("An error occurred when retrieving a transaction by invoice number" + ex.Message);
            }
        }

        // Get transaction by TransactionId
        public async Task<TransactionViewModel> GetByTransactionIdAsync(string transactionId)
        {
            try
            {
                using var context = _contextFactory.CreateDbContext();
                var transaction = await context.Transactions.Where(t => t.TransactionId == transactionId)
                    .Select(t => new TransactionViewModel
                    {
                        TransactionId = t.TransactionId,
                        InvoiceNumber = t.InvoiceNumber,
                        Guest = t.Guest.FullName,
                        GuestPhoneNo = t.Guest.PhoneNumber,
                        Date = t.Date,
                        TotalRevenue = t.TransactionItems.Where(ti => ti.Status == TransactionStatus.Paid).Sum(ti => ti.Amount),
                        TotalReceivables = t.TransactionItems.Where(ti => ti.Status != TransactionStatus.Paid).Sum(ti => ti.Amount),
                        Description = t.Description,
                        IssuedBy = t.ApplicationUser.FullName,
                        TransationItem = t.TransactionItems.OrderByDescending(ti => ti.DateAdded).ToList(),
                        Booking = t.Booking,
                        DateCreated = t.CreatedAt,
                        DateUpdated = t.UpdatedAt,
                    }).FirstOrDefaultAsync();
                return transaction;
            }
            catch (Exception ex)
            {
                throw new Exception("An error occurred when retrieving a transaction by invoice number" + ex.Message);
            }
        }

        // TrnsactionItem
        public async Task AddTransactionItemAsync(TransactionItem transactionItem)
        {
            try
            {
                await using var context = _contextFactory.CreateDbContext();
                await context.TransactionItems.AddAsync(transactionItem);
                await context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("An error occurred while adding the transaction item.", ex);
            }
        }

        public async Task<List<TransactionItemViewModel>> GetAllTransactionItemsAsync()
        {
            try
            {
                using var context = _contextFactory.CreateDbContext();
                var transactionItems = await context.TransactionItems
                    .Where(ti => !ti.IsTrashed)
                    .Select(ti => new TransactionItemViewModel
                    {
                        ServiceId = ti.ServiceId,
                        Amount = ti.Amount.ToString("N2"),
                        TaxAmount = ti.TaxAmount,
                        ServiceCharge = ti.ServiceCharge,
                        Discount = ti.Discount,
                        Category = ti.Category.ToString(),
                        Type = ti.Type.ToString(),
                        Status = ti.Status,
                        BankAccount = ti.BankAccount,
                        DateAdded = ti.DateAdded,
                        IssuedBy = ti.ApplicationUser.FullName,
                    })
                    .OrderByDescending(ti => ti.DateAdded)
                    .ToListAsync();
                return transactionItems;
            }
            catch (Exception ex)
            {
                throw new Exception("An error occurred when retrieving transaction items. " + ex.Message);
            }
        }

        public async Task<TransactionItem> GetUnpaidTransactionItemsByServiceIdAsync(string serviceId, string guestId, decimal amount)
        {
            try
            {
                using var context = _contextFactory.CreateDbContext();
                var transactionItems = await context.TransactionItems
                    .FirstOrDefaultAsync(ti => ti.ServiceId == serviceId &&
                    ti.Amount == amount &&
                    ti.Transaction.GuestId == guestId &&
                    ti.Status == TransactionStatus.Unpaid);
                return transactionItems;
            }
            catch (Exception ex)
            {
                throw new Exception("An error occurred when retrieving transaction items. " + ex.Message);
            }
        }

        public async Task<List<TransactionItemViewModel>> GetTransactionItemByBookingIdAsync(string bookingId)
        {
            try
            {
                using var context = _contextFactory.CreateDbContext();
                var transactionItems = await context.TransactionItems
                    .Where(ti => ti.Transaction.BookingId == bookingId)
                    .Select(ti => new TransactionItemViewModel
                    {
                        ServiceId = ti.ServiceId,
                        Amount = ti.Amount.ToString("N2"),
                        TaxAmount = ti.TaxAmount,
                        ServiceCharge = ti.ServiceCharge,
                        Discount = ti.Discount,
                        Category = ti.Category.ToString(),
                        Type = ti.Type.ToString(),
                        Status = ti.Status,
                        BankAccount = ti.BankAccount,
                        DateAdded = ti.DateAdded,
                        IssuedBy = ti.ApplicationUser.FullName,
                    })
                    .OrderByDescending(ti => ti.DateAdded)
                    .ToListAsync();
                return transactionItems;
            }
            catch (Exception ex)
            {
                throw new Exception("An error occurred when retrieving transaction items. " + ex.Message);
            }
        }

        public async Task<List<TransactionItemViewModel>> GetTransactionItemByRoomIdAsync(string roomId)
        {
            try
            {
                using var context = _contextFactory.CreateDbContext();
                var transactionItems = await context.TransactionItems
                    .Include(ti => ti.Transaction)
                    .Include(ti => ti.Transaction.Booking)
                    .Include(ti => ti.Transaction.Guest)
                    .Include(ti => ti.Transaction.Booking.Room)
                    .Where(ti => ti.Transaction.Booking.RoomId == roomId)
                    .Select(ti => new TransactionItemViewModel
                    {
                        ServiceId = ti.ServiceId,
                        Amount = ti.Amount.ToString("N2"),
                        TaxAmount = ti.TaxAmount,
                        ServiceCharge = ti.ServiceCharge,
                        Discount = ti.Discount,
                        Category = ti.Category.ToString(),
                        Type = ti.Type.ToString(),
                        Status = ti.Status,
                        BankAccount = ti.BankAccount,
                        DateAdded = ti.DateAdded,
                        IssuedBy = ti.ApplicationUser.FullName,
                    })
                    .OrderByDescending(ti => ti.DateAdded)
                    .ToListAsync();
                return transactionItems;
            }
            catch (Exception ex)
            {
                throw new Exception("An error occurred when retrieving transaction items. " + ex.Message);
            }
        }

        public async Task<List<TransactionItemViewModel>> GetTransactionItemByRoomIdAndDate(string roomId, DateTime from, DateTime to)
        {
            try
            {
                using var context = _contextFactory.CreateDbContext();
                var transactionItems = await context.TransactionItems
                    .Where(ti => ti.Transaction.Booking.RoomId == roomId &&
                    ti.DateAdded.Date >= from.Date &&
                    ti.DateAdded.Date <= to.Date)
                    .Select(ti => new TransactionItemViewModel
                    {
                        ServiceId = ti.ServiceId,
                        Amount = ti.Amount.ToString("N2"),
                        TaxAmount = ti.TaxAmount,
                        ServiceCharge = ti.ServiceCharge,
                        Discount = ti.Discount,
                        Category = ti.Category.ToString(),
                        Type = ti.Type.ToString(),
                        Status = ti.Status,
                        BankAccount = ti.BankAccount,
                        DateAdded = ti.DateAdded,
                        IssuedBy = ti.ApplicationUser.FullName,
                    })
                    .OrderByDescending(ti => ti.DateAdded)
                    .ToListAsync();
                return transactionItems;
            }
            catch (Exception ex)
            {
                throw new Exception("An error occurred when retrieving transaction items. " + ex.Message);
            }
        }

        public async Task<List<TransactionItemViewModel>> GetTransactionItemsByGuestIdAsync(string guestId)
        {
            try
            {
                using var context = _contextFactory.CreateDbContext();
                var transactionItems = await context.TransactionItems
                    .Where(ti => ti.Transaction.GuestId == guestId)
                    .Select(ti => new TransactionItemViewModel
                    {
                        ServiceId = ti.ServiceId,
                        Amount = ti.Amount.ToString("N2"),
                        TaxAmount = ti.TaxAmount,
                        ServiceCharge = ti.ServiceCharge,
                        Discount = ti.Discount,
                        Category = ti.Category.ToString(),
                        Type = ti.Type.ToString(),
                        Status = ti.Status,
                        BankAccount = ti.BankAccount,
                        DateAdded = ti.DateAdded,
                        IssuedBy = ti.ApplicationUser.FullName,
                    })
                    .OrderByDescending(ti => ti.DateAdded)
                    .ToListAsync();
                return transactionItems;
            }
            catch (Exception ex)
            {
                throw new Exception("An error occurred when retrieving transaction items. " + ex.Message);
            }
        }

        // Get transactionitem by transaction transactionid
        public async Task<List<TransactionItemViewModel>> GetTransactionItemsByTransactionIdAsync(string transactionId)
        {
            try
            {
                using var context = _contextFactory.CreateDbContext();
                var transactionItems = await context.TransactionItems
                    .Where(ti => ti.Transaction.TransactionId == transactionId)
                    .Select(ti => new TransactionItemViewModel
                    {
                        ServiceId = ti.ServiceId,
                        Amount = ti.Amount.ToString("N2"),
                        TaxAmount = ti.TaxAmount,
                        ServiceCharge = ti.ServiceCharge,
                        Discount = ti.Discount,
                        Category = ti.Category.ToString(),
                        Type = ti.Type.ToString(),
                        Status = ti.Status,
                        BankAccount = ti.BankAccount,
                        DateAdded = ti.DateAdded,
                        IssuedBy = ti.ApplicationUser.FullName,
                    })
                    .OrderByDescending(ti => ti.DateAdded)
                    .ToListAsync();
                return transactionItems;
            }
            catch (Exception ex)
            {
                throw new Exception("An error occurred when retrieving transaction items. " + ex.Message);
            }
        }

        public async Task<List<TransactionItemViewModel>> GetUnpaidTransactionItemsByGuestIdAsync(string guestId)
        {
            try
            {
                using var context = _contextFactory.CreateDbContext();
                var transactionItems = await context.TransactionItems
                    .Where(ti => ti.Transaction.GuestId == guestId && ti.Status == TransactionStatus.Unpaid)
                    .Select(ti => new TransactionItemViewModel
                    {
                        ServiceId = ti.ServiceId,
                        Amount = ti.Amount.ToString("N2"),
                        TaxAmount = ti.TaxAmount,
                        ServiceCharge = ti.ServiceCharge,
                        Discount = ti.Discount,
                        Category = ti.Category.ToString(),
                        Type = ti.Type.ToString(),
                        Status = ti.Status,
                        BankAccount = ti.BankAccount,
                        DateAdded = ti.DateAdded,
                        IssuedBy = ti.ApplicationUser.FullName,
                    })
                    .OrderByDescending(ti => ti.DateAdded)
                    .ToListAsync();
                return transactionItems;
            }
            catch (Exception ex)
            {
                throw new Exception("An error occurred when retrieving transaction items. " + ex.Message);
            }
        }


        public async Task<List<TransactionItemViewModel>> GetTransactionItemByGuestIdAndDate(string guestId, DateTime from, DateTime to)
        {
            try
            {
                using var context = _contextFactory.CreateDbContext();
                var transactionItems = await context.TransactionItems
                    .Where(ti => ti.Transaction.GuestId == guestId &&
                    ti.DateAdded >= from &&
                    ti.DateAdded <= to)
                    .Select(ti => new TransactionItemViewModel
                    {
                        ServiceId = ti.ServiceId,
                        Amount = ti.Amount.ToString("N2"),
                        TaxAmount = ti.TaxAmount,
                        ServiceCharge = ti.ServiceCharge,
                        Discount = ti.Discount,
                        Category = ti.Category.ToString(),
                        Type = ti.Type.ToString(),
                        Status = ti.Status,
                        BankAccount = ti.BankAccount,
                        DateAdded = ti.DateAdded,
                        IssuedBy = ti.ApplicationUser.FullName,
                    })
                    .OrderByDescending(ti => ti.DateAdded)
                    .ToListAsync();
                return transactionItems;
            }
            catch (Exception ex)
            {
                throw new Exception("An error occurred when retrieving transaction items. " + ex.Message);
            }
        }

        public async Task<List<TransactionItemViewModel>> GetTransactionItemByBookingIdAndDate(string bookingId, DateTime from, DateTime to)
        {
            try
            {
                using var context = _contextFactory.CreateDbContext();
                var transactionItems = await context.TransactionItems
                    .Where(ti => ti.Transaction.BookingId == bookingId &&
                    ti.DateAdded >= from &&
                    ti.DateAdded <= to)
                    .Select(ti => new TransactionItemViewModel
                    {
                        ServiceId = ti.ServiceId,
                        Amount = ti.Amount.ToString("N2"),
                        TaxAmount = ti.TaxAmount,
                        ServiceCharge = ti.ServiceCharge,
                        Discount = ti.Discount,
                        Category = ti.Category.ToString(),
                        Type = ti.Type.ToString(),
                        Status = ti.Status,
                        BankAccount = ti.BankAccount,
                        DateAdded = ti.DateAdded,
                        IssuedBy = ti.ApplicationUser.FullName,
                    })
                    .OrderByDescending(ti => ti.DateAdded)
                    .ToListAsync();
                return transactionItems;
            }
            catch (Exception ex)
            {
                throw new Exception("An error occurred when retrieving transaction items. " + ex.Message);
            }
        }

        public async Task<List<TransactionItemViewModel>> GetTransactionItemsByIdAsync(string id)
        {
            try
            {
                using var context = _contextFactory.CreateDbContext();
                var transactionItems = await context.TransactionItems
                    .Where(ti => ti.Id == id)
                    .Select(ti => new TransactionItemViewModel
                    {
                        ServiceId = ti.ServiceId,
                        Amount = ti.Amount.ToString("N2"),
                        TaxAmount = ti.TaxAmount,
                        ServiceCharge = ti.ServiceCharge,
                        Discount = ti.Discount,
                        Category = ti.Category.ToString(),
                        Type = ti.Type.ToString(),
                        Status = ti.Status,
                        BankAccount = ti.BankAccount,
                        DateAdded = ti.DateAdded,
                        IssuedBy = ti.ApplicationUser.FullName,
                    })
                    .OrderByDescending(ti => ti.DateAdded)
                    .ToListAsync();
                return transactionItems;
            }
            catch (Exception ex)
            {
                throw new Exception("An error occurred when retrieving transaction items. " + ex.Message);
            }
        }

        public async Task UpdateTransactionItemAsync(TransactionItem transactionItem)
        {
            try
            {
                using var context = _contextFactory.CreateDbContext();
                context.Entry(transactionItem).State = EntityState.Modified;
                await context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                throw new Exception("An error occurred when updating transaction item. " + ex.Message);
            }
        }

        public async Task DeleteTransactionItemAsync(string id)
        {
            try
            {
                using var context = _contextFactory.CreateDbContext();
                var transactionItem = await context.TransactionItems.FindAsync(id);
                if (transactionItem != null)
                {
                    transactionItem.IsTrashed = true;
                    await context.SaveChangesAsync();
                }
            }
            catch (Exception ex)
            {
                throw new Exception("An error occurred when deleting transaction item. " + ex.Message);
            }
        }

        public async Task<List<TransactionItemViewModel>> GetTransactionItemsByDateAsync(DateTime fromTime, DateTime endTime)
        {
            try
            {
                using var context = _contextFactory.CreateDbContext();
                var transactionItems = await context.TransactionItems
                    .Where(ti => ti.DateAdded >= fromTime && ti.DateAdded <= endTime)
                    .Select(ti => new TransactionItemViewModel
                    {
                        ServiceId = ti.ServiceId,
                        Amount = ti.Amount.ToString("N2"),
                        TaxAmount = ti.TaxAmount,
                        ServiceCharge = ti.ServiceCharge,
                        Discount = ti.Discount,
                        Category = ti.Category.ToString(),
                        Type = ti.Type.ToString(),
                        Status = ti.Status,
                        BankAccount = ti.BankAccount,
                        DateAdded = ti.DateAdded,
                        IssuedBy = ti.ApplicationUser.FullName,
                    })
                    .OrderByDescending(ti => ti.DateAdded)
                    .ToListAsync();
                return transactionItems;
            }
            catch (Exception ex)
            {
                throw new Exception("An error occurred when retrieving transaction items by date. " + ex.Message);
            }
        }

        public async Task<List<TransactionItemViewModel>> GetTransactionItemsByFilterAsync(string filter)
        {
            try
            {
                using var context = _contextFactory.CreateDbContext();
                var transactionItems = await context.TransactionItems
                    .Where(ti => ti.Description.Contains(filter) || ti.BankAccount.Contains(filter) || ti.ApplicationUser.FullName.Contains(filter))
                    .Select(ti => new TransactionItemViewModel
                    {
                        ServiceId = ti.ServiceId,
                        Amount = ti.Amount.ToString("N2"),
                        TaxAmount = ti.TaxAmount,
                        ServiceCharge = ti.ServiceCharge,
                        Discount = ti.Discount,
                        Category = ti.Category.ToString(),
                        Type = ti.Type.ToString(),
                        Status = ti.Status,
                        BankAccount = ti.BankAccount,
                        DateAdded = ti.DateAdded,
                        IssuedBy = ti.ApplicationUser.FullName,
                    })
                    .OrderByDescending(ti => ti.DateAdded)
                    .ToListAsync();
                return transactionItems;
            }
            catch (Exception ex)
            {
                throw new Exception("An error occurred when retrieving transaction items by filter. " + ex.Message);
            }
        }

        public async Task<List<RevenueViewModel>> GetRevenueByDateRange(DateTime from, DateTime to)
        {
            try
            {
                using var context = _contextFactory.CreateDbContext();

                var dailyRevenue = await context.TransactionItems
                    .Where(ti => ti.DateAdded.Date >= from.Date && ti.DateAdded.Date <= to.Date && ti.Status == TransactionStatus.Paid)
                    .GroupBy(ti => ti.DateAdded.Date)
                    .Select(g => new RevenueViewModel
                    {
                        Date = g.Key,
                        TotalRevenue = g.Sum(ti => ti.Amount)
                    })
                    .OrderBy(r => r.Date)
                    .ToListAsync();

                return dailyRevenue;
            }
            catch (Exception ex)
            {
                throw new Exception("An error occurred when retrieving daily revenue by filter. " + ex.Message);
            }
        }


        public async Task AddBankAccountAsync(BankAccount bankAccount)
        {
            try
            {
                using var context = _contextFactory.CreateDbContext();
                await context.BankAccount.AddAsync(bankAccount);
                await context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                throw new Exception("An error occurred when adding bank account details. " + ex.Message);
            }
        }

        public async Task<BankAccount> GetBankAccountById(string id)
        {
            try
            {
                using var context = _contextFactory.CreateDbContext();
                var bankAccount = await context.BankAccount.FirstOrDefaultAsync(ba => ba.Id == id);

                return bankAccount;
            }
            catch (Exception ex)
            {
                throw new Exception("An error occurred when getting bank account details by id. " + ex.Message);
            }
        }

        public async Task<List<BankAccount>> GetAllBankAccountAsync()
        {
            try
            {
                using var context = _contextFactory.CreateDbContext();
                var allBankAccounts = await context.BankAccount.ToListAsync();
                return allBankAccounts;
            }
            catch (Exception ex)
            {
                throw new Exception("An error occurred when retrieving al bank accounts. " + ex.Message);
            }
        }

        public async Task UpdateBankAccountAsync(BankAccount bankAccount)
        {
            try
            {
                using var context = _contextFactory.CreateDbContext();
                context.Entry(bankAccount).State = EntityState.Modified;
                await context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                throw new Exception("An error occurred when updating the bank account. " + ex.Message);
            }
        }

        public async Task DeleteBankAccountAsync(BankAccount bankAccount)
        {
            try
            {
                using var context = _contextFactory.CreateDbContext();
                context.BankAccount.Remove(bankAccount);
                await context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                throw new Exception("An error ocurred when deleting the bank account. " + ex.Message);
            }
        }
    }
}
