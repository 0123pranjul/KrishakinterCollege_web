using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using School_CRM.Models;

namespace School_CRM.Controllers
{
    public class EnquiryController : Controller
    {
        private readonly LibmanagementContext _context;

        public EnquiryController(LibmanagementContext context)
        {
            _context = context;
        }

        private int CurrentUserId =>
            int.TryParse(
                HttpContext.Request.Cookies["EmployeeId"],
                out var id) ? id : 0;
        private bool IsAdmin =>
            HttpContext.Request.Cookies["IsAdmin"] == "true";

        // ── PUBLIC FORM (No Layout) ───────────────────────────
        [AllowAnonymous]
        [HttpGet]
        public async Task<IActionResult> PublicForm()
        {
            ViewBag.Classes = await _context.TblClasses
                .Where(c => c.IsActive == true)
                .OrderBy(c => c.ClassName)
                .ToListAsync();

            ViewBag.Sessions = await _context.TblAcademicSessions
                .Where(s => s.IsActive == true)
                .OrderByDescending(s => s.SessionId)
                .ToListAsync();

            return View();
        }



        [AllowAnonymous]
        [HttpPost]
        [IgnoreAntiforgeryToken]
        public async Task<IActionResult> SubmitPublicForm(
            [FromBody] PublicEnquiryDto dto)
        {
            try
            {
                // Verify Captcha
                var sessionCaptcha = TempData["EnquiryCaptchaCode"] as string;
                if (string.IsNullOrEmpty(sessionCaptcha) || string.IsNullOrEmpty(dto.CaptchaInput) || dto.CaptchaInput.Trim().ToUpper() != sessionCaptcha.ToUpper())
                {
                    return Json(new
                    {
                        success = false,
                        message = "Incorrect Verification Code. Please try again."
                    });
                }
                var enquiry = new TblEnquiry
                {
                    StudentName = dto.StudentName,
                    DateOfBirth = dto.DateOfBirth.HasValue
                        ? DateOnly.FromDateTime(dto.DateOfBirth.Value)
                        : null,
                    Gender = dto.Gender,
                    ParentName = dto.ParentName,
                    MobileNo = dto.MobileNo,
                    AlternateMobile = dto.AlternateMobile,
                    Email = dto.Email,
                    Address = dto.Address,
                    City = dto.City,
                    InterestedClassId = dto.InterestedClassId,
                    SessionId = dto.SessionId,
                    Source = "Website",
                    Status = "New",
                    EnquiryDate = DateTime.Now,
                    IsActive = true,
                    CreatedDate = DateTime.Now
                };

                _context.TblEnquiries.Add(enquiry);
                await _context.SaveChangesAsync();

                return Json(new
                {
                    success = true,
                    message = "Enquiry submitted successfully! " +
                              "We will contact you soon.",
                    enquiryId = enquiry.EnquiryId
                });
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    success = false,
                    message = ex.Message
                });
            }
        }

        // ── ADMIN LIST ────────────────────────────────────────
        public IActionResult Index()
        {
            ViewBag.IsAdmin = IsAdmin;
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> GetAll(
            string? status, string? source,
            int? classId, int? sessionId)
        {
            var query = _context.TblEnquiries
                .Include(e => e.InterestedClass)
                .Include(e => e.Session)
                .Where(e => e.IsActive == true)
                .AsQueryable();

            if (!string.IsNullOrEmpty(status))
                query = query.Where(e => e.Status == status);
            if (!string.IsNullOrEmpty(source))
                query = query.Where(e => e.Source == source);
            if (classId.HasValue)
                query = query.Where(
                    e => e.InterestedClassId == classId);
            if (sessionId.HasValue)
                query = query.Where(
                    e => e.SessionId == sessionId);

            var list = await query
                .OrderByDescending(e => e.EnquiryDate)
                .Select(e => new {
                    e.EnquiryId,
                    e.StudentName,
                    e.ParentName,
                    e.MobileNo,
                    e.Gender,
                    e.City,
                    e.Source,
                    e.Status,
                    className = e.InterestedClass != null
                        ? e.InterestedClass.ClassName : "-",
                    sessionName = e.Session != null
                        ? e.Session.SessionName : "-",
                    enquiryDate = e.EnquiryDate.HasValue
                        ? e.EnquiryDate.Value
                            .ToString("dd-MM-yyyy") : "-",
                    followUpCount =
                        e.TblEnquiryFollowUps
                         .Count(f => f.IsActive == true)
                }).ToListAsync();

            return Json(new { data = list });
        }

        // ── ENQUIRY DETAIL ────────────────────────────────────
        [HttpGet]
        public async Task<IActionResult> Detail(int id)
        {
            var enquiry = await _context.TblEnquiries
                .Include(e => e.InterestedClass)
                .Include(e => e.Session)
                .Include(e => e.TblEnquiryFollowUps
                    .Where(f => f.IsActive == true)
                    .OrderByDescending(f => f.FollowUpDate))
                .Include(e => e.TblEnquiryDocuments
                    .Where(d => d.IsActive == true))
                .FirstOrDefaultAsync(e => e.EnquiryId == id);

            if (enquiry == null)
                return NotFound();

            ViewBag.Classes = await _context.TblClasses
                .Where(c => c.IsActive == true)
                .ToListAsync();
            ViewBag.Sessions = await _context.TblAcademicSessions
                .Where(s => s.IsActive == true)
                .ToListAsync();
            ViewBag.Employees = await _context.Employees
                .Where(e => e.IsActive == true)
                .ToListAsync();

            return View(enquiry);
        }

        // ── UPDATE STATUS ─────────────────────────────────────
        [HttpPost]
        [IgnoreAntiforgeryToken]
        public async Task<IActionResult> UpdateStatus(
            [FromBody] UpdateStatusDto dto)
        {
            try
            {
                var enquiry = await _context.TblEnquiries
                    .FindAsync(dto.EnquiryId);
                if (enquiry == null)
                    return Json(new
                    {
                        success = false,
                        message = "Not found!"
                    });

                enquiry.Status = dto.Status;
                enquiry.UpdatedBy = CurrentUserId;
                enquiry.UpdatedDate = DateTime.Now;
                if (!string.IsNullOrEmpty(dto.Remarks))
                    enquiry.Remarks = dto.Remarks;

                await _context.SaveChangesAsync();
                return Json(new
                {
                    success = true,
                    message = $"Status updated to {dto.Status}!"
                });
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    success = false,
                    message = ex.Message
                });
            }
        }

        // ── ADD FOLLOW UP ─────────────────────────────────────
        [HttpPost]
        [IgnoreAntiforgeryToken]
        public async Task<IActionResult> AddFollowUp(
            [FromBody] FollowUpDto dto)
        {
            try
            {
                var followUp = new TblEnquiryFollowUp
                {
                    EnquiryId = dto.EnquiryId,
                    FollowUpDate = DateTime.Now,
                    NextFollowUpDate = dto.NextFollowUpDate,
                    Status = dto.Status,
                    Remarks = dto.Remarks,
                    IsActive = true,
                    CreatedBy = CurrentUserId,
                    CreatedDate = DateTime.Now
                };

                _context.TblEnquiryFollowUps.Add(followUp);

                // Update enquiry status
                var enquiry = await _context.TblEnquiries
                    .FindAsync(dto.EnquiryId);
                if (enquiry != null)
                {
                    enquiry.Status = dto.EnquiryStatus;
                    enquiry.UpdatedDate = DateTime.Now;
                    enquiry.UpdatedBy = CurrentUserId;
                }

                await _context.SaveChangesAsync();
                return Json(new
                {
                    success = true,
                    message = "Follow-up added!"
                });
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    success = false,
                    message = ex.Message
                });
            }
        }

        // ── ADD DOCUMENT ──────────────────────────────────────
        [HttpPost]
        [IgnoreAntiforgeryToken]
        public async Task<IActionResult> AddDocument(
            [FromBody] DocumentDto dto)
        {
            try
            {
                _context.TblEnquiryDocuments.Add(
                    new TblEnquiryDocument
                    {
                        EnquiryId = dto.EnquiryId,
                        DocumentType = dto.DocumentType,
                        DocumentUrl = dto.DocumentUrl,
                        IsActive = true,
                        CreatedBy = CurrentUserId,
                        CreatedDate = DateTime.Now
                    });

                await _context.SaveChangesAsync();
                return Json(new
                {
                    success = true,
                    message = "Document added!"
                });
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    success = false,
                    message = ex.Message
                });
            }
        }

        // ── CONVERT TO ADMISSION (GET form) ───────────────────
        [HttpGet]
        public async Task<IActionResult> ConvertToAdmission(int id)
        {
            var enquiry = await _context.TblEnquiries
                .Include(e => e.InterestedClass)
                .Include(e => e.Session)
                .FirstOrDefaultAsync(e => e.EnquiryId == id);

            if (enquiry == null) return NotFound();

            ViewBag.Classes = await _context.TblClasses
                .Where(c => c.IsActive == true)
                .ToListAsync();
            ViewBag.Sessions = await _context.TblAcademicSessions
                .Where(s => s.IsActive == true)
                .ToListAsync();
            ViewBag.Sections = await _context.TblSections
                .Where(s => s.IsActive == true)
                .ToListAsync();

            return View(enquiry);
        }

        // ── SAVE ADMISSION + CREATE STUDENT ───────────────────
        [HttpPost]
        [IgnoreAntiforgeryToken]
        public async Task<IActionResult> SaveAdmission(
            [FromBody] SaveAdmissionDto dto)
        {
            using var transaction =
                await _context.Database.BeginTransactionAsync();
            try
            {
                // 1. Create Student
                var student = new TblStudent
                {
                    StudentName = dto.StudentName,
                    DateOfBirth = dto.DateOfBirth.HasValue
                        ? DateOnly.FromDateTime(dto.DateOfBirth.Value)
                        : null,
                    Gender = dto.Gender,
                    AdmissionNo = await GenerateAdmissionNo(),
                    AdmissionDate = DateOnly.FromDateTime(
                        dto.AdmissionDate),
                    AddressLine1 = dto.Address,
                    City = dto.City,
                    IsActive = true,
                    CreatedBy = CurrentUserId,
                    CreatedDate = DateTime.Now
                };

                _context.TblStudents.Add(student);
                await _context.SaveChangesAsync();

                // 2. Create Admission
                var admission = new TblAdmission
                {
                    StudentId = student.StudentId,
                    SessionId = dto.SessionId,
                    ClassId = dto.ClassId,
                    SectionId = dto.SectionId,
                    AdmissionDate = DateOnly.FromDateTime(
                        dto.AdmissionDate),
                    JoiningDate = dto.JoiningDate.HasValue
                        ? DateOnly.FromDateTime(dto.JoiningDate.Value)
                        : null,
                    AdmissionType = dto.AdmissionType,
                    AdmissionStatus = "Pending",
                    Remarks = dto.Remarks,
                    IsActive = true,
                    CreatedBy = CurrentUserId,
                    CreatedDate = DateTime.Now
                };

                _context.TblAdmissions.Add(admission);

                // 3. Update Enquiry to Converted
                var enquiry = await _context.TblEnquiries
                    .FindAsync(dto.EnquiryId);
                if (enquiry != null)
                {
                    enquiry.Status = "Converted";
                    enquiry.UpdatedDate = DateTime.Now;
                    enquiry.UpdatedBy = CurrentUserId;
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return Json(new
                {
                    success = true,
                    message = "Student admitted successfully!",
                    studentId = student.StudentId,
                    admissionId = admission.AdmissionId
                });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return Json(new
                {
                    success = false,
                    message = ex.Message
                });
            }
        }

        // ── DELETE ENQUIRY ────────────────────────────────────
        [HttpPost]
        [IgnoreAntiforgeryToken]
        public async Task<IActionResult> Delete(
            [FromBody] int id)
        {
            try
            {
                var e = await _context.TblEnquiries.FindAsync(id);
                if (e == null)
                    return Json(new { success = false });
                e.IsActive = false;
                e.UpdatedDate = DateTime.Now;
                await _context.SaveChangesAsync();
                return Json(new
                {
                    success = true,
                    message = "Deleted!"
                });
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    success = false,
                    message = ex.Message
                });
            }
        }

        // ── HELPERS ───────────────────────────────────────────
        [AllowAnonymous]
        [HttpGet]
        public async Task<IActionResult> GetClasses()
        {
            var list = await _context.TblClasses
                .Where(c => c.IsActive == true)
                .OrderBy(c => c.ClassName)
                .Select(c => new {
                    c.ClassId,
                    c.ClassName
                }).ToListAsync();
            return Json(list);
        }

        [AllowAnonymous]
        [HttpGet]
        public async Task<IActionResult> GetSessions()
        {
            var list = await _context.TblAcademicSessions
                .Where(s => s.IsActive == true)
                .OrderByDescending(s => s.SessionId)
                .Select(s => new {
                    s.SessionId,
                    s.SessionName
                }).ToListAsync();
            return Json(list);
        }

        [HttpGet]
        public async Task<IActionResult> GetSections(int classId)
        {
            var list = await _context.TblSections
                //.Where(s => s.IsActive == true &&
                //            s.ClassId == classId)
                .Select(s => new {
                    s.SectionId,
                    s.SectionName
                }).ToListAsync();
            return Json(list);
        }

        [AllowAnonymous]
        [HttpGet]
        public IActionResult GetCaptchaImage()
        {
            var code = GenerateRandomCaptchaString(5);
            TempData["EnquiryCaptchaCode"] = code;
            var svg = GenerateCaptchaSvg(code);
            return Content(svg, "image/svg+xml");
        }

        private string GenerateRandomCaptchaString(int length)
        {
            const string chars = "ABCDEFGHJKLMNOPQRSTUVWXYZ23456789";
            var random = new Random();
            return new string(Enumerable.Repeat(chars, length)
                .Select(s => s[random.Next(s.Length)]).ToArray());
        }

        private string GenerateCaptchaSvg(string code)
        {
            var random = new Random();
            var width = 120;
            var height = 38;
            var svg = $"<svg width='{width}' height='{height}' xmlns='http://www.w3.org/2000/svg' style='background: #f8fafc; border-radius: 8px; border: 1.5px solid #cbd5e1;'>";
            
            // Draw noise lines
            for (int i = 0; i < 4; i++)
            {
                var x1 = random.Next(width);
                var y1 = random.Next(height);
                var x2 = random.Next(width);
                var y2 = random.Next(height);
                svg += $"<line x1='{x1}' y1='{y1}' x2='{x2}' y2='{y2}' stroke='rgba(75,85,99,0.15)' stroke-width='1.5'/>";
            }

            // Draw text
            var colors = new[] { "#0f172a", "#0d9488", "#2563eb", "#dc2626", "#65a30d" };
            for (int i = 0; i < code.Length; i++)
            {
                var ch = code[i];
                var fontSize = random.Next(20, 24);
                var angle = random.Next(-15, 15);
                var x = 12 + (i * 20) + random.Next(-2, 2);
                var y = 26 + random.Next(-3, 3);
                var color = colors[random.Next(colors.Length)];
                svg += $"<text x='{x}' y='{y}' font-size='{fontSize}' font-weight='bold' fill='{color}' font-family='Courier New, monospace' transform='rotate({angle} {x} {y})'>{ch}</text>";
            }

            svg += "</svg>";
            return svg;
        }

        private async Task<string> GenerateAdmissionNo()
        {
            var year = DateTime.Today.Year;
            var count = await _context.TblStudents
                .CountAsync(s => s.CreatedDate.HasValue &&
                    s.CreatedDate.Value.Year == year);
            return $"ADM{year}{(count + 1):D4}";
        }
    }

    // ── DTOs ─────────────────────────────────────────────────
    public class PublicEnquiryDto
    {
        public string? StudentName { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public string? Gender { get; set; }
        public string? ParentName { get; set; }
        public string? MobileNo { get; set; }
        public string? AlternateMobile { get; set; }
        public string? Email { get; set; }
        public string? Address { get; set; }
        public string? City { get; set; }
        public int? InterestedClassId { get; set; }
        public int? SessionId { get; set; }
        public string? CaptchaInput { get; set; }
    }

    public class UpdateStatusDto
    {
        public int EnquiryId { get; set; }
        public string? Status { get; set; }
        public string? Remarks { get; set; }
    }

    public class FollowUpDto
    {
        public int EnquiryId { get; set; }
        public string? Status { get; set; }
        public string? EnquiryStatus { get; set; }
        public string? Remarks { get; set; }
        public DateTime? NextFollowUpDate { get; set; }
    }

    public class DocumentDto
    {
        public int? EnquiryId { get; set; }
        public string? DocumentType { get; set; }
        public string? DocumentUrl { get; set; }
    }

    public class SaveAdmissionDto
    {
        public int EnquiryId { get; set; }
        public string? StudentName { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public string? Gender { get; set; }
        public string? Address { get; set; }
        public string? City { get; set; }
        public int? SessionId { get; set; }
        public int? ClassId { get; set; }
        public int? SectionId { get; set; }
        public DateTime AdmissionDate { get; set; }
        public DateTime? JoiningDate { get; set; }
        public string? AdmissionType { get; set; }
        public string? Remarks { get; set; }
    }
}