using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using School_CRM.Models;
using School_CRM.Models.ViewModels;

namespace School_CRM.Controllers
{
    public class StudentController : Controller
    {
        private readonly LibmanagementContext _context;
        private readonly IWebHostEnvironment _env;

        public StudentController(LibmanagementContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }

        // GET: Student/Index
        public IActionResult Index()
        {
            return View();
        }

        // GET: Student/GetAll - AJAX Grid Data
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var students = await _context.TblStudents
                .Where(s => s.IsActive == true)
                .Select(s => new
                {
                    s.StudentId,
                    s.StudentName,
                    s.RollNo,
                    s.AdmissionNo,
                    s.Gender,
                    Status = s.IsActive == true ? "Active" : "Inactive",
                    CreatedDate = s.CreatedDate.HasValue ? s.CreatedDate.Value.ToString("dd-MM-yyyy") : "-"
                })
                .ToListAsync();

            return Json(new { data = students });
        }

        // GET: Student/CreateOrEdit/5
        [HttpGet]
        public async Task<IActionResult> CreateOrEdit(int id = 0)
        {
            if (id == 0)
                return PartialView("_StudentModal", new TblStudent { IsActive = true });

            var student = await _context.TblStudents.FindAsync(id);
            if (student == null) return NotFound();

            return PartialView("_StudentModal", student);
        }

        // POST: Student/CreateOrEdit
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateOrEdit(int id, TblStudent student)
        {
            try
            {
                if (id == 0)
                {
                    student.CreatedDate = DateTime.Now;
                    student.CreatedBy = 1;
                    _context.TblStudents.Add(student);
                }
                else
                {
                    var existing = await _context.TblStudents.FindAsync(id);
                    if (existing == null)
                        return Json(new { success = false, message = "Student not found!" });

                    existing.StudentName = student.StudentName;
                    existing.RollNo = student.RollNo;
                    existing.IsActive = student.IsActive;
                    existing.UpdatedDate = DateTime.Now;
                    existing.UpdatedBy = 1;
                }

                await _context.SaveChangesAsync();
                return Json(new { success = true, message = "Student saved successfully!" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error saving student! " + ex.Message });
            }
        }

        // POST: Student/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var student = await _context.TblStudents.FindAsync(id);
            if (student == null)
                return Json(new { success = false, message = "Student not found!" });

            student.IsActive = false; // Soft Delete
            await _context.SaveChangesAsync();

            return Json(new { success = true, message = "Student deleted successfully!" });
        }

        // GET: Student/View/5
        [HttpGet]
        public async Task<IActionResult> View(int id)
        {
            var student = await _context.TblStudents
                .Include(s => s.TblStudentSessions)
                    .ThenInclude(ss => ss.Session)
                .Include(s => s.TblStudentSessions)
                    .ThenInclude(ss => ss.Class)
                .Include(s => s.TblStudentSessions)
                    .ThenInclude(ss => ss.Section)
                .Include(s => s.TblStudentParents)
                .Include(s => s.TblFeeCollections)
                .Include(s => s.TblStudentDues)
                .Include(s => s.TblStudentExtraCharges)
                .Include(s => s.TblStudentFeeOverrides)
                .FirstOrDefaultAsync(s => s.StudentId == id);

            if (student == null) return NotFound();

            return PartialView("_StudentViewModal", student);
        }

        // ── New Admission ────────────────────────────────────────────────────

        // GET: Student/NewAdmission
        [HttpGet]
        public async Task<IActionResult> NewAdmission(int id = 0)
        {
            var vm = await BuildAdmissionViewModel(id);
            return View(vm);
        }

        // POST: Student/NewAdmission
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> NewAdmission(NewAdmissionViewModel vm)
        {
            if (!ModelState.IsValid)
            {
                await PopulateDropdowns(vm);
                return View(vm);
            }

            await using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // ── Handle photo upload ──────────────────────────────────────
                string? photoUrl = vm.PhotoUrl;
                if (vm.PhotoFile != null && vm.PhotoFile.Length > 0)
                {
                    var allowed = new[] { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
                    var ext = Path.GetExtension(vm.PhotoFile.FileName).ToLowerInvariant();
                    if (!allowed.Contains(ext))
                    {
                        ModelState.AddModelError("PhotoFile", "Only image files (jpg, jpeg, png, gif, webp) are allowed.");
                        await PopulateDropdowns(vm);
                        return View(vm);
                    }

                    var uploadDir = Path.Combine(_env.WebRootPath, "uploads", "students");
                    Directory.CreateDirectory(uploadDir);
                    var fileName = $"{Guid.NewGuid()}{ext}";
                    var filePath = Path.Combine(uploadDir, fileName);
                    using (var stream = new FileStream(filePath, FileMode.Create))
                        await vm.PhotoFile.CopyToAsync(stream);

                    // Delete old photo if editing
                    if (!string.IsNullOrEmpty(vm.PhotoUrl))
                    {
                        var oldPath = Path.Combine(_env.WebRootPath, vm.PhotoUrl.TrimStart('/'));
                        if (System.IO.File.Exists(oldPath))
                            System.IO.File.Delete(oldPath);
                    }

                    photoUrl = $"/uploads/students/{fileName}";
                }

                TblStudent student;

                if (vm.StudentId == 0)
                {
                    // ── Create new student ───────────────────────────────────
                    student = new TblStudent
                    {
                        StudentName          = vm.StudentName,
                        AdmissionNo          = vm.AdmissionNo,
                        RollNo               = vm.RollNo,
                        AdmissionDate        = vm.AdmissionDate,
                        DateOfBirth          = vm.DateOfBirth,
                        Gender               = vm.Gender,
                        BloodGroup           = vm.BloodGroup,
                        AadhaarNo            = vm.AadhaarNo,
                        PreviousSchool       = vm.PreviousSchool,
                        AddressLine1         = vm.AddressLine1,
                        AddressLine2         = vm.AddressLine2,
                        City                 = vm.City,
                        State                = vm.State,
                        Pincode              = vm.Pincode,
                        EmergencyContactName   = vm.EmergencyContactName,
                        EmergencyContactNumber = vm.EmergencyContactNumber,
                        PhotoUrl             = photoUrl,
                        IsActive             = true,
                        CreatedDate          = DateTime.Now,
                        CreatedBy            = 1
                    };
                    _context.TblStudents.Add(student);
                    await _context.SaveChangesAsync();
                }
                else
                {
                    // ── Update existing student ──────────────────────────────
                    student = await _context.TblStudents.FindAsync(vm.StudentId)
                              ?? throw new Exception("Student not found.");

                    student.StudentName          = vm.StudentName;
                    student.AdmissionNo          = vm.AdmissionNo;
                    student.RollNo               = vm.RollNo;
                    student.AdmissionDate        = vm.AdmissionDate;
                    student.DateOfBirth          = vm.DateOfBirth;
                    student.Gender               = vm.Gender;
                    student.BloodGroup           = vm.BloodGroup;
                    student.AadhaarNo            = vm.AadhaarNo;
                    student.PreviousSchool       = vm.PreviousSchool;
                    student.AddressLine1         = vm.AddressLine1;
                    student.AddressLine2         = vm.AddressLine2;
                    student.City                 = vm.City;
                    student.State                = vm.State;
                    student.Pincode              = vm.Pincode;
                    student.EmergencyContactName   = vm.EmergencyContactName;
                    student.EmergencyContactNumber = vm.EmergencyContactNumber;
                    if (photoUrl != null) student.PhotoUrl = photoUrl;
                    student.UpdatedDate = DateTime.Now;
                    student.UpdatedBy   = 1;

                    await _context.SaveChangesAsync();
                }

                // ── Save / update father parent record ───────────────────────
                var father = await _context.TblStudentParents
                    .FirstOrDefaultAsync(p => p.StudentId == student.StudentId && p.ParentType == "Father");

                if (father == null)
                {
                    father = new TblStudentParent
                    {
                        StudentId   = student.StudentId,
                        ParentType  = "Father",
                        IsPrimary   = true,
                        IsActive    = true,
                        CreatedDate = DateTime.Now,
                        CreatedBy   = 1
                    };
                    _context.TblStudentParents.Add(father);
                }
                father.ParentName  = vm.FatherName;
                father.MobileNo    = vm.FatherMobile;
                father.Email       = vm.FatherEmail;
                father.Occupation  = vm.FatherOccupation;

                // ── Save / update mother parent record ───────────────────────
                if (!string.IsNullOrWhiteSpace(vm.MotherName))
                {
                    var mother = await _context.TblStudentParents
                        .FirstOrDefaultAsync(p => p.StudentId == student.StudentId && p.ParentType == "Mother");

                    if (mother == null)
                    {
                        mother = new TblStudentParent
                        {
                            StudentId   = student.StudentId,
                            ParentType  = "Mother",
                            IsPrimary   = false,
                            IsActive    = true,
                            CreatedDate = DateTime.Now,
                            CreatedBy   = 1
                        };
                        _context.TblStudentParents.Add(mother);
                    }
                    mother.ParentName = vm.MotherName;
                    mother.MobileNo   = vm.MotherMobile;
                    mother.Occupation = vm.MotherOccupation;
                }

                // ── Save / update student session ────────────────────────────
                var session = await _context.TblStudentSessions
                    .FirstOrDefaultAsync(s => s.StudentId == student.StudentId && s.IsActive == true);

                if (session == null)
                {
                    session = new TblStudentSession
                    {
                        StudentId   = student.StudentId,
                        IsActive    = true,
                        CreatedDate = DateTime.Now,
                        CreatedBy   = 1
                    };
                    _context.TblStudentSessions.Add(session);
                }
                session.SessionId = vm.SessionId;
                session.ClassId   = vm.ClassId;
                session.SectionId = vm.SectionId;

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                TempData["Success"] = vm.StudentId == 0
                    ? $"Admission registered successfully! Admission No: {student.AdmissionNo ?? student.StudentId.ToString()}"
                    : "Student record updated successfully!";

                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                ModelState.AddModelError(string.Empty, "Error saving admission: " + ex.Message);
                await PopulateDropdowns(vm);
                return View(vm);
            }
        }

        // ── Helpers ──────────────────────────────────────────────────────────

        private async Task<NewAdmissionViewModel> BuildAdmissionViewModel(int id)
        {
            var vm = new NewAdmissionViewModel();

            if (id > 0)
            {
                var student = await _context.TblStudents
                    .Include(s => s.TblStudentParents)
                    .Include(s => s.TblStudentSessions)
                    .FirstOrDefaultAsync(s => s.StudentId == id);

                if (student != null)
                {
                    vm.StudentId             = student.StudentId;
                    vm.StudentName           = student.StudentName ?? string.Empty;
                    vm.AdmissionNo           = student.AdmissionNo;
                    vm.RollNo                = student.RollNo;
                    vm.AdmissionDate         = student.AdmissionDate ?? DateOnly.FromDateTime(DateTime.Today);
                    vm.DateOfBirth           = student.DateOfBirth ?? DateOnly.FromDateTime(DateTime.Today);
                    vm.Gender                = student.Gender ?? string.Empty;
                    vm.BloodGroup            = student.BloodGroup;
                    vm.AadhaarNo             = student.AadhaarNo;
                    vm.PreviousSchool        = student.PreviousSchool;
                    vm.AddressLine1          = student.AddressLine1;
                    vm.AddressLine2          = student.AddressLine2;
                    vm.City                  = student.City;
                    vm.State                 = student.State;
                    vm.Pincode               = student.Pincode;
                    vm.EmergencyContactName  = student.EmergencyContactName;
                    vm.EmergencyContactNumber = student.EmergencyContactNumber;
                    vm.PhotoUrl              = student.PhotoUrl;

                    var father = student.TblStudentParents.FirstOrDefault(p => p.ParentType == "Father");
                    if (father != null)
                    {
                        vm.FatherName       = father.ParentName ?? string.Empty;
                        vm.FatherMobile     = father.MobileNo;
                        vm.FatherEmail      = father.Email;
                        vm.FatherOccupation = father.Occupation;
                    }

                    var mother = student.TblStudentParents.FirstOrDefault(p => p.ParentType == "Mother");
                    if (mother != null)
                    {
                        vm.MotherName       = mother.ParentName;
                        vm.MotherMobile     = mother.MobileNo;
                        vm.MotherOccupation = mother.Occupation;
                    }

                    var sess = student.TblStudentSessions.FirstOrDefault(s => s.IsActive == true);
                    if (sess != null)
                    {
                        vm.SessionId = sess.SessionId ?? 0;
                        vm.ClassId   = sess.ClassId   ?? 0;
                        vm.SectionId = sess.SectionId ?? 0;
                    }
                }
            }

            await PopulateDropdowns(vm);
            return vm;
        }

        private async Task PopulateDropdowns(NewAdmissionViewModel vm)
        {
            vm.Sessions = await _context.TblAcademicSessions
                .Where(s => s.IsActive == true)
                .OrderByDescending(s => s.StartDate)
                .Select(s => new SelectListItem
                {
                    Value = s.SessionId.ToString(),
                    Text  = s.SessionName ?? s.SessionId.ToString()
                })
                .ToListAsync();

            vm.Classes = await _context.TblClasses
                .Where(c => c.IsActive == true)
                .OrderBy(c => c.ClassName)
                .Select(c => new SelectListItem
                {
                    Value = c.ClassId.ToString(),
                    Text  = c.ClassName ?? c.ClassId.ToString()
                })
                .ToListAsync();

            vm.Sections = await _context.TblSections
                .Where(s => s.IsActive == true)
                .OrderBy(s => s.SectionName)
                .Select(s => new SelectListItem
                {
                    Value = s.SectionId.ToString(),
                    Text  = s.SectionName ?? s.SectionId.ToString()
                })
                .ToListAsync();
        }
    }
}
