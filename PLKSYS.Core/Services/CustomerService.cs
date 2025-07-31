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
    public class CustomerService: ICustomerService
    {
        private readonly PLKSYSContext _context;

        public CustomerService(PLKSYSContext context)
        {
            _context = context;
        }

        // Müşteri kayıt işlemleri
        public async Task<Customer?> RegisterCustomerAsync(CustomerRegistrationRequest request)
        {
            try
            { 
               

                var customer = new Customer
                {
                    FirstName = request.FirstName,
                    LastName = request.LastName,
                    
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                await _context.Customers.AddAsync(customer);
                await _context.SaveChangesAsync();

                return customer;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"RegisterCustomerAsync error: {ex.Message}");
                return null;
            }
        }

        public async Task<Customer?> GetCustomerByIdAsync(int customerId)
        {
            return await _context.Customers
                .Include(c => c.WalkInVisits)
                .Include(c => c.OwnedVehicles)
                .FirstOrDefaultAsync(c => c.Id == customerId);
        }

        

        public async Task<List<Customer>> GetAllCustomersAsync()
        {
            return await _context.Customers
                .Include(c => c.WalkInVisits)
                .OrderByDescending(c => c.CreatedAt)
                .ToListAsync();
        }

        // Yaya ziyaret işlemleri
        public async Task<WalkInVisit?> RegisterWalkInVisitAsync(WalkInVisitRequest request)
        {
            try
            {
                var walkInVisit = new WalkInVisit
                {
                    CustomerId = request.CustomerId,
                    VisitedDepartment = request.VisitedDepartment,
                    VisitedPersonnel = request.VisitedPersonnel,
                    VisitReason = request.VisitReason,
                    VisitTime = DateTime.UtcNow,
                    Status = "Aktif",
                    CreatedAt = DateTime.UtcNow,
                    NotificationSent = true, // Bildirim gönderildi olarak işaretle
                    NotificationSentAt = DateTime.UtcNow
                };

                await _context.WalkInVisits.AddAsync(walkInVisit);
                await _context.SaveChangesAsync();

                // Customer bilgilerini de yükle
                await _context.Entry(walkInVisit)
                    .Reference(w => w.Customer)
                    .LoadAsync();

                return walkInVisit;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"RegisterWalkInVisitAsync error: {ex.Message}");
                return null;
            }
        }

        public async Task<List<WalkInVisit>> GetActiveWalkInVisitsAsync()
        {
            return await _context.WalkInVisits
                .Include(w => w.Customer)
                .Where(w => w.Status == "Aktif")
                .OrderByDescending(w => w.VisitTime)
                .ToListAsync();
        }

        public async Task<List<WalkInVisit>> GetWalkInVisitsByDepartmentAsync(string department)
        {
            return await _context.WalkInVisits
                .Include(w => w.Customer)
                .Where(w => w.VisitedDepartment == department && w.Status == "Aktif")
                .OrderByDescending(w => w.VisitTime)
                .ToListAsync();
        }

        public async Task<bool> CompleteWalkInVisitAsync(int visitId)
        {
            try
            {
                var visit = await _context.WalkInVisits.FindAsync(visitId);
                if (visit == null || visit.Status != "Aktif")
                    return false;

                visit.Status = "Tamamlandı";
                visit.CompletedAt = DateTime.UtcNow;

                _context.WalkInVisits.Update(visit);
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"CompleteWalkInVisitAsync error: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> CancelWalkInVisitAsync(int visitId)
        {
            try
            {
                var visit = await _context.WalkInVisits.FindAsync(visitId);
                if (visit == null || visit.Status != "Aktif")
                    return false;

                visit.Status = "İptal";
                visit.CompletedAt = DateTime.UtcNow;

                _context.WalkInVisits.Update(visit);
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"CancelWalkInVisitAsync error: {ex.Message}");
                return false;
            }
        }

        // Araç durumu sorgulama
        public async Task<VehicleStatusResponse?> GetVehicleStatusAsync(string plateNumber)
        {
            try
            {
                var vehicle = await _context.Vehicles
                    .Include(v => v.Notes.OrderByDescending(n => n.Timestamp).Take(3))
                    .FirstOrDefaultAsync(v => v.PlateNumber.ToUpper() == plateNumber.ToUpper());

                if (vehicle == null)
                {
                    return null;
                }

                var response = new VehicleStatusResponse
                {
                    PlateNumber = vehicle.PlateNumber,
                    CustomerName = vehicle.CustomerName,
                    CurrentStatus = vehicle.CurrentStatus,
                    ServiceType = vehicle.ServiceType,
                    LastEntryTime = vehicle.LastEntryTime,
                    LastExitTime = vehicle.LastExitTime,
                    HasAppointment = vehicle.HasAppointment,
                    AppointmentDate = vehicle.AppointmentDate,
                    AppointmentDetails = vehicle.AppointmentDetails,
                    ClaimedByUserName = vehicle.ClaimedByUserName,
                    ClaimedAt = vehicle.ClaimedAt,
                    ExitApprovalRequired = vehicle.ExitApprovalRequired,
                    ExitApproved = vehicle.ExitApproved,
                    
                };

                return response;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"GetVehicleStatusAsync error: {ex.Message}");
                return null;
            }
        }
    }
}
