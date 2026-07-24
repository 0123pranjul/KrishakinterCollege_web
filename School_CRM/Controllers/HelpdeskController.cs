using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using School_CRM.Models;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace School_CRM.Controllers
{
    public class HelpdeskController : Controller
    {
        private readonly LibmanagementContext _context;

        public HelpdeskController(LibmanagementContext context)
        {
            _context = context;
        }

        // GET: Helpdesk
        public async Task<IActionResult> Index()
        {
            var userIdStr = Request.Cookies["userId"];
            if (string.IsNullOrEmpty(userIdStr))
            {
                return RedirectToAction("Login", "Account");
            }
            int userId = int.Parse(userIdStr);
            bool isAdmin = Request.Cookies["IsAdmin"] == "true";

            ViewBag.IsAdmin = isAdmin;
            ViewBag.Categories = await _context.TblHelpdeskCategories.Where(c => c.IsActive).ToListAsync();
            ViewBag.Employees = await _context.Employees.Where(e => e.IsActive == true).Select(e => new { e.Id, e.Name }).ToListAsync();

            IQueryable<TblHelpdeskTicket> query = _context.TblHelpdeskTickets
                .Include(t => t.Category)
                .Include(t => t.RaisedByNavigation)
                .Include(t => t.AssignedToNavigation);

            if (!isAdmin)
            {
                // Users see only their own raised tickets
                query = query.Where(t => t.RaisedBy == userId);
            }

            var tickets = await query.OrderByDescending(t => t.CreatedDate).ToListAsync();
            return View(tickets);
        }

        // POST: Helpdesk/Create
        [HttpPost]
        public async Task<IActionResult> Create(int categoryId, string title, string description, string priority, IFormFile? attachment)
        {
            try
            {
                var userIdStr = Request.Cookies["userId"];
                if (string.IsNullOrEmpty(userIdStr))
                {
                    return Json(new { success = false, message = "User not logged in." });
                }
                int userId = int.Parse(userIdStr);

                string ticketNo = $"TKT-{DateTime.Now:yyyyMMdd-HHmmss}";
                string? attachmentUrl = null;

                if (attachment != null && attachment.Length > 0)
                {
                    var uploadsDir = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "helpdesk");
                    Directory.CreateDirectory(uploadsDir);
                    var fileName = $"{Guid.NewGuid()}_{Path.GetFileName(attachment.FileName)}";
                    var filePath = Path.Combine(uploadsDir, fileName);
                    using var stream = new FileStream(filePath, FileMode.Create);
                    await attachment.CopyToAsync(stream);
                    attachmentUrl = $"/uploads/helpdesk/{fileName}";
                }

                var ticket = new TblHelpdeskTicket
                {
                    TicketNo = ticketNo,
                    CategoryId = categoryId,
                    Title = title,
                    Description = description,
                    Priority = priority ?? "Normal",
                    Status = "Open",
                    RaisedBy = userId,
                    CreatedDate = DateTime.Now,
                    AttachmentUrl = attachmentUrl
                };

                _context.TblHelpdeskTickets.Add(ticket);
                await _context.SaveChangesAsync();

                return Json(new { success = true, message = "Ticket raised successfully!" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        // GET: Helpdesk/Details/5
        public async Task<IActionResult> Details(int id)
        {
            var userIdStr = Request.Cookies["userId"];
            if (string.IsNullOrEmpty(userIdStr))
            {
                return RedirectToAction("Login", "Account");
            }
            int userId = int.Parse(userIdStr);
            bool isAdmin = Request.Cookies["IsAdmin"] == "true";

            var ticket = await _context.TblHelpdeskTickets
                .Include(t => t.Category)
                .Include(t => t.RaisedByNavigation)
                .Include(t => t.AssignedToNavigation)
                .Include(t => t.TblHelpdeskReplies)
                    .ThenInclude(r => r.ReplyByNavigation)
                .FirstOrDefaultAsync(t => t.Id == id);

            if (ticket == null)
            {
                return NotFound();
            }

            // Security check: regular users can only see their own tickets
            if (!isAdmin && ticket.RaisedBy != userId)
            {
                return Forbid();
            }

            ViewBag.IsAdmin = isAdmin;
            ViewBag.CurrentUserId = userId;
            ViewBag.Employees = await _context.Employees.Where(e => e.IsActive == true).ToListAsync();

            return View(ticket);
        }

        // POST: Helpdesk/Reply
        [HttpPost]
        public async Task<IActionResult> Reply(int ticketId, string replyMessage)
        {
            try
            {
                var userIdStr = Request.Cookies["userId"];
                if (string.IsNullOrEmpty(userIdStr))
                {
                    return Json(new { success = false, message = "User not logged in." });
                }
                int userId = int.Parse(userIdStr);
                bool isAdmin = Request.Cookies["IsAdmin"] == "true";

                var ticket = await _context.TblHelpdeskTickets.FindAsync(ticketId);
                if (ticket == null)
                {
                    return Json(new { success = false, message = "Ticket not found." });
                }

                if (!isAdmin && ticket.RaisedBy != userId)
                {
                    return Json(new { success = false, message = "Unauthorized action." });
                }

                var reply = new TblHelpdeskReply
                {
                    TicketId = ticketId,
                    ReplyMessage = replyMessage,
                    ReplyBy = userId,
                    IsAdminReply = isAdmin,
                    CreatedDate = DateTime.Now
                };

                _context.TblHelpdeskReplies.Add(reply);

                // Auto-set ticket to "In Progress" if admin replies and ticket was "Open"
                if (isAdmin && ticket.Status == "Open")
                {
                    ticket.Status = "In Progress";
                }

                await _context.SaveChangesAsync();

                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        // POST: Helpdesk/UpdateStatus (Admin only)
        [HttpPost]
        public async Task<IActionResult> UpdateStatus(int ticketId, string status, int? assignedTo, string remarks)
        {
            try
            {
                bool isAdmin = Request.Cookies["IsAdmin"] == "true";
                if (!isAdmin)
                {
                    return Json(new { success = false, message = "Unauthorized access." });
                }

                var ticket = await _context.TblHelpdeskTickets.FindAsync(ticketId);
                if (ticket == null)
                {
                    return Json(new { success = false, message = "Ticket not found." });
                }

                ticket.Status = status;
                ticket.AssignedTo = assignedTo;
                ticket.Remarks = remarks;

                if (status == "Resolved" || status == "Closed")
                {
                    ticket.ResolvedDate = DateTime.Now;
                }
                else
                {
                    ticket.ResolvedDate = null;
                }

                await _context.SaveChangesAsync();
                return Json(new { success = true, message = "Ticket updated successfully." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }
    }
}
