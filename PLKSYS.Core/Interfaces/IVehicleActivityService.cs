using PLKSYS.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PLKSYS.Core.Interfaces
{
    public interface IVehicleActivityService
    {
        // Araç işlem geçmişi alma
        Task<List<VehicleActivityViewModel>> GetVehicleActivitiesAsync(string plateNumber);
        Task<List<VehicleActivityViewModel>> GetAllActivitiesAsync(int pageNumber = 1, int pageSize = 50);
        Task<List<VehicleActivityViewModel>> GetActivitiesByDepartmentAsync(string department, int pageNumber = 1, int pageSize = 50);
        Task<List<VehicleActivityViewModel>> GetActivitiesByTypeAsync(string activityType, int pageNumber = 1, int pageSize = 50);
        Task<List<VehicleActivityViewModel>> GetRecentActivitiesAsync(int count = 20);

        // İşlem ekleme
        Task<VehicleActivity> LogActivityAsync(VehicleActivityCreateDto activityDto, string userId, string userName, string department);
        Task<VehicleActivity> LogActivityAsync(string plateNumber, string activityType, string description, string userId, string userName, string department, string? previousValue = null, string? newValue = null, int? referenceId = null, string? referenceType = null);

        // Özel log metodları (farklı işlem türleri için)
        Task LogVehicleEntryAsync(string plateNumber, string userId, string userName, string description = "Araç plaza girişi yapıldı");
        Task LogVehicleExitAsync(string plateNumber, string userId, string userName, string description = "Araç plaza çıkışı yapıldı");
        Task LogReservationAsync(string plateNumber, int reservationId, string userId, string userName, string description);
        Task LogWashingAsync(string plateNumber, int washingId, string userId, string userName, string description);
        Task LogStatusChangeAsync(string plateNumber, string previousStatus, string newStatus, string userId, string userName, string department);
        Task LogApprovalAsync(string plateNumber, string approvalType, int referenceId, string userId, string userName, string department);
        Task LogRejectionAsync(string plateNumber, string rejectionType, int referenceId, string userId, string userName, string department, string reason);

        // İstatistikler
        Task<Dictionary<string, int>> GetActivityStatsAsync(DateTime? startDate = null, DateTime? endDate = null);
        Task<Dictionary<string, int>> GetDepartmentActivityStatsAsync(DateTime? startDate = null, DateTime? endDate = null);
    }

    // Güncellenmiş Note interface
    
}
