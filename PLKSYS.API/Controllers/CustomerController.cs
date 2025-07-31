using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PLKSYS.Core.Data;
using PLKSYS.Core.Interfaces;
using PLKSYS.Core.Models;

namespace PLKSYS.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CustomerController : ControllerBase
    {
        private readonly PLKSYSContext _context;
        private readonly ICustomerService _customerService;

        public CustomerController(PLKSYSContext context, ICustomerService customerService)
        {
            _context = context;
            _customerService = customerService;
        }

        // Müşteri kayıt işlemleri - Public (Authentication gerektirmez)
        [HttpPost("register")]
        [AllowAnonymous]
        public async Task<ActionResult<Customer>> RegisterCustomer(CustomerRegistrationRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var customer = await _customerService.RegisterCustomerAsync(request);
            if (customer == null)
            {
                return BadRequest("Müşteri kaydı oluşturulamadı.");
            }

            return Ok(customer);
        }

        // Yaya ziyaret kaydı - Public (Authentication gerektirmez)
        [HttpPost("walk-in-visit")]
        [AllowAnonymous]
        public async Task<ActionResult<WalkInVisit>> RegisterWalkInVisit(WalkInVisitRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var visit = await _customerService.RegisterWalkInVisitAsync(request);
            if (visit == null)
            {
                return BadRequest("Yaya ziyaret kaydı oluşturulamadı.");
            }

            return Ok(visit);
        }

        // Araç durumu sorgulama - Public (Authentication gerektirmez)
        [HttpGet("vehicle-status/{plateNumber}")]
        [AllowAnonymous]
        public async Task<ActionResult<VehicleStatusResponse>> GetVehicleStatus(string plateNumber)
        {
            if (string.IsNullOrWhiteSpace(plateNumber))
            {
                return BadRequest("Plaka numarası boş olamaz.");
            }

            var status = await _customerService.GetVehicleStatusAsync(plateNumber);
            if (status == null)
            {
                return NotFound($"'{plateNumber}' plakalı araç bulunamadı.");
            }

            return Ok(status);
        }

        // Admin/Personel için aktif yaya ziyaretlerini getir
        [HttpGet("active-walk-in-visits")]
        [Authorize]
        public async Task<ActionResult<List<WalkInVisit>>> GetActiveWalkInVisits()
        {
            var visits = await _customerService.GetActiveWalkInVisitsAsync();
            return Ok(visits);
        }

        // Departmana göre yaya ziyaretlerini getir
        [HttpGet("walk-in-visits/department/{department}")]
        [Authorize]
        public async Task<ActionResult<List<WalkInVisit>>> GetWalkInVisitsByDepartment(string department)
        {
            var visits = await _customerService.GetWalkInVisitsByDepartmentAsync(department);
            return Ok(visits);
        }

        // Yaya ziyareti tamamla
        [HttpPost("complete-walk-in-visit/{visitId}")]
        [Authorize]
        public async Task<IActionResult> CompleteWalkInVisit(int visitId)
        {
            var result = await _customerService.CompleteWalkInVisitAsync(visitId);
            if (!result)
            {
                return NotFound("Ziyaret bulunamadı veya zaten tamamlanmış.");
            }

            return Ok(new { message = "Ziyaret başarıyla tamamlandı." });
        }

        // Yaya ziyareti iptal et
        [HttpPost("cancel-walk-in-visit/{visitId}")]
        [Authorize]
        public async Task<IActionResult> CancelWalkInVisit(int visitId)
        {
            var result = await _customerService.CancelWalkInVisitAsync(visitId);
            if (!result)
            {
                return NotFound("Ziyaret bulunamadı veya zaten tamamlanmış.");
            }

            return Ok(new { message = "Ziyaret başarıyla iptal edildi." });
        }

        // Admin için tüm müşterileri getir
        [HttpGet("all")]
        [Authorize(Policy = "AdminOrSuperadmin")]
        public async Task<ActionResult<List<Customer>>> GetAllCustomers()
        {
            var customers = await _customerService.GetAllCustomersAsync();
            return Ok(customers);
        }

        // Müşteri detayı getir
        [HttpGet("{customerId}")]
        [Authorize]
        public async Task<ActionResult<Customer>> GetCustomer(int customerId)
        {
            var customer = await _customerService.GetCustomerByIdAsync(customerId);
            if (customer == null)
            {
                return NotFound("Müşteri bulunamadı.");
            }

            return Ok(customer);
        }
    }
}
