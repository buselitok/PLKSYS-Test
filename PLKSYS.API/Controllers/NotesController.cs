using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using PLKSYS.Core.Interfaces;
using PLKSYS.Core.Models;
using PLKSYS.Core.Data;
using Microsoft.EntityFrameworkCore; // ClaimTypes için

namespace PLKSYS.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize] // Tüm kimliği doğrulanmış kullanıcılar not ekleyebilir/görebilir
    public class NotesController : ControllerBase
    {
        private readonly PLKSYSContext _context;
        private readonly INoteService _noteService;

        public NotesController(PLKSYSContext context, INoteService noteService)
        {
            _context = context;
            _noteService = noteService;
        }

        // GET: api/Notes/vehicle/{plateNumber}
        [HttpGet("vehicle/{plateNumber}")]
        public async Task<ActionResult<IEnumerable<Note>>> GetNotesForVehicle(string plateNumber)
        {
            return await _context.Notes.Where(n => n.PlateNumber == plateNumber).ToListAsync();
        }

        // GET: api/Notes/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Note>> GetNote(int id)
        {
            var note = await _context.Notes.FindAsync(id);

            if (note == null)
            {
                return NotFound();
            }

            return note;
        }

         

        // POST: api/Notes
        [HttpPost]
        public async Task<ActionResult<Note>> PostNote(NoteCreateDto noteDto)
        {
            // Giriş yapan kullanıcının ID'sini ve departmanını al
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var userDepartment = User.FindFirst("Department")?.Value; // Departman claim'ini al

            if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(userDepartment))
            {
                return Unauthorized("Kullanıcı bilgileri eksik. Not eklenemedi.");
            }

            var note = new Note
            {
                PlateNumber = noteDto.PlateNumber,
                NoteContent = noteDto.NoteContent,
                UserId = userId,
                Department = userDepartment,
                Timestamp = DateTime.UtcNow
            };

            _context.Notes.Add(note);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetNote", new { id = note.Id }, note);
        }

        [HttpPut("{id}")]
        [Authorize(Policy = "AdminOrSuperadmin")] // Politika kullanıldı
        public async Task<IActionResult> PutNote(int id, Note note)
        {
            if (id != note.Id)
            {
                return BadRequest();
            }

            _context.Entry(note).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!NoteExists(id))
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


        // DELETE: api/Notes/5
        [HttpDelete("{id}")]
        [Authorize(Policy = "AdminOrSuperadmin")] // Politika kullanıldı
        public async Task<IActionResult> DeleteNote(int id)
        {
            var note = await _context.Notes.FindAsync(id);
            if (note == null)
            {
                return NotFound();
            }

            _context.Notes.Remove(note);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool NoteExists(int id)
        {
            return _context.Notes.Any(e => e.Id == id);
        }
    }
}