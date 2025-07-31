using Microsoft.EntityFrameworkCore;
using PLKSYS.Core.Data;
using PLKSYS.Core.Interfaces;
using PLKSYS.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PLKSYS.Core.Services
{
    public class VehicleActivityService : IVehicleActivityService
    {
        private readonly PLKSYSContext _context;

        public VehicleActivityService(PLKSYSContext context)
        {
            _context = context;
        }

        public async Task<List<VehicleActivityViewModel>> GetVehicleActivitiesAsync(string plateNumber)
        {
            var activities = await _context.VehicleActivities
                .Where(va => va.PlateNumber == plateNumber.ToUpper())
                .OrderByDescending(va => va.Timestamp)
                .Select(va => new VehicleActivityViewModel
                {
                    Id = va.Id,
                    PlateNumber = va.PlateNumber,
                    Timestamp = va.Timestamp,
                    ActivityType = va.ActivityType,
                    Department = va.Department,
                    UserName = va.UserName,
                    Description = va.Description,
                    PreviousValue = va.PreviousValue,
                    NewValue = va.NewValue,
                    AdditionalData = va.AdditionalData,
                    ReferenceId = va.ReferenceId,
                    ReferenceType = va.ReferenceType
                })
                .ToListAsync();

            return activities;
        }

        public async Task<List<VehicleActivityViewModel>> GetAllActivitiesAsync(int pageNumber = 1, int pageSize = 50)
        {
            var activities = await _context.VehicleActivities
                .OrderByDescending(va => va.Timestamp)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(va => new VehicleActivityViewModel
                {
                    Id = va.Id,
                    PlateNumber = va.PlateNumber,
                    Timestamp = va.Timestamp,
                    ActivityType = va.ActivityType,
                    Department = va.Department,
                    UserName = va.UserName,
                    Description = va.Description,
                    PreviousValue = va.PreviousValue,
                    NewValue = va.NewValue,
                    AdditionalData = va.AdditionalData,
                    ReferenceId = va.ReferenceId,
                    ReferenceType = va.ReferenceType
                })
                .ToListAsync();

            return activities;
        }

        public async Task<List<VehicleActivityViewModel>> GetActivitiesByDepartmentAsync(string department, int pageNumber = 1, int pageSize = 50)
        {
            var activities = await _context.VehicleActivities
                .Where(va => va.Department == department)
                .OrderByDescending(va => va.Timestamp)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(va => new VehicleActivityViewModel
                {
                    Id = va.Id,
                    PlateNumber = va.PlateNumber,
                    Timestamp = va.Timestamp,
                    ActivityType = va.ActivityType,
                    Department = va.Department,
                    UserName = va.UserName,
                    Description = va.Description,
                    PreviousValue = va.PreviousValue,
                    NewValue = va.NewValue,
                    AdditionalData = va.AdditionalData,
                    ReferenceId = va.ReferenceId,
                    ReferenceType = va.ReferenceType
                })
                .ToListAsync();

            return activities;
        }

        public async Task<List<VehicleActivityViewModel>> GetActivitiesByTypeAsync(string activityType, int pageNumber = 1, int pageSize = 50)
        {
            var activities = await _context.VehicleActivities
                .Where(va => va.ActivityType == activityType)
                .OrderByDescending(va => va.Timestamp)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(va => new VehicleActivityViewModel
                {
                    Id = va.Id,
                    PlateNumber = va.PlateNumber,
                    Timestamp = va.Timestamp,
                    ActivityType = va.ActivityType,
                    Department = va.Department,
                    UserName = va.UserName,
                    Description = va.Description,
                    PreviousValue = va.PreviousValue,
                    NewValue = va.NewValue,
                    AdditionalData = va.AdditionalData,
                    ReferenceId = va.ReferenceId,
                    ReferenceType = va.ReferenceType
                })
                .ToListAsync();

            return activities;
        }

        public async Task<List<VehicleActivityViewModel>> GetRecentActivitiesAsync(int count = 20)
        {
            var activities = await _context.VehicleActivities
                .OrderByDescending(va => va.Timestamp)
                .Take(count)
                .Select(va => new VehicleActivityViewModel
                {
                    Id = va.Id,
                    PlateNumber = va.PlateNumber,
                    Timestamp = va.Timestamp,
                    ActivityType = va.ActivityType,
                    Department = va.Department,
                    UserName = va.UserName,
                    Description = va.Description,
                    PreviousValue = va.PreviousValue,
                    NewValue = va.NewValue,
                    AdditionalData = va.AdditionalData,
                    ReferenceId = va.ReferenceId,
                    ReferenceType = va.ReferenceType
                })
                .ToListAsync();

            return activities;
        }

        public async Task<VehicleActivity> LogActivityAsync(VehicleActivityCreateDto activityDto, string userId, string userName, string department)
        {
            var activity = new VehicleActivity
            {
                PlateNumber = activityDto.PlateNumber.ToUpper(),
                ActivityType = activityDto.ActivityType,
                Department = department,
                UserId = userId,
                UserName = userName,
                Description = activityDto.Description,
                PreviousValue = activityDto.PreviousValue,
                NewValue = activityDto.NewValue,
                AdditionalData = activityDto.AdditionalData,
                ReferenceId = activityDto.ReferenceId,
                ReferenceType = activityDto.ReferenceType,
                Timestamp = DateTime.UtcNow
            };

            await _context.VehicleActivities.AddAsync(activity);
            await _context.SaveChangesAsync();

            return activity;
        }

        public async Task<VehicleActivity> LogActivityAsync(string plateNumber, string activityType, string description, string userId, string userName, string department, string? previousValue = null, string? newValue = null, int? referenceId = null, string? referenceType = null)
        {
            var activity = new VehicleActivity
            {
                PlateNumber = plateNumber.ToUpper(),
                ActivityType = activityType,
                Department = department,
                UserId = userId,
                UserName = userName,
                Description = description,
                PreviousValue = previousValue,
                NewValue = newValue,
                ReferenceId = referenceId,
                ReferenceType = referenceType,
                Timestamp = DateTime.UtcNow
            };

            await _context.VehicleActivities.AddAsync(activity);
            await _context.SaveChangesAsync();

            return activity;
        }

        // Özel log metodları
        public async Task LogVehicleEntryAsync(string plateNumber, string userId, string userName, string description = "Araç plaza girişi yapıldı")
        {
            await LogActivityAsync(plateNumber, ActivityTypes.Entry, description, userId, userName, "Security");
        }

        public async Task LogVehicleExitAsync(string plateNumber, string userId, string userName, string description = "Araç plaza çıkışı yapıldı")
        {
            await LogActivityAsync(plateNumber, ActivityTypes.Exit, description, userId, userName, "Security");
        }

        public async Task LogReservationAsync(string plateNumber, int reservationId, string userId, string userName, string description)
        {
            await LogActivityAsync(plateNumber, ActivityTypes.Reservation, description, userId, userName, "Reception", referenceId: reservationId, referenceType: "Reservation");
        }

        public async Task LogWashingAsync(string plateNumber, int washingId, string userId, string userName, string description)
        {
            await LogActivityAsync(plateNumber, ActivityTypes.Washing, description, userId, userName, "Washing", referenceId: washingId, referenceType: "Washing");
        }

        public async Task LogStatusChangeAsync(string plateNumber, string previousStatus, string newStatus, string userId, string userName, string department)
        {
            var description = $"Araç durumu değiştirildi";
            await LogActivityAsync(plateNumber, ActivityTypes.StatusChange, description, userId, userName, department, previousStatus, newStatus);
        }

        public async Task LogApprovalAsync(string plateNumber, string approvalType, int referenceId, string userId, string userName, string department)
        {
            var description = $"{approvalType} onaylandı";
            await LogActivityAsync(plateNumber, ActivityTypes.Approval, description, userId, userName, department, referenceId: referenceId, referenceType: approvalType);
        }

        public async Task LogRejectionAsync(string plateNumber, string rejectionType, int referenceId, string userId, string userName, string department, string reason)
        {
            var description = $"{rejectionType} reddedildi: {reason}";
            await LogActivityAsync(plateNumber, ActivityTypes.Rejection, description, userId, userName, department, referenceId: referenceId, referenceType: rejectionType);
        }

        // İstatistikler
        public async Task<Dictionary<string, int>> GetActivityStatsAsync(DateTime? startDate = null, DateTime? endDate = null)
        {
            var query = _context.VehicleActivities.AsQueryable();

            if (startDate.HasValue)
                query = query.Where(va => va.Timestamp >= startDate.Value);

            if (endDate.HasValue)
                query = query.Where(va => va.Timestamp <= endDate.Value);

            var stats = await query
                .GroupBy(va => va.ActivityType)
                .Select(g => new { ActivityType = g.Key, Count = g.Count() })
                .ToDictionaryAsync(x => x.ActivityType, x => x.Count);

            return stats;
        }

        public async Task<Dictionary<string, int>> GetDepartmentActivityStatsAsync(DateTime? startDate = null, DateTime? endDate = null)
        {
            var query = _context.VehicleActivities.AsQueryable();

            if (startDate.HasValue)
                query = query.Where(va => va.Timestamp >= startDate.Value);

            if (endDate.HasValue)
                query = query.Where(va => va.Timestamp <= endDate.Value);

            var stats = await query
                .GroupBy(va => va.Department)
                .Select(g => new { Department = g.Key, Count = g.Count() })
                .ToDictionaryAsync(x => x.Department, x => x.Count);

            return stats;
        }
    }
}
