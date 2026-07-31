using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using School_CRM.Models;
using System.Diagnostics;
using System.Data;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace School_CRM.Controllers
{
    [AllowAnonymous]  // Home controller bina login ke accessible hai
    public class HomeController : Controller
    {
        private readonly LibmanagementContext _context;

        public HomeController(LibmanagementContext context)
        {
            _context = context;
        }
        public IActionResult Index()
        {
            return View();
        }
        public IActionResult IndexTheme()
        {
            return View();
        }
        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }


        public IActionResult indexTwo()
        {
            return View();
        }
        public IActionResult about()
        {
            return View();
        }
        public IActionResult events()
        {
            return View();
        }
        public IActionResult eventsingle()
        {
            return View();
        }
        public IActionResult schedule()
        {
            return View();
        }
        public IActionResult errorData()
        {
            return View();
        }
        public IActionResult classes()
        {
            return View();
        }
        public IActionResult classesSingle()
        {
            return View();
        }
        public IActionResult teachers()
        {
            return View();
        }
        public IActionResult teachersingle()
        {
            return View();
        }
        public IActionResult blog()
        {
            return View();
        }
        public IActionResult post()
        {
            return View();
        }
        public IActionResult contacts()
        {
            return View();
        }
        public IActionResult Gallery()
        {
            return View();
        }
        public IActionResult DirectorMessage()
        {
            return View();
        }

        // ============================================================
        // CONTACT QUERIES & SUBMISSION
        // ============================================================

        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> SubmitContactQuery([FromBody] ContactQuerySubmitDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Name) || string.IsNullOrWhiteSpace(dto.Mobile) || string.IsNullOrWhiteSpace(dto.Message))
            {
                return Json(new { success = false, message = "Please fill in all required fields." });
            }

            if (dto.Mobile.Length != 10)
            {
                return Json(new { success = false, message = "Please enter a valid 10-digit mobile number." });
            }

            var query = new TblContactQuery
            {
                Name = dto.Name,
                Email = dto.Email,
                Mobile = dto.Mobile,
                Subject = dto.Subject,
                Message = dto.Message,
                IsActive = true,
                IsRead = false,
                CreatedAt = DateTime.Now
            };

            _context.TblContactQueries.Add(query);
            await _context.SaveChangesAsync();

            return Json(new { success = true, message = "Thank you for contacting us! We will get back to you shortly." });
        }

        [HttpGet]
        public async Task<IActionResult> ContactRequests()
        {
            if (User?.Identity?.IsAuthenticated != true)
            {
                return RedirectToAction("Login", "Account");
            }

            var list = await _context.TblContactQueries
                .OrderByDescending(q => q.CreatedAt)
                .Select(q => new ContactQueryDto
                {
                    QueryId = q.QueryId,
                    Name = q.Name,
                    Email = q.Email,
                    Mobile = q.Mobile,
                    Subject = q.Subject,
                    Message = q.Message,
                    IsActive = q.IsActive,
                    IsRead = q.IsRead,
                    CreatedAt = q.CreatedAt
                })
                .ToListAsync();

            return View(list);
        }

        [HttpPost]
        public async Task<IActionResult> ToggleQueryStatus(int id)
        {
            if (User?.Identity?.IsAuthenticated != true)
            {
                return Json(new { success = false, message = "Access denied. Please login." });
            }

            var query = await _context.TblContactQueries.FindAsync(id);
            if (query == null)
            {
                return Json(new { success = false, message = "Query not found." });
            }

            query.IsActive = !query.IsActive;
            query.UpdatedAt = DateTime.Now;

            await _context.SaveChangesAsync();
            return Json(new { success = true, message = "Status toggled successfully." });
        }
    }

    public class ContactQueryDto
    {
        public int QueryId { get; set; }
        public string Name { get; set; } = null!;
        public string? Email { get; set; }
        public string Mobile { get; set; } = null!;
        public string? Subject { get; set; }
        public string Message { get; set; } = null!;
        public bool IsActive { get; set; }
        public bool IsRead { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class ContactQuerySubmitDto
    {
        public string Name { get; set; } = null!;
        public string? Email { get; set; }
        public string Mobile { get; set; } = null!;
        public string? Subject { get; set; }
        public string Message { get; set; } = null!;
    }
}
