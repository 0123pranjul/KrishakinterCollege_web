using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using School_CRM.Models;
using System.Linq;
using System.Threading.Tasks;
using System;

namespace School_CRM.Controllers
{
    public class LessonPlanController : Controller
    {
        private readonly LibmanagementContext _context;

        public LessonPlanController(LibmanagementContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var plans = await _context.TblLessonPlans
                .Include(p => p.Class)
                .Include(p => p.Subject)
                .Include(p => p.Employee)
                .OrderByDescending(p => p.CreatedDate)
                .ToListAsync();

            return View(plans);
        }

        public async Task<IActionResult> Create(int? id)
        {
            ViewBag.Classes = await _context.TblClasses.Where(c => c.IsActive == true).ToListAsync();
            ViewBag.Subjects = await _context.TblSubjects.Where(s => s.IsActive == true).ToListAsync();
            ViewBag.Teachers = await _context.Employees.ToListAsync();

            if (id.HasValue && id.Value > 0)
            {
                var plan = await _context.TblLessonPlans.FindAsync(id.Value);
                if (plan != null) return View(plan);
            }

            return View(new TblLessonPlan { StartDate = DateOnly.FromDateTime(DateTime.Today), EndDate = DateOnly.FromDateTime(DateTime.Today.AddDays(7)) });
        }

        [HttpPost]
        public async Task<IActionResult> Save(TblLessonPlan model)
        {
            try
            {
                var userIdStr = Request.Cookies["userId"];
                int userId = !string.IsNullOrEmpty(userIdStr) ? int.Parse(userIdStr) : 1;

                if (model.Id == 0)
                {
                    model.CreatedBy = userId;
                    model.CreatedDate = DateTime.Now;
                    if (string.IsNullOrEmpty(model.Status)) model.Status = "Pending";
                    _context.TblLessonPlans.Add(model);
                }
                else
                {
                    var existing = await _context.TblLessonPlans.FindAsync(model.Id);
                    if (existing != null)
                    {
                        existing.PlanTitle = model.PlanTitle;
                        existing.ClassId = model.ClassId;
                        existing.SubjectId = model.SubjectId;
                        existing.EmployeeId = model.EmployeeId;
                        existing.StartDate = model.StartDate;
                        existing.EndDate = model.EndDate;
                        existing.Objectives = model.Objectives ?? "";
                        existing.TeachingMethod = model.TeachingMethod ?? "";
                        existing.RequiredMaterials = model.RequiredMaterials;
                        existing.Status = "Pending"; // Resubmit on edit
                    }
                }
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "Error saving Lesson Plan: " + ex.Message);
                ViewBag.Classes = await _context.TblClasses.Where(c => c.IsActive == true).ToListAsync();
                ViewBag.Subjects = await _context.TblSubjects.Where(s => s.IsActive == true).ToListAsync();
                ViewBag.Teachers = await _context.Employees.ToListAsync();
                return View("Create", model);
            }
        }

        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var plan = await _context.TblLessonPlans.FindAsync(id);
                if (plan != null)
                {
                    _context.TblLessonPlans.Remove(plan);
                    await _context.SaveChangesAsync();
                }
                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }
    }
}
