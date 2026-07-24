using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using School_CRM.Models;

namespace School_CRM.Controllers
{
    public class TeacherController : Controller
    {
        private readonly LibmanagementContext _context;
        public TeacherController(LibmanagementContext context) => _context = context;

        public IActionResult Index() => View();

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var data = await _context.TblTeachers
                .Where(t => t.IsActive == true)
                .Select(t => new
                {
                    t.TeacherId,
                    t.TeacherName,
                    t.MobileNo,
                    t.Email,
                    t.Designation,
                    JoiningDate = t.JoiningDate.HasValue ? t.JoiningDate.Value.ToString("dd-MM-yyyy") : "-",
                    Status = t.IsActive == true ? "Active" : "Inactive",
                    CreatedDate = t.CreatedDate.HasValue ? t.CreatedDate.Value.ToString("dd-MM-yyyy") : "-"
                }).ToListAsync();
            return Json(new { data });
        }

        [HttpGet]
        public async Task<IActionResult> CreateOrEdit(int id = 0)
        {
            if (id == 0) return PartialView("_TeacherModal", new TblTeacher { IsActive = true });
            var item = await _context.TblTeachers.FindAsync(id);
            if (item == null) return NotFound();
            return PartialView("_TeacherModal", item);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateOrEdit(int id, TblTeacher model)
        {
            try
            {
                if (id == 0)
                {
                    model.CreatedDate = DateTime.Now;
                    model.IsActive = true;
                    _context.TblTeachers.Add(model);
                }
                else
                {
                    var existing = await _context.TblTeachers.FindAsync(id);
                    if (existing == null) return Json(new { success = false, message = "Record not found!" });
                    existing.TeacherName = model.TeacherName;
                    existing.MobileNo = model.MobileNo;
                    existing.Email = model.Email;
                    existing.Designation = model.Designation;
                    existing.JoiningDate = model.JoiningDate;
                    existing.IsActive = model.IsActive;
                    existing.UpdatedDate = DateTime.Now;
                }
                await _context.SaveChangesAsync();
                return Json(new { success = true, message = id == 0 ? "Teacher added successfully!" : "Teacher updated successfully!" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            var item = await _context.TblTeachers.FindAsync(id);
            if (item == null) return Json(new { success = false, message = "Record not found!" });
            item.IsActive = false;
            item.UpdatedDate = DateTime.Now;
            await _context.SaveChangesAsync();
            return Json(new { success = true, message = "Teacher deleted successfully!" });
        }
    }
}
