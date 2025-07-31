using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PLKSYS.Core.Data;
using PLKSYS.Core.Interfaces;
using PLKSYS.Core.Models;
using PLKSYS.Core.Services;
using System.Security.Claims;
using static PLKSYS.Core.Models.ClaimRequest;
using static PLKSYS.Core.Models.Vehicle;

namespace PLKSYS.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    // Departman bazlı yetkilendirme: Bu controller'a sadece belirli departmanlar erişebilir
    [Authorize] // Tüm departmanlar için politika kullanıldı
    public class VehiclesController : ControllerBase
    {
        private readonly PLKSYSContext _context;
        private readonly IVehicleService _vehicleService;


        public VehiclesController(PLKSYSContext context, IVehicleService vehicleService)
        {
            _context = context;
            _vehicleService = vehicleService;
        }

        // GET: api/Vehicles/live
        [HttpGet("live")]
        public async Task<ActionResult<IEnumerable<Vehicle>>> GetLiveVehicles()
        {
            return await _context.Vehicles
                                 .Where(v => v.CurrentStatus == "in")
                                 .Include(v => v.Notes)
                                 .ToListAsync();
        }

        // GET: api/Vehicles/logs
        [HttpGet("logs")]
        public async Task<ActionResult<IEnumerable<Vehicle>>> GetVehicleLogs()
        {
            return await _context.Vehicles
                                 .Include(v => v.Notes)
                                 .ToListAsync();
        }

        // GET: api/Vehicles/{plateNumber}
        [HttpGet("{plateNumber}")]
        public async Task<ActionResult<Vehicle>> GetVehicle(string plateNumber)
        {
            var vehicle = await _context.Vehicles
                                        .Include(v => v.Notes)
                                        .FirstOrDefaultAsync(v => v.PlateNumber == plateNumber);

            if (vehicle == null)
            {
                return NotFound();
            }

            return vehicle;
        }

        /*// PUT: api/Vehicles/{plateNumber}
        [HttpPut("{plateNumber}")]
        [Authorize(Policy = "AdminOrSecurityOrSuperadmin")] // Politika kullanıldı
        public async Task<IActionResult> PutVehicle(string plateNumber, Vehicle vehicle)
        {
            if (plateNumber != vehicle.PlateNumber)
            {
                return BadRequest();
            }

            _context.Entry(vehicle).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!VehicleExists(plateNumber))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        } */

        // POST: api/Vehicles/entry
        /*[HttpPost("entry")]
        [Authorize(Policy = "AdminOrSecurityOrSuperadmin")] // Politika kullanıldı
        public async Task<ActionResult<Vehicle>> PostVehicleEntry(PlateEntryRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var plateNumber = request.PlateNumber.ToUpperInvariant(); // Gelen plaka numarasını büyük harfe dönüştür
            var vehicle = await _context.Vehicles.FirstOrDefaultAsync(v => v.PlateNumber == plateNumber);
            var now = DateTime.UtcNow;

            // Rezervasyon kontrolü: Plaka numarası eşleşen, durumu "Confirmed" olan ve
            // bugüne veya geleceğe ait bir rezervasyon var mı?
            var reservation = await _context.Reservations
                                            .FirstOrDefaultAsync(r => r.PlateNumber == plateNumber &&
                                                                     r.Status == "Confirmed" &&
                                                                     r.ReservationDateTime.Date >= now.Date);

            if (vehicle == null)
            {
                // Yeni araç girişi
                vehicle = new Vehicle
                {
                    PlateNumber = plateNumber,
                    LastEntryTime = now,
                    CurrentStatus = "in",
                    CreatedAt = now,
                    UpdatedAt = now
                };

                if (reservation != null)
                {
                    // Rezervasyon varsa, bilgileri rezervasyondan al
                    vehicle.CustomerName = reservation.CustomerName;
                    vehicle.VehicleModel = "Belirtilmemiş Model"; // Varsayılan değer, gerekirse güncellenebilir
                    vehicle.HasAppointment = true;
                    vehicle.AppointmentDate = reservation.ReservationDateTime;
                    vehicle.AppointmentDetails = reservation.Notes;
                    vehicle.ServiceType = reservation.ServiceType;
                }
                else
                {
                    // Rezervasyon yoksa varsayılan değerler
                    vehicle.CustomerName = "Bilinmeyen Müşteri";
                    vehicle.VehicleModel = "Bilinmeyen Model";
                    vehicle.HasAppointment = false;
                    vehicle.AppointmentDate = null;
                    vehicle.AppointmentDetails = null;
                    vehicle.ServiceType = "Randevusuz Giriş";
                    vehicle.InsuranceStatus = "Yok";
                    vehicle.PotentialSalesReferral = false;
                    vehicle.PotentialInsuranceReferral = false;
                }
                _context.Vehicles.Add(vehicle);
            }
            else
            {
                // Mevcut aracın yeniden girişi
                vehicle.LastEntryTime = now;
                vehicle.LastExitTime = null;
                vehicle.CurrentStatus = "in";
                vehicle.UpdatedAt = now;

                if (reservation != null)
                {
                    // Rezervasyon varsa, bilgileri rezervasyondan güncelle
                    vehicle.CustomerName = reservation.CustomerName;
                    vehicle.VehicleModel = "Belirtilmemiş Model"; // Varsayılan değer, gerekirse güncellenebilir
                    vehicle.HasAppointment = true;
                    vehicle.AppointmentDate = reservation.ReservationDateTime;
                    vehicle.AppointmentDetails = reservation.Notes;
                    vehicle.ServiceType = reservation.ServiceType;
                }
                else
                {
                    // Rezervasyon yoksa varsayılan durumları ayarla
                    vehicle.HasAppointment = false;
                    vehicle.AppointmentDate = null;
                    vehicle.AppointmentDetails = null;
                    vehicle.ServiceType = "Randevusuz Giriş";
                    // Müşteri adı ve model, mevcut araçtan korunur, sadece randevu durumu değişir.
                    // vehicle.CustomerName = vehicle.CustomerName; // Zaten mevcut değeri korur
                    // vehicle.VehicleModel = vehicle.VehicleModel; // Zaten mevcut değeri korur
                }
                _context.Vehicles.Update(vehicle);
            }

            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetVehicle), new { plateNumber = vehicle.PlateNumber }, vehicle);
        }

        // POST: api/Vehicles/exit
        [HttpPost("exit")]
        [Authorize(Policy = "AdminOrSecurityOrSuperadmin")] // Politika kullanıldı
        public async Task<IActionResult> PostVehicleExit(PlateEntryRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var plateNumber = request.PlateNumber.ToUpperInvariant();
            var vehicle = await _context.Vehicles.FirstOrDefaultAsync(v => v.PlateNumber == plateNumber && v.CurrentStatus == "in");

            if (vehicle == null)
            {
                return NotFound($"Plaka '{plateNumber}' plazada bulunamadı veya zaten çıkış yapmış.");
            }

            vehicle.LastExitTime = DateTime.UtcNow;
            vehicle.CurrentStatus = "out";
            vehicle.UpdatedAt = DateTime.UtcNow;

            _context.Vehicles.Update(vehicle);
            await _context.SaveChangesAsync();

            return Ok(vehicle);
        }*/


        // YENİ: Onay bekleyen araçları getir (Muhasebe için)
        [HttpGet("pending-exit-approval")]
        [Authorize(Policy = "AdminOrAccountingOrSuperadmin")] // Muhasebe politikası eklenecek
        public async Task<ActionResult<IEnumerable<Vehicle>>> GetVehiclesPendingExitApproval()
        {
            return await _context.Vehicles
                                 .Where(v => v.CurrentStatus == "in" &&
                                           v.ExitApprovalRequired == true &&
                                           v.ExitApproved == false)
                                 .Include(v => v.Notes)
                                 .ToListAsync();
        }

        // YENİ: Randevulu araç girişi
        [HttpPost("entry/appointment")]
        [Authorize(Policy = "AdminOrSecurityOrSuperadmin")]
        public async Task<ActionResult<Vehicle>> PostAppointmentEntry(AppointmentEntryRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var plateNumber = request.PlateNumber.ToUpperInvariant();
            var vehicle = await _context.Vehicles.FirstOrDefaultAsync(v => v.PlateNumber == plateNumber);
            var now = DateTime.UtcNow;

            // Rezervasyon kontrolü
            var reservation = await _context.Reservations
                                            .FirstOrDefaultAsync(r => r.PlateNumber == plateNumber &&
                                                                     r.Status == "Onaylandı" &&
                                                                     r.ReservationDateTime.Date >= now.Date);

            if (vehicle == null)
            {
                vehicle = new Vehicle
                {
                    PlateNumber = plateNumber,
                    LastEntryTime = now,
                    CurrentStatus = "in",
                    CreatedAt = now,
                    UpdatedAt = now,
                    EntryType = "appointment",
                    ExitApprovalRequired = false, // Randevulu girişlerde onay gerekmez
                    ExitApproved = true
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
                _context.Vehicles.Add(vehicle);
            }
            else
            {
                vehicle.LastEntryTime = now;
                vehicle.LastExitTime = null;
                vehicle.CurrentStatus = "in";
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

                _context.Vehicles.Update(vehicle);
            }

            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetVehicle), new { plateNumber = vehicle.PlateNumber }, vehicle);
        }

        // YENİ: Randevusuz araç girişi
        [HttpPost("entry/walk-in")]
        [Authorize(Policy = "AdminOrSecurityOrSuperadmin")]
        public async Task<ActionResult<Vehicle>> PostWalkInEntry(WalkInEntryRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var plateNumber = request.PlateNumber.ToUpperInvariant();
            var vehicle = await _context.Vehicles.FirstOrDefaultAsync(v => v.PlateNumber == plateNumber);
            var now = DateTime.UtcNow;

            if (vehicle == null)
            {
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
                    ExitApprovalRequired = true, // Randevusuz girişlerde onay gerekir
                    ExitApproved = false
                };
                _context.Vehicles.Add(vehicle);
            }
            else
            {
                vehicle.LastEntryTime = now;
                vehicle.LastExitTime = null;
                vehicle.CurrentStatus = "in";
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

            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetVehicle), new { plateNumber = vehicle.PlateNumber }, vehicle);
        }

        // YENİ: Çıkış onayı verme (Muhasebe için)
        [HttpPost("approve-exit")]
        [Authorize(Policy = "AdminOrAccountingOrSuperadmin")] // Muhasebe politikası
        public async Task<IActionResult> ApproveExit(ExitApprovalRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var plateNumber = request.PlateNumber.ToUpperInvariant();
            var vehicle = await _context.Vehicles.FirstOrDefaultAsync(v => v.PlateNumber == plateNumber &&
                                                                          v.CurrentStatus == "in" &&
                                                                          v.ExitApprovalRequired == true);

            if (vehicle == null)
            {
                return NotFound($"Plaka '{plateNumber}' için onay bekleyen araç bulunamadı.");
            }

            // Kullanıcı bilgilerini al
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var userName = User.FindFirst(ClaimTypes.Name)?.Value ?? "Bilinmeyen Kullanıcı";

            vehicle.ExitApproved = true;
            vehicle.ExitApprovedByUserId = userId != null ? int.Parse(userId) : null;
            vehicle.ExitApprovedByUserName = userName;
            vehicle.ExitApprovedAt = DateTime.UtcNow;
            vehicle.UpdatedAt = DateTime.UtcNow;

            _context.Vehicles.Update(vehicle);
            await _context.SaveChangesAsync();

            return Ok(new { message = $"Plaka '{plateNumber}' için çıkış onayı verildi.", vehicle });
        }

        // GÜNCELLENMIŞ: Araç çıkışı (onay kontrolü ile)
        [HttpPost("exit")]
        [Authorize(Policy = "AdminOrSecurityOrSuperadmin")]
        public async Task<IActionResult> PostVehicleExit(PlateEntryRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var plateNumber = request.PlateNumber.ToUpperInvariant();
            var vehicle = await _context.Vehicles.FirstOrDefaultAsync(v => v.PlateNumber == plateNumber &&
                                                                          v.CurrentStatus == "in");

            if (vehicle == null)
            {
                return NotFound($"Plaka '{plateNumber}' plazada bulunamadı veya zaten çıkış yapmış.");
            }

            // Onay kontrolü
            if (vehicle.ExitApprovalRequired && !vehicle.ExitApproved)
            {
                return BadRequest($"Plaka '{plateNumber}' için çıkış yapabilmek için muhasebe onayı gereklidir. Lütfen önce onay alın.");
            }

            vehicle.LastExitTime = DateTime.UtcNow;
            vehicle.CurrentStatus = "out";
            vehicle.UpdatedAt = DateTime.UtcNow;

            _context.Vehicles.Update(vehicle);
            await _context.SaveChangesAsync();

            return Ok(vehicle);
        }

        // ESKİ METHODLAR (Geriye uyumluluk için korundu, deprecated olarak işaretlenebilir)
        [HttpPost("entry")]
        [Authorize(Policy = "AdminOrSecurityOrSuperadmin")]
        [Obsolete("Bu method deprecated. Yeni sistemde 'entry/appointment' veya 'entry/walk-in' kullanın.")]
        public async Task<ActionResult<Vehicle>> PostVehicleEntry(PlateEntryRequest request)
        {
            // Eski method, randevulu giriş olarak yönlendir
            var appointmentRequest = new AppointmentEntryRequest { PlateNumber = request.PlateNumber };
            return await PostAppointmentEntry(appointmentRequest);
        }



    }
}