using PLKSYS.Core.Data;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using PLKSYS.Core.Models;
using PLKSYS.Core.Interfaces; // OrderByDescending için

namespace PLKSYS.Core.Services
{
    public class NoteService : INoteService
    {
        private readonly PLKSYSContext _context;
        private readonly IVehicleActivityService _activityService;

        public NoteService(PLKSYSContext context, IVehicleActivityService activityService)
        {
            _context = context;
            _activityService = activityService;
        }

        public async Task<List<Note>> GetAllNotesAsync()
        {
            return await _context.Notes.OrderByDescending(n => n.Timestamp).ToListAsync();
        }

        public async Task<List<Note>> GetNotesByPlateAsync(string plateNumber)
        {
            return await _context.Notes
                .Where(n => n.PlateNumber == plateNumber.ToUpper())
                .OrderByDescending(n => n.Timestamp)
                .ToListAsync();
        }

        public async Task<List<Note>> GetNotesByDepartmentAsync(string department)
        {
            return await _context.Notes
                .Where(n => n.Department == department || !n.IsPrivate)
                .OrderByDescending(n => n.Timestamp)
                .ToListAsync();
        }

        public async Task<List<Note>> GetPublicNotesAsync(string plateNumber)
        {
            return await _context.Notes
                .Where(n => n.PlateNumber == plateNumber.ToUpper() && !n.IsPrivate)
                .OrderByDescending(n => n.Timestamp)
                .ToListAsync();
        }

        public async Task<Note?> CreateNoteAsync(NoteCreateDto noteDto, string userId, string userName, string department)
        {
            var note = new Note
            {
                PlateNumber = noteDto.PlateNumber.ToUpper(),
                NoteContent = noteDto.NoteContent,
                UserId = userId,
                UserName = userName,
                Department = department,
                IsPrivate = noteDto.IsPrivate,
                Timestamp = DateTime.UtcNow
            };

            await _context.Notes.AddAsync(note);
            await _context.SaveChangesAsync();

            // Not eklenmesini activity log'a kaydet
            var activityDescription = noteDto.IsPrivate ? "Özel not eklendi" : "Not eklendi";
            await _activityService.LogActivityAsync(
                noteDto.PlateNumber,
                ActivityTypes.Note,
                activityDescription,
                userId,
                userName,
                department,
                referenceId: note.Id,
                referenceType: "Note"
            );

            return note;
        }

        public async Task<bool> UpdateNoteAsync(int noteId, string newContent, string userId)
        {
            var note = await _context.Notes.FindAsync(noteId);
            if (note == null || note.UserId != userId)
                return false;

            var oldContent = note.NoteContent;
            note.NoteContent = newContent;

            await _context.SaveChangesAsync();

            // Not güncellemesini activity log'a kaydet
            await _activityService.LogActivityAsync(
                note.PlateNumber,
                ActivityTypes.Note,
                "Not güncellendi",
                userId,
                note.UserName,
                note.Department,
                oldContent,
                newContent,
                referenceId: note.Id,
                referenceType: "Note"
            );

            return true;
        }

        public async Task<bool> DeleteNoteAsync(int noteId, string userId, string userRole)
        {
            var note = await _context.Notes.FindAsync(noteId);
            if (note == null)
                return false;

            // Sadece not sahibi veya admin/superadmin silebilir
            if (note.UserId != userId && userRole != "Admin" && userRole != "Superadmin")
                return false;

            _context.Notes.Remove(note);
            await _context.SaveChangesAsync();

            // Not silmesini activity log'a kaydet
            await _activityService.LogActivityAsync(
                note.PlateNumber,
                ActivityTypes.Note,
                "Not silindi",
                userId,
                note.UserName,
                note.Department,
                referenceId: noteId,
                referenceType: "Note"
            );

            return true;
        }

        public async Task<Note?> GetNoteByIdAsync(int noteId)
        {
            return await _context.Notes.FindAsync(noteId);
        }
    }
}