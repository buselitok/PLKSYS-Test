using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PLKSYS.Core.Models
{
    public class Reservation
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        [StringLength(15)]
        public string PlateNumber { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string CustomerName { get; set; } = string.Empty;

        [Required]
        public DateTime ReservationDateTime { get; set; }

        [Required]
        [StringLength(50)]
        public string ServiceType { get; set; } = string.Empty; // "Periyodik Bakım", "Hasar Onarım", "Yeni Araç Satış", "Kasko Yenileme", "Diğer"

        [StringLength(500)]
        public string? Notes { get; set; }

        [Required]
        [StringLength(20)]
        public string Status { get; set; } = "Beklemede"; // "Confirmed", "Completed", "Cancelled", "Pending"

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        
    }

    public class ReservationCreateDto
    {
        private string _plateNumber = string.Empty;

        [Required(ErrorMessage = "Plaka numarası zorunludur.")]
        [StringLength(10, MinimumLength = 5, ErrorMessage = "Plaka numarası 5 ile 10 karakter arasında olmalıdır.")]
        public string PlateNumber
        {
            get => _plateNumber;
            set => _plateNumber = value.ToUpperInvariant(); // Plaka numarasını büyük harfe dönüştür
        }

        [Required(ErrorMessage = "Müşteri adı zorunludur.")]
        [StringLength(100, MinimumLength = 3, ErrorMessage = "Müşteri adı 3 ile 100 karakter arasında olmalıdır.")]
        public string CustomerName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Rezervasyon tarihi ve saati zorunludur.")]
        public DateTime ReservationDateTime { get; set; }

        [Required(ErrorMessage = "Hizmet türü zorunludur.")]
        
        public string ServiceType { get; set; } = string.Empty;

        [StringLength(500, ErrorMessage = "Notlar en fazla 500 karakter olmalıdır.")]
        public string? Notes { get; set; }
    }

    public class ReservationUpdateDto
    {
        public int Id { get; set; }

        private string _plateNumber = string.Empty;

        [Required(ErrorMessage = "Plaka numarası zorunludur.")]
        [StringLength(10, MinimumLength = 5, ErrorMessage = "Plaka numarası 5 ile 10 karakter arasında olmalıdır.")]
        public string PlateNumber
        {
            get => _plateNumber;
            set => _plateNumber = value.ToUpperInvariant(); // Plaka numarasını büyük harfe dönüştür
        }

        [Required(ErrorMessage = "Müşteri adı zorunludur.")]
        [StringLength(100, MinimumLength = 3, ErrorMessage = "Müşteri adı 3 ile 100 karakter arasında olmalıdır.")]
        public string CustomerName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Rezervasyon tarihi ve saati zorunludur.")]
        public DateTime ReservationDateTime { get; set; }

        [Required(ErrorMessage = "Hizmet türü zorunludur.")]
        
        public string ServiceType { get; set; } = string.Empty;

        [StringLength(500, ErrorMessage = "Notlar en fazla 500 karakter olmalıdır.")]
        public string? Notes { get; set; }

        [Required(ErrorMessage = "Durum zorunludur.")]
        
        public string Status { get; set; } = string.Empty;

        
    }
}

