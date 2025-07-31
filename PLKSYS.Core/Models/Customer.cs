using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PLKSYS.Core.Models
{
    public class Customer
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Ad zorunludur.")]
        [StringLength(50, ErrorMessage = "Ad en fazla 50 karakter olabilir.")]
        public string FirstName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Soyad zorunludur.")]
        [StringLength(50, ErrorMessage = "Soyad en fazla 50 karakter olabilir.")]
        public string LastName { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // İlişkiler
        public ICollection<WalkInVisit> WalkInVisits { get; set; } = new List<WalkInVisit>();
        public ICollection<Vehicle> OwnedVehicles { get; set; } = new List<Vehicle>();
    }

    // Yaya ziyaretçi kayıtları için model
    public class WalkInVisit
    {
        [Key]
        public int Id { get; set; }

        public int CustomerId { get; set; }
        public Customer Customer { get; set; } = null!;

        [Required(ErrorMessage = "Ziyaret edilecek birim zorunludur.")]
        public string VisitedDepartment { get; set; } = string.Empty;

        [Required(ErrorMessage = "Ziyaret edilecek personel zorunludur.")]
        [StringLength(100, ErrorMessage = "Personel adı en fazla 100 karakter olabilir.")]
        public string VisitedPersonnel { get; set; } = string.Empty;

        [Required(ErrorMessage = "Ziyaret nedeni zorunludur.")]
        [StringLength(200, ErrorMessage = "Ziyaret nedeni en fazla 200 karakter olabilir.")]
        public string VisitReason { get; set; } = string.Empty;

        public DateTime VisitTime { get; set; } = DateTime.UtcNow;
        public string Status { get; set; } = "Aktif"; // Active, Completed, Cancelled

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? CompletedAt { get; set; }

        // Bildirim gönderildi mi?
        public bool NotificationSent { get; set; } = false;
        public DateTime? NotificationSentAt { get; set; }
    }

    // Müşteri kayıt formu için DTO
    public class CustomerRegistrationRequest
    {
        [Required(ErrorMessage = "Ad zorunludur.")]
        [StringLength(50, ErrorMessage = "Ad en fazla 50 karakter olabilir.")]
        public string FirstName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Soyad zorunludur.")]
        [StringLength(50, ErrorMessage = "Soyad en fazla 50 karakter olabilir.")]
        public string LastName { get; set; } = string.Empty;

        
    }

    // Yaya ziyaret kayıt formu için DTO
    public class WalkInVisitRequest
    {
        public int CustomerId { get; set; }

        [Required(ErrorMessage = "Ziyaret edilecek birim zorunludur.")]
        public string VisitedDepartment { get; set; } = string.Empty;

        [Required(ErrorMessage = "Ziyaret edilecek personel zorunludur.")]
        [StringLength(100, ErrorMessage = "Personel adı en fazla 100 karakter olabilir.")]
        public string VisitedPersonnel { get; set; } = string.Empty;

        [Required(ErrorMessage = "Ziyaret nedeni zorunludur.")]
        [StringLength(200, ErrorMessage = "Ziyaret nedeni en fazla 200 karakter olabilir.")]
        public string VisitReason { get; set; } = string.Empty;
    }

    // Araç durumu sorgulama için DTO
    public class VehicleStatusRequest
    {
        [Required(ErrorMessage = "Plaka numarası zorunludur.")]
        [StringLength(10, MinimumLength = 5, ErrorMessage = "Plaka numarası 5 ile 10 karakter arasında olmalıdır.")]
        public string PlateNumber { get; set; } = string.Empty;
    }
}

