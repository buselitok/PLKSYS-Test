// PLKSYS.API/Controllers/WashingController.cs
// Yıkama birimi işlemleri için API kontrolcüsü.
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PLKSYS.Core.Interfaces;
using PLKSYS.Core.Models;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;

namespace PLKSYS.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize] // Temel yetkilendirme
    public class WashingController : ControllerBase
    {
        private readonly IWashingService _washingService;
        private readonly IUserService _userService; // Kullanıcı adını almak için

        public WashingController(IWashingService washingService, IUserService userService)
        {
            _washingService = washingService;
            _userService = userService;
        }

        // POST: api/Washing/send-to-washing
        // Aracı yıkama kuyruğuna ekler. Servis personeli veya yöneticiler tarafından çağrılır.
        [HttpPost("send-to-washing")]
        [Authorize(Policy = "ServisOrAdminOrSuperadmin")] // Servis, Admin veya Superadmin yetkisi
        public async Task<ActionResult<WashingQueueEntry>> SendToWashing([FromBody] WashingQueueEntryCreateDto createDto)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(userIdClaim, out int userId))
            {
                return Unauthorized("Kullanıcı kimliği alınamadı.");
            }

            var user = await _userService.GetUserByIdAsync(userId);
            if (user == null)
            {
                return Unauthorized("Kullanıcı bulunamadı.");
            }

            var entry = await _washingService.AddToWashingQueueAsync(createDto, user.Username); // Kullanıcı adını gönder
             
            if (entry == null)
            {
                return BadRequest("Araç zaten yıkama kuyruğunda veya bir hata oluştu.");
            }

            return CreatedAtAction(nameof(GetWashingEntry), new { id = entry.Id }, entry);
        }

        // PUT: api/Washing/{id}/complete
        // Yıkama işlemini tamamlandı olarak işaretler. Yıkama birimi personeli veya yöneticiler tarafından çağrılır.
        [HttpPut("{id}/complete")]
        [Authorize(Policy = "WashingOrAdminOrSuperadmin")] // Yıkama birimi, Admin veya Superadmin yetkisi
        public async Task<IActionResult> MarkWashingCompleted(Guid id)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(userIdClaim, out int userId))
            {
                return Unauthorized("Kullanıcı kimliği alınamadı.");
            }

            var user = await _userService.GetUserByIdAsync(userId);
            if (user == null)
            {
                return Unauthorized("Kullanıcı bulunamadı.");
            }

            var updatedEntry = await _washingService.MarkWashingCompletedAsync(id, user.Username); // Kullanıcı adını gönder
            if (updatedEntry == null)
            {
                return NotFound("Yıkama girişi bulunamadı veya zaten tamamlanmış.");
            }
            return NoContent();
        }

        // GET: api/Washing/pending
        // Bekleyen yıkama girişlerini getirir. Yıkama birimi personeli veya yöneticiler tarafından çağrılır.
        [HttpGet("pending")]
        [Authorize(Policy = "WashingOrAdminOrSuperadmin")] // Yıkama birimi, Admin veya Superadmin yetkisi
        public async Task<ActionResult<IEnumerable<WashingQueueEntry>>> GetPendingWashingEntries()
        {
            var entries = await _washingService.GetPendingWashingEntriesAsync();
            return Ok(entries);
        }

        // GET: api/Washing/all
        // Tüm yıkama girişlerini (bekleyen ve tamamlanmış) getirir. Yöneticiler tarafından çağrılır.
        [HttpGet("all")]
        [Authorize(Policy = "AdminOrSuperadmin")] // Sadece Admin veya Superadmin yetkisi
        public async Task<ActionResult<IEnumerable<WashingQueueEntry>>> GetAllWashingEntries()
        {
            var entries = await _washingService.GetAllWashingEntriesAsync();
            return Ok(entries);
        }

        // GET: api/Washing/{id}
        // Belirli bir yıkama girişini getirir.
        [HttpGet("{id}")]
        public async Task<ActionResult<WashingQueueEntry>> GetWashingEntry(Guid id)
        {
            var entry = await _washingService.GetWashingEntryByIdAsync(id);
            if (entry == null)
            {
                return NotFound();
            }
            return Ok(entry);
        }
    }

}
