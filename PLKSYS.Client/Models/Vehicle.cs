using System;
using System.Collections.Generic; // List için

namespace PLKSYS.Client.Models
{
    public class Vehicle
    {
        public string PlateNumber { get; set; } = string.Empty;
        public int Id { get; set; }
        public string CustomerName { get; set; } = string.Empty;
        public DateTime LastEntryTime { get; set; }
        public DateTime? LastExitTime { get; set; } // Nullable olabilir
        public string CurrentStatus { get; set; } = string.Empty; // "in" veya "out"
        public bool HasAppointment { get; set; }
        public DateTime? AppointmentDate { get; set; } // Nullable olabilir
        public string? AppointmentDetails { get; set; } // Nullable olabilir
        public bool HasToyotaAssistantPackage { get; set; }
        public string InsuranceStatus { get; set; } = string.Empty; // "Active", "Expired", "Pending"
        public bool PotentialSalesReferral { get; set; }
        public bool PotentialInsuranceReferral { get; set; }
        public List<Note> Notes { get; set; } = new List<Note>(); // Araca ait notlar
        public string? ServiceType { get; set; } // Bu satırı ekleyin
        public int? ReservationId { get; set; }

        // Claim alanları:
        public int? ClaimedByUserId { get; set; }
        public string? ClaimedByUserName { get; set; }
        public DateTime? ClaimedAt { get; set; }


    }
}
