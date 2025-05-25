using ESMART.Domain.Entities.FrontDesk;
using ESMART.Domain.ViewModels.FrontDesk;

namespace ESMART.Application.Common.Interface
{
    public interface IBookingRepository
    {
        Task<int> GetBookingNumber();
        Task AddBooking(Booking booking);
        Task<List<BookingViewModel>> GetAllBookingsAsync();
        Task<List<Booking>> GetActiveBooking();
        Task<List<BookingViewModel>> GetBookingsByFilterAsync(string roomTypeId, DateTime fromTime, DateTime endTime, bool IsTrashed);
        Task<List<BookingViewModel>> GetBookingByDate(DateTime fromTime, DateTime endTime, bool IsTrashed);
        Task<BookingViewModel> GetBookingByIdViewModel(string id);
        Task<List<BookingViewModel>> GetAllBookingByDate(DateTime fromTime, DateTime endTime);
        Task<Booking> GetBookingById(string id);
        Task<List<BookingViewModel>> GetOverStayedBooking();
        Task<List<BookingViewModel>> GetTodayBooking();
        Task<List<BookingViewModel>> GetCheckedOutBookingByDate(DateTime fromTime, DateTime endTime);
        Task<List<BookingViewModel>> GetRoomTypeBookingByFilter(string roomTypeId, DateTime fromTime, DateTime endTime);
        Task UpdateBooking(Booking booking);
        Task DeleteBooking(string id);
        Task<List<BookingViewModel>> GetRecycledBookings();
        Task<List<BookingViewModel>> SearchBooking(string keyword);
        int GetGuestBooking(string id);

        Task<List<BookingViewModel>> GetExpectedDepartureBooking(DateTime fromTime, DateTime endTime);
        Task<List<BookingViewModel>> GetInHouseGuest();
        Task<List<BookingViewModel>> GetOverstayedGuestsAsync(DateTime fromDate, DateTime toDate);
    }
}
