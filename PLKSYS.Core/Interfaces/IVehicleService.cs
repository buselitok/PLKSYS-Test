using PLKSYS.Core.Models;
using System.Collections.Generic;
using System.Threading.Tasks;
using static PLKSYS.Core.Models.Vehicle;

namespace PLKSYS.Core.Interfaces
{
    public interface IVehicleService
    {
        Task<List<Vehicle>> GetLiveVehicles();
        Task<List<Vehicle>> GetVehicleLogs();
        Task<Vehicle?> GetVehicleByPlate(string plateNumber);
        Task<Vehicle?> RegisterVehicleEntry(PlateEntryRequest request);
        Task<Vehicle?> RegisterVehicleExit(PlateEntryRequest request);
        //Task<bool> ClaimVehicleAsync(int vehicleId, int userId, string userName);
        Task<bool> ClaimVehicle(string plateNumber, int userId, string userName);
        Task<bool> UnclaimVehicle(string plateNumber, int userId);

        Task<bool> UpdateVehicleClaimStatus(Vehicle vehicle);
        Task<Vehicle?> RegisterAppointmentEntry(AppointmentEntryRequest request);
        Task<Vehicle?> RegisterWalkInEntry(WalkInEntryRequest request);
        Task<List<Vehicle>> GetVehiclesPendingExitApproval();
        Task<bool> ApproveVehicleExit(string plateNumber, int approverUserId, string approverUserName, string? approvalNotes = null);
        Task<bool> RevokeExitApproval(string plateNumber, int revokerUserId, string revokerUserName, string reason);
    }
}