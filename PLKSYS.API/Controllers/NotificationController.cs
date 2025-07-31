using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PLKSYS.Core.Data;
using PLKSYS.Core.Interfaces;

namespace PLKSYS.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize] // Sadece login kontrolü, rol kontrolü yok
    public class NotificationController : ControllerBase
    {
        private readonly IVehicleService _vehicleService;
        private readonly PLKSYSContext _context; // veya nasıl inject ediyorsanız

        public NotificationController(IVehicleService vehicleService, PLKSYSContext context)
        {
            _vehicleService = vehicleService;
            _context = context;
        }


        [HttpPost("claim")]
        public async Task<IActionResult> ClaimVehicle([FromBody] ClaimVehicleRequest request)
        {
            try
            {
                var vehicle = await _vehicleService.GetVehicleByPlate(request.PlateNumber);
                if (vehicle == null || vehicle.CurrentStatus != "in")
                {
                    return NotFound(new { success = false, message = "Araç bulunamadı veya plazada değil." });
                }
                if (vehicle.ClaimedByUserId.HasValue)
                {
                    return BadRequest(new { success = false, message = "Araç başka biri tarafından zaten üstlenilmiş." });
                }

                // DÜZELTME: Direkt database'i güncelle
                vehicle.ClaimedByUserId = request.UserId;
                vehicle.ClaimedByUserName = request.UserName;
                vehicle.ClaimedAt = DateTime.UtcNow;
                vehicle.UpdatedAt = DateTime.UtcNow;

                _context.Vehicles.Update(vehicle);
                await _context.SaveChangesAsync();

                return Ok(new { success = true, message = "Araç başarıyla üzerinize alındı." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = $"Hata: {ex.Message}" });
            }
        }

        [HttpPost("unclaim")]
        public async Task<IActionResult> UnclaimVehicle([FromBody] UnclaimVehicleRequest request)
        {
            try
            {
                var vehicle = await _vehicleService.GetVehicleByPlate(request.PlateNumber);

                if (vehicle == null || vehicle.ClaimedByUserId != request.UserId)
                {
                    return NotFound(new { success = false, message = "Araç bulunamadı veya size ait değil." });
                }

                // Claim bilgilerini temizle
                vehicle.ClaimedByUserId = null;
                vehicle.ClaimedByUserName = null;
                vehicle.ClaimedAt = null;
                vehicle.UpdatedAt = DateTime.UtcNow;

                _context.Vehicles.Update(vehicle);
                await _context.SaveChangesAsync();

                return Ok(new { success = true, message = "Araç başarıyla bırakıldı." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = $"Hata: {ex.Message}" });
            }
        }
    }

    public class ClaimVehicleRequest
    {
        public string PlateNumber { get; set; } = string.Empty;
        public int UserId { get; set; }
        public string UserName { get; set; } = string.Empty;
    }

    public class UnclaimVehicleRequest
    {
        public string PlateNumber { get; set; } = string.Empty;
        public int UserId { get; set; }
    }
}
