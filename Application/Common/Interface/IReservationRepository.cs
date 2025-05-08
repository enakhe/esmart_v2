using ESMART.Domain.Entities.FrontDesk;
using ESMART.Domain.Enum;
using ESMART.Domain.ViewModels.FrontDesk;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ESMART.Application.Common.Interface
{
    public interface IReservationRepository
    {
        Task AddReservationAsync(Reservation reservation);
        Task<Reservation> GetReservationByIdAsync(string id);
        Task<List<ReservationViewModel>> GetAllReservationsAsync();
        Task UpdateReservationAsync(Reservation reservation);
        Task DeleteReservationAsync(string id);
        Task<List<ReservationViewModel>> GetReservationsByGuestIdAsync(string guestId);
        Task<List<ReservationViewModel>> GetTodayReservationsAsync();
        Task<bool> CanExtendStayAsync(string reservationId, string roomId, DateTime newDepartureDate);
        Task<List<ReservationViewModel>> GetReservationsByRoomIdAsync(string roomId);
        Task<List<ReservationViewModel>> GetReservationsByApplicationUserIdAsync(string applicationUserId);
        Task<List<ReservationViewModel>> GetReservationsByDateRangeAsync(DateTime startDate, DateTime endDate);
        Task<List<ReservationViewModel>> GetReservationsByStatusAsync(ReservationStatus status);
        Task<List<ReservationViewModel>> GetReservationsByRoomNoAndDateRangeAsync(string roomNo, DateTime startDate, DateTime endDate);
        Task<List<ReservationViewModel>> GetReservationsByPaymentStatusAsync(TransactionStatus paymentStatus);
        Task<List<ReservationViewModel>> GetCancelledReservationsAsync();
    }
}
