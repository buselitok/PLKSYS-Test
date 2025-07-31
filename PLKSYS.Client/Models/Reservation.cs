using System;
using System.ComponentModel.DataAnnotations;

namespace PLKSYS.Client.Models
{
    public class Reservation
    {
        public int Id { get; set; }
        public string PlateNumber { get; set; } = string.Empty;
        public string CustomerName { get; set; } = string.Empty;
        public DateTime ReservationDateTime { get; set; }
        public string ServiceType { get; set; } = string.Empty;
        public string? Notes { get; set; }
        public string Status { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }

        
    }

    // Rezervasyon oluşturma için DTO
    public class ReservationCreateDto
    {
        [Required(ErrorMessage = "Plaka numarası zorunludur.")]
        [StringLength(9, ErrorMessage = "Plaka numarası 9 karakterden uzun olamaz.")]
        public string PlateNumber { get; set; } = string.Empty;

        [Required(ErrorMessage = "Müşteri adı zorunludur.")]
        
        public string CustomerName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Rezervasyon tarihi ve saati zorunludur.")]
        public DateTime ReservationDateTime { get; set; }

        [Required(ErrorMessage = "Hizmet türü zorunludur.")]
        
        public string ServiceType { get; set; } = string.Empty;

        [StringLength(100, ErrorMessage = "Notlar 100 karakterden uzun olamaz.")]
        public string? Notes { get; set; }

        [Required(ErrorMessage = "Durum zorunludur.")]
        
        public string Status { get; set; } = "Onaylandı";


    }

    // Rezervasyon güncelleme için DTO (Create DTO'dan miras alabilir veya ayrı olabilir)
    public class ReservationUpdateDto : ReservationCreateDto
    {
        [Required]
        public int Id { get; set; }
    }
}