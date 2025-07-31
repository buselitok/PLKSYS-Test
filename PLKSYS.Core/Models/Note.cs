using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace PLKSYS.Core.Models
{
    public class Note
    {
        public int Id { get; set; }

        [Required]
        public string PlateNumber { get; set; } = string.Empty;

        [Required]
        public string NoteContent { get; set; } = string.Empty;

        [Required]
        public string UserId { get; set; } = string.Empty;

        [Required]
        public string UserName { get; set; } = string.Empty;

        [Required]
        public string Department { get; set; } = string.Empty;

        public DateTime Timestamp { get; set; } = DateTime.UtcNow;

        public bool IsPrivate { get; set; } = false; // Sadece yazan departman görebilir mi?

        // İlişki
        public Vehicle? Vehicle { get; set; }
    }

    public class NoteCreateDto
    {
        [Required(ErrorMessage = "Plaka numarası zorunludur.")]
        public string PlateNumber { get; set; } = string.Empty;

        [Required(ErrorMessage = "Not içeriği boş olamaz.")]
        [StringLength(500, MinimumLength = 5, ErrorMessage = "Not içeriği 5 ile 500 karakter arasında olmalıdır.")]
        public string NoteContent { get; set; } = string.Empty;

        public bool IsPrivate { get; set; } = false;
    }

}
