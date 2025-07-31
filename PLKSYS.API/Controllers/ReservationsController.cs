using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using System;
using PLKSYS.Core.Interfaces;
using PLKSYS.Core.Models;
using PLKSYS.Core.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity; // DateTime için

namespace PLKSYS.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    // Departman bazlı yetkilendirme: Bu controller'a sadece belirli departmanlar erişebilir
    [Authorize(Policy = "AllDepartments")] // Tüm departmanlar için politika kullanıldı
    public class ReservationsController : ControllerBase
    {
        private readonly PLKSYSContext _context;
        private readonly IUserService _userService;
        

        public ReservationsController(PLKSYSContext context, IUserService userService)
        {
            _context = context;
            _userService = userService;
            
        }

        // GET: api/Reservations
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Reservation>>> GetReservations()
        {
            return await _context.Reservations.ToListAsync();
        }

        // GET: api/Reservations/upcoming?date=2023-10-26
        [HttpGet("upcoming")]
        public async Task<ActionResult<IEnumerable<Reservation>>> GetUpcomingReservations([FromQuery] DateTime date)
        {
            return await _context.Reservations
                                 .Where(r => r.ReservationDateTime.Date == date.Date && r.Status == "Onaylandı")
                                 .ToListAsync();
        }

        // GET: api/Reservations/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Reservation>> GetReservation(int id)
        {
            var reservation = await _context.Reservations.FindAsync(id);

            if (reservation == null)
            {
                return NotFound();
            }

            return reservation;
        }

        

        // PUT: api/Reservations/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutReservation(int id, ReservationUpdateDto reservationDto)
        {
            if (id != reservationDto.Id)
            {
                return BadRequest();
            }

            var reservation = await _context.Reservations.FindAsync(id);
            if (reservation == null)
            {
                return NotFound();
            }

            // PlateNumber, DTO'dan gelirken zaten büyük harfe dönüştürülmüş olacak.
            reservation.PlateNumber = reservationDto.PlateNumber;
            reservation.CustomerName = reservationDto.CustomerName;
            reservation.ReservationDateTime = reservationDto.ReservationDateTime;
            reservation.ServiceType = reservationDto.ServiceType;
            reservation.Notes = reservationDto.Notes;
            reservation.Status = reservationDto.Status;
            reservation.UpdatedAt = DateTime.UtcNow;

            _context.Entry(reservation).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ReservationExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // POST: api/Reservations
        [HttpPost]
        public async Task<ActionResult<Reservation>> PostReservation(ReservationCreateDto reservationDto)
        {
            var reservation = new Reservation
            {
                // PlateNumber, DTO'dan gelirken zaten büyük harfe dönüştürülmüş olacak.
                PlateNumber = reservationDto.PlateNumber,
                CustomerName = reservationDto.CustomerName,
                ReservationDateTime = reservationDto.ReservationDateTime,
                ServiceType = reservationDto.ServiceType,
                Notes = reservationDto.Notes,
                Status = "Onaylandı", // Varsayılan olarak onaylandı
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.Reservations.Add(reservation);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetReservation", new { id = reservation.Id }, reservation);
        }

        // DELETE: api/Reservations/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteReservation(int id)
        {
            var reservation = await _context.Reservations.FindAsync(id);
            if (reservation == null)
            {
                return NotFound();
            }

            _context.Reservations.Remove(reservation);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        

        private bool ReservationExists(int id)
        {
            return _context.Reservations.Any(e => e.Id == id);
        }
    }
}