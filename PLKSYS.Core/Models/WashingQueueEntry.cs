using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PLKSYS.Core.Models
{
    public class WashingQueueEntry
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid Id { get; set; } = Guid.NewGuid(); // Benzersiz kimlik

        [Required]
        [StringLength(15)]
        public string PlateNumber { get; set; } = string.Empty; // Aracın plakası

        public DateTime SentToWashingTime { get; set; } = DateTime.UtcNow; // Yıkamaya gönderildiği saat (UTC)

        public DateTime? WashingCompletedTime { get; set; } // Yıkamanın tamamlandığı saat (UTC)

        [StringLength(100)]
        public string SentByUserName { get; set; } = string.Empty; // Yıkamaya gönderen kullanıcının adı

        [StringLength(100)]
        public string? CompletedByUserName { get; set; } // Yıkamayı tamamlayan kullanıcının adı

        [Required]
        [StringLength(20)]
        public string Status { get; set; } = "Beklemede"; // "Pending" (Beklemede), "Completed" (Tamamlandı)
    }

    public class WashingQueueEntryCreateDto
    {
        [Required(ErrorMessage = "Plaka numarası zorunludur.")]
        [StringLength(10, MinimumLength = 5, ErrorMessage = "Plaka numarası 5 ile 10 karakter arasında olmalıdır.")]
        public string PlateNumber { get; set; } = string.Empty;
    }
}
