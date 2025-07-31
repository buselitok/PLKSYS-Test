using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PLKSYS.Core.Models
{
    public class Vehicle
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)] // ID'yi otomatik oluşturma, manuel atayacağız
        public string PlateNumber { get; set; } = string.Empty;

        public int Id { get; set; }
        public int? ReservationId { get; set; }
        public string CustomerName { get; set; } = string.Empty;
        public DateTime LastEntryTime { get; set; }
        public DateTime? LastExitTime { get; set; }
        public string CurrentStatus { get; set; } = "out"; // "in" veya "out"

        public bool HasAppointment { get; set; }
        public DateTime? AppointmentDate { get; set; }
        public string? AppointmentDetails { get; set; }

        public bool ToyotaAssistantPackage { get; set; }
        public string? ServiceType { get; set; } // Randevunun servis tipi (örn: "Periyodik Bakım", "Hasar Onarım")

        public string InsuranceStatus { get; set; } = "Yok"; // "Yok", "Toyota Kasko", "Diğer"
        public bool PotentialSalesReferral { get; set; } // Satışa yönlendirme potansiyeli
        public bool PotentialInsuranceReferral { get; set; } // Sigortaya yönlendirme potansiyeli

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;


        // Claim bilgileri
        public int? ClaimedByUserId { get; set; }
        public string? ClaimedByUserName { get; set; }
        public DateTime? ClaimedAt { get; set; }



        // YENİ ALANLAR - Giriş kartı sistemi için
        public string EntryType { get; set; } = "appointment"; // "appointment" veya "walk-in"
        public string? VisitorName { get; set; } // Randevusuz girişlerde ziyaretçi adı
        public string? VisitorSurname { get; set; } // Randevusuz girişlerde ziyaretçi soyadı
        public string? VisitedDepartment { get; set; } // Ziyaret edilecek birim
        public string? VisitedPersonnel { get; set; } // Ziyaret edilecek personel
        public string? VisitReason { get; set; } // Ziyaret nedeni
        public bool ExitApprovalRequired { get; set; } = true; // Çıkış onayı gerekli mi?
        public bool ExitApproved { get; set; } = false; // Muhasebe onayı verildi mi?
        public int? ExitApprovedByUserId { get; set; } // Onaylayan kullanıcı ID
        public string? ExitApprovedByUserName { get; set; } // Onaylayan kullanıcı adı
        public DateTime? ExitApprovedAt { get; set; } // Onay tarihi



        // İlişkiler
        public ICollection<Note> Notes { get; set; } = new List<Note>();
    }

    // Araç girişi için kullanılan DTO
    public class PlateEntryRequest
    {
        [Required(ErrorMessage = "Plaka numarası zorunludur.")]
        [StringLength(10, MinimumLength = 5, ErrorMessage = "Plaka numarası 5 ile 10 karakter arasında olmalıdır.")]
        public string PlateNumber { get; set; } = string.Empty;
    }

    // Randevulu giriş için
    public class AppointmentEntryRequest
    {
        [Required(ErrorMessage = "Plaka numarası zorunludur.")]
        [StringLength(10, MinimumLength = 5, ErrorMessage = "Plaka numarası 5 ile 10 karakter arasında olmalıdır.")]
        public string PlateNumber { get; set; } = string.Empty;
    }

    // Randevusuz giriş için
    public class WalkInEntryRequest
    {
        [Required(ErrorMessage = "Plaka numarası zorunludur.")]
        [StringLength(10, MinimumLength = 5, ErrorMessage = "Plaka numarası 5 ile 10 karakter arasında olmalıdır.")]
        public string PlateNumber { get; set; } = string.Empty;

        [Required(ErrorMessage = "Ziyaretçi adı zorunludür.")]
        [StringLength(50, ErrorMessage = "Ziyaretçi adı en fazla 50 karakter olabilir.")]
        public string VisitorName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Ziyaretçi soyadı zorunludür.")]
        [StringLength(50, ErrorMessage = "Ziyaretçi soyadı en fazla 50 karakter olabilir.")]
        public string VisitorSurname { get; set; } = string.Empty;

        [Required(ErrorMessage = "Ziyaret edilecek birim zorunludur.")]
        
        public string VisitedDepartment { get; set; } = string.Empty;

        [Required(ErrorMessage = "Ziyaret edilecek personel zorunludur.")]
        [StringLength(100, ErrorMessage = "Personel adı en fazla 100 karakter olabilir.")]
        public string VisitedPersonnel { get; set; } = string.Empty;

        [Required(ErrorMessage = "Ziyaret nedeni zorunludur.")]
        [StringLength(200, ErrorMessage = "Ziyaret nedeni en fazla 200 karakter olabilir.")]
        public string VisitReason { get; set; } = string.Empty;
    }

    // Çıkış onayı için
    public class ExitApprovalRequest
    {
        [Required(ErrorMessage = "Plaka numarası zorunludur.")]
        public string PlateNumber { get; set; } = string.Empty;

        public string? ApprovalNotes { get; set; } // Onay notları (opsiyonel)
    }
}