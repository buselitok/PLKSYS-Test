using PLKSYS.Core.Data;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using PLKSYS.Core.Models;
using PLKSYS.Core.Interfaces;
using static PLKSYS.Core.Models.Vehicle; // Where ve OrderByDescending için

namespace PLKSYS.Core.Services
{
    public class VehicleService : IVehicleService
    {
        private readonly PLKSYSContext _context;

        public VehicleService(PLKSYSContext context)
        {
            _context = context;
        }

        public async Task<List<Vehicle>> GetLiveVehicles()
        {
            return await _context.Vehicles
                                 .Where(v => v.CurrentStatus == "in")
                                 .Include(v => v.Notes)
                                 .ToListAsync();
        }

        public async Task<List<Vehicle>> GetVehicleLogs()
        {
            return await _context.Vehicles
                                 .OrderByDescending(v => v.LastEntryTime)
                                 .Include(v => v.Notes)
                                 .ToListAsync();
        }

        public async Task<Vehicle?> GetVehicleByPlate(string plateNumber)
        {
            return await _context.Vehicles
                                 .Include(v => v.Notes)
                                 .SingleOrDefaultAsync(v => v.PlateNumber == plateNumber);
        }

        // YENİ: Onay bekleyen araçları getir
        public async Task<List<Vehicle>> GetVehiclesPendingExitApproval()
        {
            return await _context.Vehicles
                                 .Where(v => v.CurrentStatus == "in" &&
                                           v.ExitApprovalRequired == true &&
                                           v.ExitApproved == false)
                                 .Include(v => v.Notes)
                                 .OrderBy(v => v.LastEntryTime) // En eski bekleyenler önce
                                 .ToListAsync();
        }

        // YENİ: Randevulu araç girişi
        public async Task<Vehicle?> RegisterAppointmentEntry(AppointmentEntryRequest request)
        {
            var plateNumber = request.PlateNumber.ToUpperInvariant();
            var vehicle = await _context.Vehicles.FindAsync(plateNumber);
            var now = DateTime.UtcNow;

            // Rezervasyon kontrolü
            var reservation = await _context.Reservations
                                            .FirstOrDefaultAsync(r => r.PlateNumber == plateNumber &&
                                                                     r.Status == "Onaylandı" &&
                                                                     r.ReservationDateTime.Date >= now.Date);

            if (vehicle != null)
            {
                // Mevcut araç güncelleme
                vehicle.LastEntryTime = now;
                vehicle.CurrentStatus = "in";
                vehicle.LastExitTime = null;
                vehicle.UpdatedAt = now;
                vehicle.EntryType = "appointment";
                vehicle.ExitApprovalRequired = false;
                vehicle.ExitApproved = true;

                if (reservation != null)
                {
                    vehicle.CustomerName = reservation.CustomerName;
                    vehicle.HasAppointment = true;
                    vehicle.AppointmentDate = reservation.ReservationDateTime;
                    vehicle.AppointmentDetails = reservation.Notes;
                    vehicle.ServiceType = reservation.ServiceType;
                }
                else
                {
                    vehicle.HasAppointment = false;
                    vehicle.ServiceType = "Randevulu Giriş";
                }

                _context.Vehicles.Update(vehicle);
            }
            else
            {
                // Yeni araç ekleme
                vehicle = new Vehicle
                {
                    PlateNumber = plateNumber,
                    LastEntryTime = now,
                    CurrentStatus = "in",
                    CreatedAt = now,
                    UpdatedAt = now,
                    EntryType = "appointment",
                    ExitApprovalRequired = false,
                    ExitApproved = true,
                    ToyotaAssistantPackage = false,
                    InsuranceStatus = "Yok",
                    PotentialSalesReferral = false,
                    PotentialInsuranceReferral = false
                };

                if (reservation != null)
                {
                    vehicle.CustomerName = reservation.CustomerName;
                    vehicle.HasAppointment = true;
                    vehicle.AppointmentDate = reservation.ReservationDateTime;
                    vehicle.AppointmentDetails = reservation.Notes;
                    vehicle.ServiceType = reservation.ServiceType;
                }
                else
                {
                    vehicle.CustomerName = "Bilinmeyen Müşteri";
                    vehicle.HasAppointment = false;
                    vehicle.ServiceType = "Randevulu Giriş";
                }

                await _context.Vehicles.AddAsync(vehicle);
            }

            await _context.SaveChangesAsync();
            return vehicle;
        }

        // YENİ: Randevusuz araç girişi
        public async Task<Vehicle?> RegisterWalkInEntry(WalkInEntryRequest request)
        {
            var plateNumber = request.PlateNumber.ToUpperInvariant();
            var vehicle = await _context.Vehicles.FindAsync(plateNumber);
            var now = DateTime.UtcNow;

            if (vehicle != null)
            {
                // Mevcut araç güncelleme
                vehicle.LastEntryTime = now;
                vehicle.CurrentStatus = "in";
                vehicle.LastExitTime = null;
                vehicle.UpdatedAt = now;
                vehicle.EntryType = "walk-in";
                vehicle.VisitorName = request.VisitorName;
                vehicle.VisitorSurname = request.VisitorSurname;
                vehicle.VisitedDepartment = request.VisitedDepartment;
                vehicle.VisitedPersonnel = request.VisitedPersonnel;
                vehicle.VisitReason = request.VisitReason;
                vehicle.CustomerName = $"{request.VisitorName} {request.VisitorSurname}";
                vehicle.HasAppointment = false;
                vehicle.ServiceType = "Randevusuz Ziyaret";
                vehicle.ExitApprovalRequired = true;
                vehicle.ExitApproved = false;

                _context.Vehicles.Update(vehicle);
            }
            else
            {
                // Yeni araç ekleme
                vehicle = new Vehicle
                {
                    PlateNumber = plateNumber,
                    LastEntryTime = now,
                    CurrentStatus = "in",
                    CreatedAt = now,
                    UpdatedAt = now,
                    EntryType = "walk-in",
                    VisitorName = request.VisitorName,
                    VisitorSurname = request.VisitorSurname,
                    VisitedDepartment = request.VisitedDepartment,
                    VisitedPersonnel = request.VisitedPersonnel,
                    VisitReason = request.VisitReason,
                    CustomerName = $"{request.VisitorName} {request.VisitorSurname}",
                    HasAppointment = false,
                    ServiceType = "Randevusuz Ziyaret",
                    ExitApprovalRequired = true,
                    ExitApproved = false,
                    ToyotaAssistantPackage = false,
                    InsuranceStatus = "Yok",
                    PotentialSalesReferral = false,
                    PotentialInsuranceReferral = false
                };

                await _context.Vehicles.AddAsync(vehicle);
            }

            await _context.SaveChangesAsync();
            return vehicle;
        }

        // YENİ: Çıkış onayı verme
        public async Task<bool> ApproveVehicleExit(string plateNumber, int approverUserId, string approverUserName, string? approvalNotes = null)
        {
            try
            {
                var vehicle = await _context.Vehicles.FirstOrDefaultAsync(v => v.PlateNumber == plateNumber &&
                                                                              v.CurrentStatus == "in" &&
                                                                              v.ExitApprovalRequired == true);

                if (vehicle == null)
                    return false;

                vehicle.ExitApproved = true;
                vehicle.ExitApprovedByUserId = approverUserId;
                vehicle.ExitApprovedByUserName = approverUserName;
                vehicle.ExitApprovedAt = DateTime.UtcNow;
                vehicle.UpdatedAt = DateTime.UtcNow;

                // Onay notları varsa, araç notlarına ekle (Note model alanları kontrol edilecek)
                if (!string.IsNullOrWhiteSpace(approvalNotes))
                {
                    // TODO: Note model alanları kontrol edilerek düzenlenecek
                    /*
                    var note = new Note
                    {
                        VehiclePlateNumber = plateNumber,
                        Content = $"Çıkış Onayı: {approvalNotes}",
                        CreatedByUserId = approverUserId,
                        CreatedByUserName = approverUserName,
                        CreatedAt = DateTime.UtcNow
                    };
                    _context.Notes.Add(note);
                    */
                }

                _context.Vehicles.Update(vehicle);
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ApproveVehicleExit error: {ex.Message}");
                return false;
            }
        }

        // YENİ: Çıkış onayı iptal etme (gerekirse)
        public async Task<bool> RevokeExitApproval(string plateNumber, int revokerUserId, string revokerUserName, string reason)
        {
            try
            {
                var vehicle = await _context.Vehicles.FirstOrDefaultAsync(v => v.PlateNumber == plateNumber &&
                                                                              v.CurrentStatus == "in" &&
                                                                              v.ExitApproved == true);

                if (vehicle == null)
                    return false;

                vehicle.ExitApproved = false;
                vehicle.ExitApprovedByUserId = null;
                vehicle.ExitApprovedByUserName = null;
                vehicle.ExitApprovedAt = null;
                vehicle.UpdatedAt = DateTime.UtcNow;

                // İptal notunu ekle (Note model alanları kontrol edilecek)
                /*
                var note = new Note
                {
                    VehiclePlateNumber = plateNumber,
                    Content = $"Çıkış Onayı İptal Edildi - Sebep: {reason}",
                    CreatedByUserId = revokerUserId,
                    CreatedByUserName = revokerUserName,
                    CreatedAt = DateTime.UtcNow
                };
                _context.Notes.Add(note);
                */

                _context.Vehicles.Update(vehicle);
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"RevokeExitApproval error: {ex.Message}");
                return false;
            }
        }

        // GÜNCELLENMIŞ: Araç çıkışı (onay kontrolü ile)
        public async Task<Vehicle?> RegisterVehicleExit(PlateEntryRequest request)
        {
            var plateNumber = request.PlateNumber.ToUpperInvariant();
            var vehicle = await _context.Vehicles.FindAsync(plateNumber);

            if (vehicle == null)
            {
                throw new ArgumentException("Plazadan çıkış yapmak istenen araç bulunamadı.");
            }

            if (vehicle.CurrentStatus == "out")
            {
                throw new InvalidOperationException("Araç zaten plazadan çıkış yapmış durumda.");
            }

            // Onay kontrolü
            if (vehicle.ExitApprovalRequired && !vehicle.ExitApproved)
            {
                throw new InvalidOperationException("Bu araç için çıkış yapabilmek için muhasebe onayı gereklidir.");
            }

            vehicle.LastExitTime = DateTime.UtcNow;
            vehicle.CurrentStatus = "out";
            vehicle.UpdatedAt = DateTime.UtcNow;

            _context.Vehicles.Update(vehicle);
            await _context.SaveChangesAsync();
            return vehicle;
        }

        // MEVCUT METODLAR (Geriye uyumluluk için korundu)
        public async Task<Vehicle?> RegisterVehicleEntry(PlateEntryRequest request)
        {
            // Eski method, randevulu giriş olarak yönlendir
            var appointmentRequest = new AppointmentEntryRequest { PlateNumber = request.PlateNumber };
            return await RegisterAppointmentEntry(appointmentRequest);
        }

        public async Task<bool> UpdateVehicleClaimStatus(Vehicle vehicle)
        {
            try
            {
                _context.Vehicles.Update(vehicle);
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"UpdateVehicleClaimStatus error: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> ClaimVehicle(string plateNumber, int userId, string userName)
        {
            try
            {
                var vehicle = await GetVehicleByPlate(plateNumber);
                if (vehicle == null || vehicle.CurrentStatus != "in" || vehicle.ClaimedByUserId.HasValue)
                    return false;

                vehicle.ClaimedByUserId = userId;
                vehicle.ClaimedByUserName = userName;
                vehicle.ClaimedAt = DateTime.UtcNow;
                vehicle.UpdatedAt = DateTime.UtcNow;

                _context.Vehicles.Update(vehicle);
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ClaimVehicle error: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> UnclaimVehicle(string plateNumber, int userId)
        {
            try
            {
                var vehicle = await GetVehicleByPlate(plateNumber);
                if (vehicle == null || vehicle.ClaimedByUserId != userId)
                    return false;

                vehicle.ClaimedByUserId = null;
                vehicle.ClaimedByUserName = null;
                vehicle.ClaimedAt = null;
                vehicle.UpdatedAt = DateTime.UtcNow;

                _context.Vehicles.Update(vehicle);
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"UnclaimVehicle error: {ex.Message}");
                return false;
            }
        }
    }
}