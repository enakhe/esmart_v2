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
        Task<List<TransactionViewModel>> GetByFilterDateAsync(DateTime fromTime, DateTime endTime);

        Task AddTransactionItemAsync(TransactionItem transactionItem);
        Task UpdateTransactionItemAsync(TransactionItem transactionItem);
        Task DeleteTransactionItemAsync(string id);
        Task<TransactionItem> GetUnpaidTransactionItemsByServiceIdAsync(string serviceId);
        Task<List<TransactionItemViewModel>> GetTransactionItemsByTransactionIdAsync(string transactionId);
        Task<List<TransactionItemViewModel>> GetAllTransactionItemsAsync();
        Task<List<TransactionItemViewModel>> GetTransactionItemByBookingIdAsync(string bookingId);
        Task<List<TransactionItemViewModel>> GetTransactionItemByGuestIdAndDate(string guestId, DateTime from, DateTime to);
        Task<List<TransactionItemViewModel>> GetTransactionItemByBookingIdAndDate(string bookingId, DateTime from, DateTime to);
        Task<List<TransactionItemViewModel>> GetTransactionItemsByGuestIdAsync(string guestId);
        Task<List<TransactionItemViewModel>> GetTransactionItemsByIdAsync(string id);
        Task<List<TransactionItemViewModel>> GetTransactionItemsByDateAsync(DateTime fromTime, DateTime endTime);
        Task<List<TransactionItemViewModel>> GetTransactionItemsByFilterAsync(string filter);
    }
}
