using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using School_CRM.Models;
using System.Text.Json;

namespace School_CRM.Controllers
{
    [AllowAnonymous]
    public class ResultController : Controller
    {
        private readonly LibmanagementContext _context;

        public ResultController(LibmanagementContext context)
        {
            _context = context;
        }

        // GET: /Result/Index
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            // Only fetch active sessions for the dropdown
            ViewBag.Sessions = await _context.TblAcademicSessions
                .Where(s => s.IsActive == true)
                .OrderByDescending(s => s.StartDate)
                .ToListAsync();

            return View();
        }

        // POST: /Result/Search
        [AllowAnonymous]
        [HttpPost]
        public async Task<IActionResult> Search([FromBody] ResultSearchRequest request)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(request.StudentCode) || string.IsNullOrWhiteSpace(request.DateOfBirth))
                {
                    return Json(new { success = false, message = "Please provide both Student ID/Roll No and Date of Birth." });
                }

                if (!DateOnly.TryParse(request.DateOfBirth, out DateOnly parsedDob))
                {
                    return Json(new { success = false, message = "Invalid Date of Birth format." });
                }

                // Find the student based on RollNo or AdmissionNo or StudentId
                var student = await _context.TblStudents
                    .FirstOrDefaultAsync(s => s.IsActive == true && 
                        s.DateOfBirth == parsedDob &&
                        (s.RollNo == request.StudentCode || s.AdmissionNo == request.StudentCode || s.StudentId.ToString() == request.StudentCode));

                if (student == null)
                {
                    return Json(new { success = false, message = "Invalid Student Credentials or Date of Birth. Please check again." });
                }

                // Check if the report card for this session is generated AND published
                var reportCard = await _context.TblReportCards
                    .Include(rc => rc.Session)
                    .Include(rc => rc.Class)
                    .Include(rc => rc.Section)
                    .Include(rc => rc.Grade)
                    .Include(rc => rc.TblReportCardSubjects).ThenInclude(rcs => rcs.Subject)
                    .Include(rc => rc.TblReportCardSubjects).ThenInclude(rcs => rcs.Grade)
                    .FirstOrDefaultAsync(rc => rc.StudentId == student.StudentId && 
                                               rc.SessionId == request.SessionId && 
                                               rc.IsActive == true);

                if (reportCard == null)
                {
                    return Json(new { success = false, message = "Report Card has not been generated for the selected session." });
                }

                if (reportCard.IsPublished != true)
                {
                    return Json(new { success = false, message = "Your Result is Pending or Not Yet Published by the school administration." });
                }

                reportCard.Student = student;
                
                return PartialView("~/Views/ReportCard/_ReportCardView.cshtml", reportCard);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "An error occurred: " + ex.Message });
            }
        }

        // GET: /Result/Verify?code=XYZ
        [HttpGet]
        public async Task<IActionResult> Verify(string code)
        {
            if (string.IsNullOrWhiteSpace(code)) return NotFound();

            var reportCard = await _context.TblReportCards
                .Include(rc => rc.Student)
                .Include(rc => rc.Class)
                .FirstOrDefaultAsync(rc => rc.VerificationCode == code && rc.IsActive == true);

            if (reportCard == null)
            {
                ViewBag.Message = "Invalid Verification Code. This Marksheet could not be verified.";
                ViewBag.IsValid = false;
            }
            else
            {
                ViewBag.Message = $"Authentic Marksheet Verified. Student: {reportCard.Student.StudentName}, Class: {reportCard.Class.ClassName}, Status: {(reportCard.IsPublished == true ? "Published" : "Draft")}";
                ViewBag.IsValid = true;
            }

            return View(); // Simple verification landing page
        }
    }

    public class ResultSearchRequest
    {
        public int SessionId { get; set; }
        public string StudentCode { get; set; } = string.Empty;
        public string? DateOfBirth { get; set; }
    }
}
