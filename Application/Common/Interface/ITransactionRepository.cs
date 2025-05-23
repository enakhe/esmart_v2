using ESMART.Domain.Entities.Transaction;
using ESMART.Domain.ViewModels.Transaction;

namespace ESMART.Application.Common.Interface
{
    public interface ITransactionRepository
    {
        Task AddTransactionAsync(Transaction transaction);
        Task<List<TransactionViewModel>> GetAllTransactionsAsync();
        Task UpdateTransactionAsync(Transaction transaction);
        Task<Transaction?> GetByBookingIdAsync(string serviceId);
        Task<Transaction> GetByGuestIdAsync(string id);
        Task<Transaction> GetByIdAsync(string id);
        Task<Transaction> GetByTransactionItemIdAsync(string id);
        Task<TransactionViewModel> GetByTransactionIdAsync(string transactionId);
        Task<List<TransactionViewModel>> GetByFilterDateAsync(DateTime fromTime, DateTime endTime);
        Task<List<TransactionViewModel>> GetTransactionByGuestIdAsync(string guestId);
        Task<List<TransactionViewModel>> GetTransactionByBookingIdAsync(string bookingId);
        Task<List<TransactionViewModel>> GetTransactionByRoomNoAsync(string roomNo);

        Task AddTransactionItemAsync(TransactionItem transactionItem);
        Task UpdateTransactionItemAsync(TransactionItem transactionItem);
        Task DeleteTransactionItemAsync(string id);
        Task<TransactionItem> GetUnpaidTransactionItemsByServiceIdAsync(string serviceId, string guestId, decimal amount);
        Task<Transaction> GetByInvoiceNumberAsync(string invoiceNumber);
        Task<List<TransactionItemViewModel>> GetTransactionItemsByTransactionIdAsync(string transactionId);
        Task<List<TransactionItemViewModel>> GetAllTransactionItemsAsync();
        Task<List<TransactionItemViewModel>> GetTransactionItemByBookingIdAsync(string bookingId);
        Task<List<TransactionItemViewModel>> GetTransactionItemByRoomIdAsync(string roomId);
        Task<List<TransactionItemViewModel>> GetTransactionItemByGuestIdAndDate(string guestId, DateTime from, DateTime to);
        Task<List<TransactionItemViewModel>> GetTransactionItemByBookingIdAndDate(string bookingId, DateTime from, DateTime to);
        Task<List<TransactionItemViewModel>> GetTransactionItemByRoomIdAndDate(string roomId, DateTime from, DateTime to);
        Task<List<TransactionItemViewModel>> GetTransactionItemsByGuestIdAsync(string guestId);
        Task<List<TransactionItemViewModel>> GetUnpaidTransactionItemsByGuestIdAsync(string guestId);
        Task<TransactionItem> GetTransactionItemsByIdAsync(string id);
        Task MarkTransactionItemAsPaidAsync(string id);
        Task<List<TransactionItemViewModel>> GetTransactionItemsByDateAsync(DateTime fromTime, DateTime endTime);
        Task<List<TransactionItemViewModel>> GetTransactionItemsByFilterAsync(string filter);
        Task<List<RevenueViewModel>> GetRevenueByDateRange(DateTime from, DateTime to);

        Task AddBankAccountAsync(BankAccount bankAccount);
        Task<BankAccount> GetBankAccountById(string id);
        Task<List<BankAccount>> GetAllBankAccountAsync();
        Task UpdateBankAccountAsync(BankAccount bankAccount);
        Task DeleteBankAccountAsync(BankAccount bankAccount);
    }
}
