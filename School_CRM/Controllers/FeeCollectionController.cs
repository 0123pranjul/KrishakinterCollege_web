using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using School_CRM.Models;

namespace School_CRM.Controllers
{
    public class FeeCollectionController : Controller
    {
        private readonly LibmanagementContext _context;

        public FeeCollectionController(LibmanagementContext context)
        {
            _context = context;
        }

        // GET: FeeCollection/Index
        public IActionResult Index()
        {
            return View();
        }

        // GET: FeeCollection/SearchStudentsForFeeDetail - Select2 AJAX
        [HttpGet]
        public async Task<IActionResult> SearchStudentsForFeeDetail(string? term, int page = 1)
        {
            const int pageSize = 20;
            var query = _context.TblStudentSessions
                .Where(ss => ss.IsActive == true && ss.Student != null && ss.Student.IsActive == true)
                .Include(ss => ss.Student)
                .Include(ss => ss.Class)
                .Include(ss => ss.Section)
                .Include(ss => ss.Session)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(term))
            {
                string t = term.Trim().ToLower();
                query = query.Where(ss =>
                    (ss.Student!.StudentName != null && ss.Student.StudentName.ToLower().Contains(t)) ||
                    (ss.Student!.RollNo      != null && ss.Student.RollNo.ToLower().Contains(t))      ||
                    (ss.Student!.AdmissionNo != null && ss.Student.AdmissionNo.ToLower().Contains(t)));
            }

            var total = await query.Select(ss => ss.StudentId).Distinct().CountAsync();
            var data  = await query
                .OrderBy(ss => ss.Student!.StudentName)
                .Skip((page - 1) * pageSize).Take(pageSize)
                .Select(ss => new {
                    id          = ss.Student!.StudentId,
                    text        = ss.Student.StudentName + " | Roll: " + ss.Student.RollNo,
                    name        = ss.Student.StudentName  ?? "-",
                    roll        = ss.Student.RollNo       ?? "-",
                    className   = ss.Class   != null ? ss.Class.ClassName     : "-",
                    sectionName = ss.Section != null ? ss.Section.SectionName : "-",
                    sessionId   = ss.SessionId,
                    sessionName = ss.Session != null ? ss.Session.SessionName : "-"
                }).ToListAsync();

            return Json(new { results = data, pagination = new { more = (page * pageSize) < total } });
        }

        // GET: FeeCollection/GetStudentFeeDetail
        [HttpGet]
        public async Task<IActionResult> GetStudentFeeDetail(int studentId, int sessionId = 0)
        {
            var query = _context.TblFeeCollections
                .Where(f => f.StudentId == studentId && f.IsActive == true);

            if (sessionId > 0)
                query = query.Where(f => f.SessionId == sessionId);

            var collections = await query
                .Include(f => f.Session)
                .Include(f => f.TblFeeCollectionDetails).ThenInclude(d => d.FeeType)
                .OrderBy(f => f.Year).ThenBy(f => f.Month)
                .Select(f => new {
                    f.FeeCollectionId,
                    SessionName = f.Session != null ? f.Session.SessionName : "-",
                    MonthYear   = f.Month != null && f.Year != null
                                  ? System.Globalization.CultureInfo.CurrentCulture
                                      .DateTimeFormat.GetMonthName(f.Month.Value) + " " + f.Year
                                  : "-",
                    f.Month, f.Year,
                    TotalAmount    = f.TotalAmount    ?? 0,
                    PaidAmount     = f.PaidAmount     ?? 0,
                    DueAmount      = (f.TotalAmount ?? 0) - (f.PaidAmount ?? 0),
                    DiscountAmount = f.DiscountAmount ?? 0,
                    FineAmount     = f.FineAmount     ?? 0,
                    PaymentMode    = f.PaymentMode    ?? "-",
                    PaymentDate    = f.PaymentDate.HasValue
                                     ? f.PaymentDate.Value.ToString("dd-MM-yyyy") : "-",
                    Details = f.TblFeeCollectionDetails
                        .Where(d => d.IsActive == true)
                        .Select(d => new {
                            FeeName = d.FeeType != null ? d.FeeType.FeeName : "-",
                            Amount  = d.Amount ?? 0
                        }).ToList()
                }).ToListAsync();

            var summary = new {
                totalFee    = collections.Sum(c => c.TotalAmount),
                totalPaid   = collections.Sum(c => c.PaidAmount),
                totalDue    = collections.Sum(c => c.DueAmount),
                totalMonths = collections.Count,
                monthsPaid  = collections.Count(c => c.DueAmount <= 0)
            };

            return Json(new { success = true, collections, summary });
        }
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var data = await _context.TblFeeCollections
                .Where(f => f.IsActive == true)
                .Include(f => f.Student)
                .Include(f => f.Session)
                .Select(f => new
                {
                    f.FeeCollectionId,
                    StudentName = f.Student != null ? f.Student.StudentName : "-",
                    RollNo = f.Student != null ? f.Student.RollNo : "-",
                    SessionName = f.Session != null ? f.Session.SessionName : "-",
                    MonthYear = f.Month != null && f.Year != null
                                    ? System.Globalization.CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(f.Month.Value) + " " + f.Year
                                    : "-",
                    f.Month,
                    f.Year,
                    TotalAmount = f.TotalAmount ?? 0,
                    PaidAmount = f.PaidAmount ?? 0,
                    DiscountAmount = f.DiscountAmount ?? 0,
                    FineAmount = f.FineAmount ?? 0,
                    DueAmount = (f.TotalAmount ?? 0) - (f.PaidAmount ?? 0),
                    f.PaymentMode,
                    PaymentDate = f.PaymentDate.HasValue ? f.PaymentDate.Value.ToString("dd-MM-yyyy") : "-",
                    Status = f.IsActive == true ? "Active" : "Inactive"
                })
                .ToListAsync();

            return Json(new { data });
        }

        // GET: FeeCollection/CreateOrEdit/5
        [HttpGet]
        public async Task<IActionResult> CreateOrEdit(int id = 0)
        {
            await LoadDropdowns();

            if (id == 0)
                return PartialView("_FeeCollectionModal", new TblFeeCollection
                {
                    IsActive = true,
                    PaymentDate = DateTime.Now,
                    Month = DateTime.Now.Month,
                    Year = DateTime.Now.Year
                });

            var record = await _context.TblFeeCollections
                .Include(f => f.TblFeeCollectionDetails)
                .FirstOrDefaultAsync(f => f.FeeCollectionId == id);

            if (record == null) return NotFound();

            // Edit mode: selected student naam
            if (record.StudentId.HasValue)
            {
                var s = await _context.TblStudents
                    .Where(x => x.StudentId == record.StudentId)
                    .Select(x => new { x.StudentId, DisplayName = x.StudentName + " | Roll: " + x.RollNo })
                    .FirstOrDefaultAsync();
                ViewBag.SelectedStudent = s;
            }

            return PartialView("_FeeCollectionModal", record);
        }

        // POST: FeeCollection/CreateOrEdit
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateOrEdit(int id, TblFeeCollection model,
            List<int> FeeTypeIds, List<decimal> FeeAmounts)
        {
            try
            {
                if (id == 0)
                {
                    model.CreatedDate = DateTime.Now;
                    model.CreatedBy = 1;
                    model.IsActive = true;
                    _context.TblFeeCollections.Add(model);
                    await _context.SaveChangesAsync();

                    // Save fee collection details
                    await SaveDetails(model.FeeCollectionId, FeeTypeIds, FeeAmounts);

                    // Auto create transaction record
                    if (model.PaidAmount > 0)
                    {
                        _context.TblFeeTransactions.Add(new TblFeeTransaction
                        {
                            FeeCollectionId = model.FeeCollectionId,
                            Amount = model.PaidAmount,
                            PaymentMode = model.PaymentMode,
                            TransactionDate = model.PaymentDate ?? DateTime.Now,
                            ReferenceNo = "TXN-" + model.FeeCollectionId.ToString("D6"),
                            IsActive = true,
                            CreatedDate = DateTime.Now,
                            CreatedBy = 1
                        });
                        await _context.SaveChangesAsync();
                    }

                    // Update or create StudentDue
                    await UpdateStudentDue(model);

                    // Extra charges auto-mark paid
                    var unpaidExtras = await _context.TblStudentExtraCharges
                        .Where(e => e.StudentId == model.StudentId
                                 && e.SessionId == model.SessionId
                                 && e.IsPaid    == false
                                 && e.IsActive  == true)
                        .ToListAsync();
                    foreach (var extra in unpaidExtras)
                    {
                        extra.IsPaid      = true;
                        extra.UpdatedDate = DateTime.Now;
                        extra.UpdatedBy   = 1;
                    }
                    if (unpaidExtras.Any())
                        await _context.SaveChangesAsync();
                }
                else
                {
                    var existing = await _context.TblFeeCollections.FindAsync(id);
                    if (existing == null)
                        return Json(new { success = false, message = "Record not found!" });

                    existing.StudentId = model.StudentId;
                    existing.SessionId = model.SessionId;
                    existing.Month = model.Month;
                    existing.Year = model.Year;
                    existing.TotalAmount = model.TotalAmount;
                    existing.PaidAmount = model.PaidAmount;
                    existing.DiscountAmount = model.DiscountAmount;
                    existing.FineAmount = model.FineAmount;
                    existing.PaymentDate = model.PaymentDate;
                    existing.PaymentMode = model.PaymentMode;
                    existing.IsActive = model.IsActive;
                    existing.UpdatedDate = DateTime.Now;
                    existing.UpdatedBy = 1;
                    await _context.SaveChangesAsync();

                    // Update details
                    var oldDetails = _context.TblFeeCollectionDetails
                        .Where(d => d.FeeCollectionId == id);
                    _context.TblFeeCollectionDetails.RemoveRange(oldDetails);
                    await _context.SaveChangesAsync();
                    await SaveDetails(id, FeeTypeIds, FeeAmounts);

                    // Update StudentDue
                    existing.Student = await _context.TblStudents.FindAsync(existing.StudentId);
                    existing.Session = await _context.TblAcademicSessions.FindAsync(existing.SessionId);
                    await UpdateStudentDue(existing);
                }

                return Json(new { success = true, message = "Fee Collection saved successfully!" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error: " + ex.Message });
            }
        }

        // POST: FeeCollection/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var record = await _context.TblFeeCollections.FindAsync(id);
            if (record == null)
                return Json(new { success = false, message = "Record not found!" });

            record.IsActive = false;
            await _context.SaveChangesAsync();

            return Json(new { success = true, message = "Fee Collection deleted successfully!" });
        }

        // GET: FeeCollection/View/5
        [HttpGet]
        public async Task<IActionResult> View(int id)
        {
            var record = await _context.TblFeeCollections
                .Include(f => f.Student)
                .Include(f => f.Session)
                .Include(f => f.TblFeeCollectionDetails).ThenInclude(d => d.FeeType)
                .Include(f => f.TblFeeTransactions)
                .FirstOrDefaultAsync(f => f.FeeCollectionId == id);

            if (record == null) return NotFound();

            return PartialView("_FeeCollectionViewModal", record);
        }

        // GET: FeeCollection/GetStudentFeeInfo - AJAX: load fee structure for student
        [HttpGet]
        public async Task<IActionResult> GetStudentFeeInfo(int studentId, int sessionId)
        {
            // Get student's class from StudentSession
            var studentSession = await _context.TblStudentSessions
                .Include(s => s.Class)
                .FirstOrDefaultAsync(s => s.StudentId == studentId
                                       && s.SessionId == sessionId
                                       && s.IsActive == true);

            if (studentSession == null)
                return Json(new { success = false, message = "Student is not mapped to selected session!" });

            // ── 1. Mandatory fees: FeeStructure (class level) — sirf Regular category ──
            var feeStructures = await _context.TblFeeStructures
                .Where(f => f.SessionId == sessionId
                         && f.ClassId  == studentSession.ClassId
                         && f.IsActive == true
                         && (f.FeeType == null || f.FeeType.FeeCategory != "Optional"))
                .Include(f => f.FeeType)
                .Select(f => new
                {
                    f.FeeTypeId,
                    FeeName     = f.FeeType != null ? f.FeeType.FeeName : "-",
                    IsRecurring = f.FeeType != null && f.FeeType.IsRecurring == true,
                    f.Amount
                })
                .ToListAsync();

            // ── 2. Student-specific overrides ────────────────────────────────
            var overrides = await _context.TblStudentFeeOverrides
                .Where(o => o.StudentId == studentId && o.IsActive == true)
                .ToDictionaryAsync(o => o.FeeTypeId ?? 0, o => o.Amount);

            // ── 3. Already collected One-Time fee type IDs ────────────────────
            // One-Time fees jo kisi bhi month mein already collect ho chuki hain
            var alreadyCollectedOneTimeFeeTypeIds = await _context.TblFeeCollectionDetails
                .Where(d => d.IsActive == true
                         && d.FeeCollection != null
                         && d.FeeCollection.StudentId == studentId
                         && d.FeeCollection.SessionId == sessionId
                         && d.FeeCollection.IsActive  == true
                         && d.FeeType != null
                         && d.FeeType.IsRecurring     == false)  // sirf one-time
                .Select(d => d.FeeTypeId ?? 0)
                .Distinct()
                .ToListAsync();

            // Apply overrides + filter out already-paid one-time fees
            var finalFees = feeStructures
                .Where(f => f.IsRecurring == true
                         || !alreadyCollectedOneTimeFeeTypeIds.Contains(f.FeeTypeId ?? 0))
                .Select(f => new
                {
                    f.FeeTypeId,
                    f.FeeName,
                    f.IsRecurring,
                    Amount       = overrides.ContainsKey(f.FeeTypeId ?? 0)
                                       ? overrides[f.FeeTypeId ?? 0]
                                       : f.Amount,
                    IsOverridden = overrides.ContainsKey(f.FeeTypeId ?? 0),
                    FeeCategory  = "Mandatory",
                    Remarks      = (string?)null
                }).ToList<dynamic>();

            // ── 4. Optional fees ──────────────────────────────────────────────
            var optionalFees = await _context.TblStudentOptionalFees
                .Where(o => o.StudentId == studentId
                         && o.SessionId == sessionId
                         && o.IsActive)
                .Include(o => o.FeeType)
                .ToListAsync();

            foreach (var opt in optionalFees)
            {
                bool isRecurring = opt.FeeType?.IsRecurring == true;

                // Non-recurring optional fee already collected? Skip karo
                if (!isRecurring && alreadyCollectedOneTimeFeeTypeIds.Contains(opt.FeeTypeId))
                    continue;

                finalFees.Add(new
                {
                    FeeTypeId    = (int?)opt.FeeTypeId,
                    FeeName      = opt.FeeType?.FeeName ?? "-",
                    IsRecurring  = isRecurring,
                    Amount       = (decimal?)opt.Amount,
                    IsOverridden = false,
                    FeeCategory  = "Optional",
                    Remarks      = opt.Remarks
                });
            }

            // ── 5. Unpaid extra charges ───────────────────────────────────────
            var extraCharges = await _context.TblStudentExtraCharges
                .Where(e => e.StudentId == studentId
                         && e.SessionId == sessionId
                         && e.IsPaid    == false
                         && e.IsActive  == true)
                .Include(e => e.FeeType)
                .Select(e => new
                {
                    e.Id,
                    FeeName = e.FeeType != null ? e.FeeType.FeeName : "Extra",
                    e.Amount,
                    e.Reason
                })
                .ToListAsync();

            var totalAmount = finalFees.Sum(f => (decimal?)(f.Amount) ?? 0m)
                            + extraCharges.Sum(e => e.Amount ?? 0m);

            return Json(new
            {
                success      = true,
                className    = studentSession.Class?.ClassName ?? "-",
                fees         = finalFees,
                extraCharges = extraCharges,
                totalAmount  = totalAmount
            });
        }

        // GET: FeeCollection/GetMonthsStatus - AJAX: konse months paid/unpaid hain
        [HttpGet]
        public async Task<IActionResult> GetMonthsStatus(int studentId, int sessionId)
        {
            var paid = await _context.TblFeeCollections
                .Where(f => f.StudentId == studentId
                         && f.SessionId == sessionId
                         && f.IsActive  == true)
                .Select(f => new { f.Month, f.Year,
                    IsFull = (f.TotalAmount ?? 0) <= (f.PaidAmount ?? 0) })
                .ToListAsync();

            return Json(new { success = true, paidMonths = paid });
        }

        // GET: FeeCollection/GetCollectionsByStudent
        [HttpGet]
        public async Task<IActionResult> GetCollectionsByStudent(int studentId, int sessionId)
        {
            var collections = await _context.TblFeeCollections
                .Where(f => f.StudentId == studentId
                         && f.SessionId == sessionId
                         && f.IsActive == true)
                .Select(f => new
                {
                    f.FeeCollectionId,
                    MonthYear = System.Globalization.CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(f.Month ?? 1) + " " + f.Year,
                    f.TotalAmount,
                    f.PaidAmount,
                    DueAmount = (f.TotalAmount ?? 0) - (f.PaidAmount ?? 0),
                    f.PaymentMode,
                    PaymentDate = f.PaymentDate.HasValue ? f.PaymentDate.Value.ToString("dd-MM-yyyy") : "-"
                })
                .OrderByDescending(f => f.FeeCollectionId)
                .ToListAsync();

            return Json(new { success = true, collections });
        }

        // POST: FeeCollection/CollectMultiMonth — Multiple months ek saath collect karo
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CollectMultiMonth(
            int StudentId, int SessionId,
            List<int> SelectedMonths, List<int> SelectedYears,
            decimal PerMonthAmount, decimal DiscountAmount, decimal FineAmount,
            decimal TotalPaidAmount, string PaymentMode, DateTime PaymentDate,
            List<int> FeeTypeIds, List<decimal> FeeAmounts)
        {
            try
            {
                if (SelectedMonths == null || SelectedMonths.Count == 0)
                    return Json(new { success = false, message = "Koi bhi month select nahi kiya!" });

                if (string.IsNullOrEmpty(PaymentMode))
                    return Json(new { success = false, message = "Payment mode select karo!" });

                int totalMonths   = SelectedMonths.Count;
                decimal perMonth  = PerMonthAmount;

                // Total paid distribute karo months mein proportionally
                decimal remaining = TotalPaidAmount;
                var savedIds      = new List<int>();

                for (int i = 0; i < totalMonths; i++)
                {
                    int month = SelectedMonths[i];
                    int year  = SelectedYears[i];

                    // Already exists? Skip silently (idempotent)
                    bool alreadyExists = await _context.TblFeeCollections
                        .AnyAsync(f => f.StudentId == StudentId
                                    && f.SessionId == SessionId
                                    && f.Month     == month
                                    && f.Year      == year
                                    && f.IsActive  == true);
                    if (alreadyExists) continue;

                    // Is month ke liye kitna paid
                    decimal thisDiscount = i == 0 ? DiscountAmount : 0; // discount first month pe
                    decimal thisFine     = i == 0 ? FineAmount     : 0; // fine first month pe
                    decimal netForMonth  = perMonth - thisDiscount + thisFine;
                    decimal paidThisMonth = remaining >= netForMonth ? netForMonth : remaining;
                    if (paidThisMonth < 0) paidThisMonth = 0;
                    remaining -= paidThisMonth;
                    if (remaining < 0) remaining = 0;

                    var collection = new TblFeeCollection
                    {
                        StudentId      = StudentId,
                        SessionId      = SessionId,
                        Month          = month,
                        Year           = year,
                        TotalAmount    = netForMonth,
                        PaidAmount     = paidThisMonth,
                        DiscountAmount = thisDiscount,
                        FineAmount     = thisFine,
                        PaymentDate    = PaymentDate,
                        PaymentMode    = PaymentMode,
                        IsActive       = true,
                        CreatedDate    = DateTime.Now,
                        CreatedBy      = 1
                    };

                    _context.TblFeeCollections.Add(collection);
                    await _context.SaveChangesAsync();

                    // Fee details save karo
                    await SaveDetails(collection.FeeCollectionId, FeeTypeIds, FeeAmounts);

                    // Transaction record
                    if (paidThisMonth > 0)
                    {
                        _context.TblFeeTransactions.Add(new TblFeeTransaction
                        {
                            FeeCollectionId = collection.FeeCollectionId,
                            Amount          = paidThisMonth,
                            PaymentMode     = PaymentMode,
                            TransactionDate = PaymentDate,
                            ReferenceNo     = "TXN-" + collection.FeeCollectionId.ToString("D6"),
                            IsActive        = true,
                            CreatedDate     = DateTime.Now,
                            CreatedBy       = 1
                        });
                    }

                    await UpdateStudentDue(collection);
                    savedIds.Add(collection.FeeCollectionId);
                }

                await _context.SaveChangesAsync();

                // ── Extra charges auto-mark paid ──────────────────────────────
                // Collection ho gayi — iss student+session ke unpaid extra charges paid mark karo
                if (savedIds.Count > 0)
                {
                    var unpaidExtras = await _context.TblStudentExtraCharges
                        .Where(e => e.StudentId == StudentId
                                 && e.SessionId == SessionId
                                 && e.IsPaid    == false
                                 && e.IsActive  == true)
                        .ToListAsync();

                    foreach (var extra in unpaidExtras)
                    {
                        extra.IsPaid      = true;
                        extra.UpdatedDate = DateTime.Now;
                        extra.UpdatedBy   = 1;
                    }

                    if (unpaidExtras.Any())
                        await _context.SaveChangesAsync();
                }

                return Json(new
                {
                    success = true,
                    message = $"{savedIds.Count} month(s) ki fee successfully collected!",
                    count   = savedIds.Count
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error: " + ex.Message });
            }
        }

        // ── Private Helpers ───────────────────────────────────────────────────
        private async Task SaveDetails(int collectionId, List<int> feeTypeIds, List<decimal> amounts)
        {
            for (int i = 0; i < feeTypeIds.Count; i++)
            {
                if (feeTypeIds[i] > 0 && amounts[i] > 0)
                {
                    _context.TblFeeCollectionDetails.Add(new TblFeeCollectionDetail
                    {
                        FeeCollectionId = collectionId,
                        FeeTypeId = feeTypeIds[i],
                        Amount = amounts[i],
                        IsActive = true,
                        CreatedDate = DateTime.Now,
                        CreatedBy = 1
                    });
                }
            }
            await _context.SaveChangesAsync();
        }

        private async Task UpdateStudentDue(TblFeeCollection model)
        {
            var due = await _context.TblStudentDues
                .FirstOrDefaultAsync(d => d.StudentId == model.StudentId
                                       && d.SessionId == model.SessionId
                                       && d.Month == model.Month
                                       && d.Year == model.Year
                                       && d.IsActive == true);

            var dueAmount = (model.TotalAmount ?? 0) - (model.PaidAmount ?? 0);

            if (due == null)
            {
                _context.TblStudentDues.Add(new TblStudentDue
                {
                    StudentId = model.StudentId,
                    SessionId = model.SessionId,
                    Month = model.Month,
                    Year = model.Year,
                    TotalDue = model.TotalAmount,
                    PaidAmount = model.PaidAmount,
                    IsSettled = dueAmount <= 0,
                    SettledDate = dueAmount <= 0 ? DateTime.Now : null,
                    IsActive = true,
                    CreatedDate = DateTime.Now,
                    CreatedBy = 1
                });
            }
            else
            {
                due.TotalDue = model.TotalAmount;
                due.PaidAmount = model.PaidAmount;
                due.IsSettled = dueAmount <= 0;
                due.SettledDate = dueAmount <= 0 ? DateTime.Now : null;
                due.UpdatedDate = DateTime.Now;
                due.UpdatedBy = 1;
            }

            await _context.SaveChangesAsync();
        }

        private async Task LoadDropdowns()
        {
            // Students ab Select2 AJAX se load honge — yahan nahi chahiye

            ViewBag.Sessions = await _context.TblAcademicSessions
                .Where(s => s.IsActive == true)
                .OrderByDescending(s => s.SessionId)
                .Select(s => new { s.SessionId, s.SessionName })
                .ToListAsync();

            ViewBag.FeeTypes = await _context.TblFeeTypes
                .Where(f => f.IsActive == true && f.FeeCategory != "Optional")
                .OrderBy(f => f.FeeName)
                .Select(f => new { f.FeeTypeId, f.FeeName })
                .ToListAsync();

            ViewBag.Months = Enumerable.Range(1, 12).Select(m => new
            {
                Value = m,
                Text = System.Globalization.CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(m)
            }).ToList();

            ViewBag.Years = Enumerable.Range(DateTime.Now.Year - 2, 5).Select(y => new
            {
                Value = y,
                Text = y.ToString()
            }).ToList();

            ViewBag.PaymentModes = new[]
            {
                new { Value = "Cash",          Text = "Cash"           },
                new { Value = "Online",        Text = "Online"         },
                new { Value = "Cheque",        Text = "Cheque"         },
                new { Value = "Bank Transfer", Text = "Bank Transfer"  },
                new { Value = "UPI",           Text = "UPI"            }
            };
        }
    }
}
