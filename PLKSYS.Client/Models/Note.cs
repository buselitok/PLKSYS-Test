using System;

namespace PLKSYS.Client.Models
{
    public class Note
    {
        public int Id { get; set; }
        public string PlateNumber { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; }
        public string NoteContent { get; set; } = string.Empty;

        public string Department { get; set; } = string.Empty;
        public string UserId { get; set; } = string.Empty; // Notu ekleyen kullanıcının ID'si


        
    }

    // Not oluşturma için DTO
    public class NoteCreateDto
    {
        public string PlateNumber { get; set; } = string.Empty;
        public string NoteContent { get; set; } = string.Empty;
        // Departman ve UserId API tarafından otomatik eklenecek
    }
}