
using PLKSYS.Core.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PLKSYS.Core.Interfaces
{
    public interface INoteService
    {
        Task<List<Note>> GetAllNotesAsync();
        Task<List<Note>> GetNotesByPlateAsync(string plateNumber);
        Task<List<Note>> GetNotesByDepartmentAsync(string department);
        Task<List<Note>> GetPublicNotesAsync(string plateNumber); // Herkese açık notlar
        Task<Note?> CreateNoteAsync(NoteCreateDto noteDto, string userId, string userName, string department);
        Task<bool> UpdateNoteAsync(int noteId, string newContent, string userId);
        Task<bool> DeleteNoteAsync(int noteId, string userId, string userRole);
        Task<Note?> GetNoteByIdAsync(int noteId);



    }
}