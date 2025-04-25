using ESMART.Application.Common.Models;
using ESMART.Domain.Entities.FrontDesk;
using ESMART.Domain.ViewModels.FrontDesk;
using System;
using System.Collections.Generic;

namespace ESMART.Application.Common.Interface
{
    public interface IBookingRepository
    {
        Task<BookingResult> AddBooking(Booking booking);
        Task<List<BookingViewModel>> GetAllBookingsAsync();
        Task<List<BookingViewModel>> GetBookingsByFilterAsync(string roomTypeId, DateTime fromTime, DateTime endTime, bool IsTrashed);
        Task<List<BookingViewModel>> GetBookingByDate(DateTime fromTime, DateTime endTime, bool IsTrashed);
        Task<BookingViewModel> GetBookingByIdViewModel(string id);
        Task<List<BookingViewModel>> GetAllBookingByDate(DateTime fromTime, DateTime endTime);
        Task<List<BookingViewModel>> GetCheckedOutBookingByDate(DateTime fromTime, DateTime endTime);
        Task<List<BookingViewModel>> GetRoomTypeBookingByFilter(string roomTypeId, DateTime fromTime, DateTime endTime);
        Task<BookingResult> GetBookingById(string id);
        Task<BookingResult> UpdateBooking(Booking booking);
        Task<BookingResult> DeleteBooking(string id);
        Task<List<BookingViewModel>> GetRecycledBookings();
        Task<List<BookingViewModel>> SearchBooking(string keyword);
        int GetGuestBooking(string id);
    }
}
