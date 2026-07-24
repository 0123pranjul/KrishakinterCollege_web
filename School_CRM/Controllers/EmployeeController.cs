using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using School_CRM.Models;
using System.Text.Json;

namespace School_CRM.Controllers
{
    public class EmployeeController : Controller
    {
        private readonly LibmanagementContext _context; // Replace with your DbContext name

        public EmployeeController(LibmanagementContext context)
        {
            _context = context;
        }

        // GET: Employee/Index - Main Grid Page
        public async Task<IActionResult> Index()
        {
            //var employees = await _context.Employees
            //    .Where(e => e.IsActive == true)
            //    .ToListAsync();
            //    return View(employees);

            return View();
        }

        // GET: Employee/GetAll - AJAX Grid Data
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var employees = await _context.Employees
                .Where(e => e.IsActive == true)
                .Select(e => new
                {
                    e.Id,
                    e.EmployeeCode,
                    e.Name,
                    e.Designation,
                    e.Department,
                    BasicSalary = e.BasicSalary != null ? "₹" + e.BasicSalary.Value.ToString("0.00") : "₹0.00",
                    DailyRate = e.DailyRate != null ? "₹" + e.DailyRate.Value.ToString("0.00") : "₹0.00",
                    OvertimeRate = e.OvertimeRate != null ? "₹" + e.OvertimeRate.Value.ToString("0.00") : "₹0.00",
                    e.IsActive
                })
                .ToListAsync();

            return Json(new { data = employees });
        }

        // GET: Employee/CreateOrEdit/5
        [HttpGet]
        public async Task<IActionResult> CreateOrEdit(int id = 0)
        {
            if (id == 0)
                return PartialView("_EmployeeModal", new Employee());

            var employee = await _context.Employees.FindAsync(id);
            if (employee == null) return NotFound();

            return PartialView("_EmployeeModal", employee);
        }

        // POST: Employee/CreateOrEdit
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateOrEdit(int id, Employee employee)
        {
            if (id == 0)
            {
                // Create New
                employee.CreatedDate = DateTime.Now;
                employee.IsActive = true;
                _context.Employees.Add(employee);
            }
            else
            {
                // Update Existing
                _context.Entry(employee).State = EntityState.Modified;
            }

            try
            {
                // Auto calculate DailyRate & OvertimeRate
                if (employee.BasicSalary.HasValue)
                {
                    employee.DailyRate = employee.BasicSalary / 30;
                    employee.OvertimeRate = employee.DailyRate * 2;
                }

                await _context.SaveChangesAsync();
                return Json(new { success = true, message = "Employee saved successfully!" });
            }
            catch
            {
                return Json(new { success = false, message = "Error saving employee!" });
            }
        }

        // POST: Employee/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var employee = await _context.Employees.FindAsync(id);
            if (employee == null) return Json(new { success = false });

            employee.IsActive = false; // Soft Delete
            await _context.SaveChangesAsync();

            return Json(new { success = true, message = "Employee deleted successfully!" });
        }

        // GET: Employee/View/5 - Modal View
        [HttpGet]
        public async Task<IActionResult> View(int id)
        {
            var employee = await _context.Employees
                .Include(e => e.AttendanceMasters)
                .Include(e => e.EmployeeLeaves)
                .Include(e => e.SalaryMasters)
                .FirstOrDefaultAsync(e => e.Id == id);

            if (employee == null) return NotFound();

            return PartialView("_EmployeeViewModal", employee);
        }
    }
}