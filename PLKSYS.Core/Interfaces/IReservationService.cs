using PLKSYS.Core.Models;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;

namespace PLKSYS.Core.Interfaces
{
    public interface IReservationService
    {
        Task<List<Reservation>> GetAllReservations();
        Task<List<Reservation>> GetUpcomingReservations(DateTime? date = null);
        Task<Reservation?> GetReservationById(int id);
        Task<Reservation?> CreateReservation(ReservationCreateDto reservationDto);
        Task<bool> UpdateReservation(int id, ReservationUpdateDto reservationDto);
        Task<bool> DeleteReservation(int id);
    }
}