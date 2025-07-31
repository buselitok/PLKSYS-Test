using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PLKSYS.Core.Models
{
    public class VehicleActivity
    {
        public int Id { get; set; }

        [Required]
        public string PlateNumber { get; set; } = string.Empty;

        [Required]
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;

        [Required]
        public string ActivityType { get; set; } = string.Empty; // "Entry", "Exit", "Reservation", "Washing", "Note", "StatusChange" vb.

        [Required]
        public string Department { get; set; } = string.Empty; // "Security", "Parking", "Washing", "Reception" vb.

        [Required]
        public string UserId { get; set; } = string.Empty;

        [Required]
        public string UserName { get; set; } = string.Empty; // Kullanıcı adı (görüntüleme için)

        public string Description { get; set; } = string.Empty; // İşlem açıklaması

        public string? PreviousValue { get; set; } // Önceki durum (durum değişiklikleri için)

        public string? NewValue { get; set; } // Yeni durum (durum değişiklikleri için)

        public string? AdditionalData { get; set; } // JSON formatında ekstra bilgiler

        // İlişki
        public Vehicle? Vehicle { get; set; }

        // Bu işlemin referans ettiği ana kayıt ID'si (reservation ID, washing ID vb.)
        public int? ReferenceId { get; set; }

        public string? ReferenceType { get; set; } // "Reservation", "Washing", "LoginRequest" vb.
    }

    // Araç işlem geçmişi oluşturma DTO'su
    public class VehicleActivityCreateDto
    {
        [Required]
        public string PlateNumber { get; set; } = string.Empty;

        [Required]
        public string ActivityType { get; set; } = string.Empty;

        [Required]
        public string Description { get; set; } = string.Empty;

        public string? PreviousValue { get; set; }
        public string? NewValue { get; set; }
        public string? AdditionalData { get; set; }
        public int? ReferenceId { get; set; }
        public string? ReferenceType { get; set; }
    }

   
    

    // Araç işlem geçmişi görüntüleme için DTO
    public class VehicleActivityViewModel
    {
        public int Id { get; set; }
        public string PlateNumber { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; }
        public string ActivityType { get; set; } = string.Empty;
        public string Department { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string? PreviousValue { get; set; }
        public string? NewValue { get; set; }
        public string? AdditionalData { get; set; }
        public int? ReferenceId { get; set; }
        public string? ReferenceType { get; set; }

        // Görüntüleme için formatlanmış metin
        public string FormattedDescription
        {
            get
            {
                var desc = Description;
                if (!string.IsNullOrEmpty(PreviousValue) && !string.IsNullOrEmpty(NewValue))
                {
                    desc += $" ({PreviousValue} → {NewValue})";
                }
                return desc;
            }
        }

        public string ActivityTypeDisplay
        {
            get
            {
                return ActivityType switch
                {
                    "Entry" => "Giriş",
                    "Exit" => "Çıkış",
                    "Reservation" => "Rezervasyon",
                    "Washing" => "Yıkama",
                    "Note" => "Not",
                    "StatusChange" => "Durum Değişikliği",
                    "Login" => "Giriş İsteği",
                    "Approval" => "Onay",
                    "Rejection" => "Red",
                    _ => ActivityType
                };
            }
        }
    }

    // Araç işlem türleri için enum
    public static class ActivityTypes
    {
        public const string Entry = "Entry";
        public const string Exit = "Exit";
        public const string Reservation = "Reservation";
        public const string Washing = "Washing";
        public const string Note = "Note";
        public const string StatusChange = "StatusChange";
        public const string Login = "Login";
        public const string Approval = "Approval";
        public const string Rejection = "Rejection";
        public const string QueueUpdate = "QueueUpdate";
        public const string Notification = "Notification";
    }
}
