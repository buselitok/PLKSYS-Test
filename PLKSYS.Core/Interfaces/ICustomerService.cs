using PLKSYS.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PLKSYS.Core.Interfaces
{
    public interface ICustomerService
    {
        // Müşteri kayıt işlemleri
        Task<Customer?> RegisterCustomerAsync(CustomerRegistrationRequest request);
        Task<Customer?> GetCustomerByIdAsync(int customerId);
        
        Task<List<Customer>> GetAllCustomersAsync();

        // Yaya ziyaret işlemleri
        Task<WalkInVisit?> RegisterWalkInVisitAsync(WalkInVisitRequest request);
        Task<List<WalkInVisit>> GetActiveWalkInVisitsAsync();
        Task<List<WalkInVisit>> GetWalkInVisitsByDepartmentAsync(string department);
        Task<bool> CompleteWalkInVisitAsync(int visitId);
        Task<bool> CancelWalkInVisitAsync(int visitId);

        // Araç durumu sorgulama
        Task<VehicleStatusResponse?> GetVehicleStatusAsync(string plateNumber);
    }

    // Araç durumu response modeli
    public class VehicleStatusResponse
    {
        public string PlateNumber { get; set; } = string.Empty;
        public string CustomerName { get; set; } = string.Empty;
        public string CurrentStatus { get; set; } = string.Empty; // in, out
        public string? ServiceType { get; set; }
        public DateTime? LastEntryTime { get; set; }
        public DateTime? LastExitTime { get; set; }
        public bool HasAppointment { get; set; }
        public DateTime? AppointmentDate { get; set; }
        public string? AppointmentDetails { get; set; }

        // Servis durumu bilgileri
        public string? ClaimedByUserName { get; set; }
        public DateTime? ClaimedAt { get; set; }
        public bool ExitApprovalRequired { get; set; }
        public bool ExitApproved { get; set; }

        // Son notlar (son 3 not)
        public List<VehicleNoteInfo> RecentNotes { get; set; } = new List<VehicleNoteInfo>();
    }

    public class VehicleNoteInfo
    {
        public string Content { get; set; } = string.Empty;
        public string CreatedByUserName { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
    }
}
