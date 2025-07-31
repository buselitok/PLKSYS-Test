using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using PLKSYS.Core.Interfaces;
using PLKSYS.Core.Models;

namespace PLKSYS.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class VehicleActivitiesController : ControllerBase
    {
        private readonly IVehicleActivityService _activityService;

        public VehicleActivitiesController(IVehicleActivityService activityService)
        {
            _activityService = activityService;
        }

        // GET: api/VehicleActivities/vehicle/{plateNumber}
        [HttpGet("vehicle/{plateNumber}")]
        public async Task<ActionResult<IEnumerable<VehicleActivityViewModel>>> GetVehicleActivities(string plateNumber)
        {
            var activities = await _activityService.GetVehicleActivitiesAsync(plateNumber);
            return Ok(activities);
        }

        // GET: api/VehicleActivities
        [HttpGet]
        public async Task<ActionResult<IEnumerable<VehicleActivityViewModel>>> GetAllActivities(
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 50)
        {
            var activities = await _activityService.GetAllActivitiesAsync(pageNumber, pageSize);
            return Ok(activities);
        }

        // GET: api/VehicleActivities/department/{department}
        [HttpGet("department/{department}")]
        public async Task<ActionResult<IEnumerable<VehicleActivityViewModel>>> GetActivitiesByDepartment(
            string department,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 50)
        {
            var activities = await _activityService.GetActivitiesByDepartmentAsync(department, pageNumber, pageSize);
            return Ok(activities);
        }

        // GET: api/VehicleActivities/type/{activityType}
        [HttpGet("type/{activityType}")]
        public async Task<ActionResult<IEnumerable<VehicleActivityViewModel>>> GetActivitiesByType(
            string activityType,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 50)
        {
            var activities = await _activityService.GetActivitiesByTypeAsync(activityType, pageNumber, pageSize);
            return Ok(activities);
        }

        // GET: api/VehicleActivities/recent
        [HttpGet("recent")]
        public async Task<ActionResult<IEnumerable<VehicleActivityViewModel>>> GetRecentActivities([FromQuery] int count = 20)
        {
            var activities = await _activityService.GetRecentActivitiesAsync(count);
            return Ok(activities);
        }

        // POST: api/VehicleActivities
        [HttpPost]
        public async Task<ActionResult<VehicleActivity>> CreateActivity(VehicleActivityCreateDto activityDto)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var userName = User.FindFirst(ClaimTypes.Name)?.Value;
            var userDepartment = User.FindFirst("Department")?.Value;

            if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(userName) || string.IsNullOrEmpty(userDepartment))
            {
                return Unauthorized("Kullanıcı bilgileri eksik.");
            }

            var activity = await _activityService.LogActivityAsync(activityDto, userId, userName, userDepartment);
            return CreatedAtAction(nameof(GetVehicleActivities), new { plateNumber = activity.PlateNumber }, activity);
        }

        // POST: api/VehicleActivities/entry/{plateNumber}
        [HttpPost("entry/{plateNumber}")]
        [Authorize(Roles = "Security,Admin,Superadmin")]
        public async Task<ActionResult> LogVehicleEntry(string plateNumber, [FromBody] string? description = null)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var userName = User.FindFirst(ClaimTypes.Name)?.Value;

            if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(userName))
            {
                return Unauthorized("Kullanıcı bilgileri eksik.");
            }

            await _activityService.LogVehicleEntryAsync(plateNumber, userId, userName, description ?? "Araç plaza girişi yapıldı");
            return Ok(new { message = "Araç giriş kaydı oluşturuldu." });
        }

        // POST: api/VehicleActivities/exit/{plateNumber}
        [HttpPost("exit/{plateNumber}")]
        [Authorize(Roles = "Security,Admin,Superadmin")]
        public async Task<ActionResult> LogVehicleExit(string plateNumber, [FromBody] string? description = null)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var userName = User.FindFirst(ClaimTypes.Name)?.Value;

            if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(userName))
            {
                return Unauthorized("Kullanıcı bilgileri eksik.");
            }

            await _activityService.LogVehicleExitAsync(plateNumber, userId, userName, description ?? "Araç plaza çıkışı yapıldı");
            return Ok(new { message = "Araç çıkış kaydı oluşturuldu." });
        }

        // POST: api/VehicleActivities/status-change
        [HttpPost("status-change")]
        public async Task<ActionResult> LogStatusChange([FromBody] StatusChangeDto statusChangeDto)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var userName = User.FindFirst(ClaimTypes.Name)?.Value;
            var userDepartment = User.FindFirst("Department")?.Value;

            if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(userName) || string.IsNullOrEmpty(userDepartment))
            {
                return Unauthorized("Kullanıcı bilgileri eksik.");
            }

            await _activityService.LogStatusChangeAsync(
                statusChangeDto.PlateNumber,
                statusChangeDto.PreviousStatus,
                statusChangeDto.NewStatus,
                userId,
                userName,
                userDepartment);

            return Ok(new { message = "Durum değişikliği kaydedildi." });
        }

        // GET: api/VehicleActivities/stats
        [HttpGet("stats")]
        [Authorize(Roles = "Admin,Superadmin")]
        public async Task<ActionResult> GetActivityStats([FromQuery] DateTime? startDate = null, [FromQuery] DateTime? endDate = null)
        {
            var activityStats = await _activityService.GetActivityStatsAsync(startDate, endDate);
            var departmentStats = await _activityService.GetDepartmentActivityStatsAsync(startDate, endDate);

            return Ok(new
            {
                ActivityStats = activityStats,
                DepartmentStats = departmentStats,
                Period = new { StartDate = startDate, EndDate = endDate }
            });
        }
    }
    public class StatusChangeDto
    {
        public string PlateNumber { get; set; } = string.Empty;
        public string PreviousStatus { get; set; } = string.Empty;
        public string NewStatus { get; set; } = string.Empty;
    }

}  