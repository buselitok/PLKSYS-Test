using PLKSYS.Core.Data;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using PLKSYS.Core.Models;
using PLKSYS.Core.Interfaces; // OrderByDescending için

namespace PLKSYS.Core.Services
{
    public class ReservationService : IReservationService
    {
        private readonly PLKSYSContext _context;

        public ReservationService(PLKSYSContext context)
        {
            _context = context;
        }

        public async Task<List<Reservation>> GetAllReservations()
        {
            return await _context.Reservations.OrderByDescending(r => r.ReservationDateTime).ToListAsync();
        }

        public async Task<List<Reservation>> GetUpcomingReservations(DateTime? date = null)
        {
            var query = _context.Reservations.AsQueryable();

            if (date.HasValue)
            {
                // Belirli bir tarihten sonraki rezervasyonları getir
                query = query.Where(r => r.ReservationDateTime.Date >= date.Value.Date);
            }
            else
            {
                // Varsayılan olarak bugünden sonraki rezervasyonları getir
                query = query.Where(r => r.ReservationDateTime >= DateTime.UtcNow);
            }

            return await query.OrderBy(r => r.ReservationDateTime).ToListAsync();
        }

        public async Task<Reservation?> GetReservationById(int id)
        {
            return await _context.Reservations.FindAsync(id);
        }

        public async Task<Reservation?> CreateReservation(ReservationCreateDto reservationDto)
        {
            var newReservation = new Reservation
            {
                PlateNumber = reservationDto.PlateNumber.ToUpper(),
                CustomerName = reservationDto.CustomerName,
                ReservationDateTime = reservationDto.ReservationDateTime.ToUniversalTime(), // UTC olarak kaydet
                ServiceType = reservationDto.ServiceType,
                Notes = reservationDto.Notes,
                Status = "Onaylandı",
                CreatedAt = DateTime.UtcNow
            };

            await _context.Reservations.AddAsync(newReservation);
            await _context.SaveChangesAsync();
            return newReservation;
        }

        public async Task<bool> UpdateReservation(int id, ReservationUpdateDto reservationDto)
        {
            var reservationToUpdate = await _context.Reservations.FindAsync(id);
            if (reservationToUpdate == null)
            {
                return false; // Rezervasyon bulunamadı
            }

            reservationToUpdate.PlateNumber = reservationDto.PlateNumber.ToUpper();
            reservationToUpdate.CustomerName = reservationDto.CustomerName;
            reservationToUpdate.ReservationDateTime = reservationDto.ReservationDateTime.ToUniversalTime();
            reservationToUpdate.ServiceType = reservationDto.ServiceType;
            reservationToUpdate.Notes = reservationDto.Notes;
            reservationToUpdate.Status = reservationDto.Status;

            _context.Reservations.Update(reservationToUpdate);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteReservation(int id)
        {
            var reservationToDelete = await _context.Reservations.FindAsync(id);
            if (reservationToDelete == null)
            {
                return false; // Rezervasyon bulunamadı
            }

            _context.Reservations.Remove(reservationToDelete);
            await _context.SaveChangesAsync();
            return true;
        }
        
    }
}