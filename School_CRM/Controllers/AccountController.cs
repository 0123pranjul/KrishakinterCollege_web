using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using School_CRM.Models;
using School_CRM.Service;
using School_CRM.Services;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace School_CRM.Controllers
{
    public class AccountController : Controller
    {
        private readonly IConfiguration _config;
        private readonly LibmanagementContext _context;
        private readonly TokenService _tokenService;
        private readonly EmailService _emailService;
        private readonly ILogger<AccountController> _logger;

        public AccountController(
            LibmanagementContext context,
            TokenService tokenService,
            EmailService emailService,
            IConfiguration config,
            ILogger<AccountController> logger)
        {
            _context      = context;
            _tokenService = tokenService;
            _emailService = emailService;
            _config       = config;
            _logger       = logger;
        }

        // ─────────────────────────────────────────────
        //  GET: /Account/Login
        // ─────────────────────────────────────────────
        [AllowAnonymous]
        public IActionResult Login()
        {
            // Rate limit exceed hone par redirect yahan aata hai — query param check karo
            if (Request.Query.ContainsKey("rateLimited"))
            {
                ModelState.AddModelError("",
                    "Too many login attempts. Please wait 1 minute and try again.");
            }

            return View();
        }

        [AllowAnonymous]
        [HttpGet]
        public IActionResult GetCaptchaImage()
        {
            var code = GenerateRandomCaptchaString(5);
            TempData["CaptchaCode"] = code;
            var svg = GenerateCaptchaSvg(code);
            return Content(svg, "image/svg+xml");
        }

        private string GenerateRandomCaptchaString(int length)
        {
            const string chars = "ABCDEFGHJKLMNOPQRSTUVWXYZ23456789"; // No easily confused characters
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

        // ─────────────────────────────────────────────
        //  POST: /Account/Login
        // ─────────────────────────────────────────────
        [AllowAnonymous]
        [HttpPost]
        [ValidateAntiForgeryToken]
        [EnableRateLimiting("LoginPolicy")]   // 🚦 Max 5 attempts per IP per minute
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";

            try
            {
                // Verify Captcha Code
                var sessionCaptcha = TempData["CaptchaCode"] as string;
                if (string.IsNullOrEmpty(sessionCaptcha) || string.IsNullOrEmpty(model.CaptchaInput) || model.CaptchaInput.Trim().ToUpper() != sessionCaptcha.ToUpper())
                {
                    ModelState.AddModelError("", "Incorrect Captcha code. Please try again.");
                    return View(model);
                }

                if (!ModelState.IsValid)
                    return View(model);

                if (string.IsNullOrWhiteSpace(model.Username) || string.IsNullOrWhiteSpace(model.Password))
                {
                    ModelState.AddModelError("", "Username and Password are required.");
                    return View(model);
                }

                // 1. Verify credentials
                string encryptedPassword = Password.Encrypt(model.Password.Trim());
                var user = await _context.UserMasters
                    .Where(u => u.Username == model.Username.Trim()
                             && u.PasswordHash == encryptedPassword
                             && u.IsActive == true)
                    .FirstOrDefaultAsync();

                if (user == null)
                {
                    // ⚠️ Failed login — wrong username or password
                    _logger.LogWarning(
                        "FAILED_LOGIN | Username: {Username} | IP: {IP} | Reason: Invalid credentials | Time: {Time}",
                        model.Username.Trim(), ipAddress, DateTime.Now);

                    ModelState.AddModelError("", "Invalid username or password.");
                    return View(model);
                }

                // 2. Get role
                var roleAssign = await _context.UserRoleAssigns
                    .Include(x => x.Role)
                    .Where(x => x.UserId == user.UserId && x.IsActive == true)
                    .FirstOrDefaultAsync();

                if (roleAssign == null)
                {
                    // ⚠️ User exists but no role assigned
                    _logger.LogWarning(
                        "FAILED_LOGIN | Username: {Username} | UserId: {UserId} | IP: {IP} | Reason: No role assigned | Time: {Time}",
                        user.Username, user.UserId, ipAddress, DateTime.Now);

                    ModelState.AddModelError("", "No role assigned to this account. Contact administrator.");
                    return View(model);
                }

                // ── DEVELOPMENT MODE: OTP step skip karo ────────────────────────
#if DEBUG
                // DEBUG build mein seedha login ho jaata hai — OTP nahi aayegi
                // PRODUCTION mein automatically OTP flow chalega (kuch change nahi karna)
                _logger.LogWarning(
                    "DEV_LOGIN_SKIP_OTP | Username: {Username} | UserId: {UserId} | IP: {IP} | Time: {Time}",
                    user.Username, user.UserId, ipAddress, DateTime.Now);

                string roleName_dev = roleAssign.Role.RoleName;
                int    roleId_dev   = roleAssign.RoleId;

                var accessToken_dev  = _tokenService.GenerateAccessToken(user, roleName_dev, roleId_dev);
                var refreshToken_dev = _tokenService.GenerateRefreshToken();

                _context.RefreshTokens.Add(new RefreshToken
                {
                    UserId    = user.UserId,
                    Token     = refreshToken_dev,
                    ExpiresAt = _tokenService.GetRefreshTokenExpiry()
                });
                await _context.SaveChangesAsync();

                var persistOpts_dev = new CookieOptions
                {
                    HttpOnly    = true,
                    Secure      = false,           // dev mein HTTP bhi chalega
                    SameSite    = SameSiteMode.Lax,
                    IsEssential = true,
                    Expires     = DateTimeOffset.Now.AddDays(7)
                };
                var secureOpts_dev = new CookieOptions
                {
                    HttpOnly = true, Secure = false, SameSite = SameSiteMode.Lax
                };

                Response.Cookies.Append("AccessToken",  accessToken_dev,            secureOpts_dev);
                Response.Cookies.Append("RefreshToken", refreshToken_dev,           secureOpts_dev);
                Response.Cookies.Append("roleId",       roleId_dev.ToString(),      persistOpts_dev);
                Response.Cookies.Append("roleName",     roleName_dev,               persistOpts_dev);
                Response.Cookies.Append("userId",       user.UserId.ToString(),     persistOpts_dev);
                Response.Cookies.Append("userName",     user.Username,              persistOpts_dev);

                await ResolveRoleIdentity(user, roleName_dev, persistOpts_dev);

                return RedirectToRoleLanding(roleName_dev);
#else
                // ── PRODUCTION MODE: OTP flow ────────────────────────────────

                // 3. Verify user has email
                if (string.IsNullOrWhiteSpace(user.Email))
                {
                    _logger.LogWarning(
                        "FAILED_LOGIN | Username: {Username} | UserId: {UserId} | IP: {IP} | Reason: Email not registered | Time: {Time}",
                        user.Username, user.UserId, ipAddress, DateTime.Now);

                    ModelState.AddModelError("", "No email address registered for this user. Please contact administrator.");
                    return View(model);
                }

                // 4. Generate random 6-digit OTP & Verification Token
                string otpCode = Random.Shared.Next(100000, 999999).ToString();
                string verificationToken = Guid.NewGuid().ToString("N");

                var otpRecord = new UserOtp
                {
                    UserId = user.UserId,
                    OtpCode = otpCode,
                    VerificationToken = verificationToken,
                    CreatedDateTime = DateTime.Now,
                    ExpiryDateTime = DateTime.Now.AddMinutes(3), // 3-minute validity
                    IsVerified = false,
                    IpAddress = ipAddress
                };

                _context.UserOtps.Add(otpRecord);
                await _context.SaveChangesAsync();

                // 5. Send OTP to User's Email
                try
                {
                    await _emailService.SendOtpEmailAsync(user.Email, user.Username, otpCode, 3);
                }
                catch (Exception emailEx)
                {
                    _logger.LogError(emailEx, "EMAIL_SEND_FAILED | UserId: {UserId} | Email: {Email} | Time: {Time}",
                        user.UserId, user.Email, DateTime.Now);

                    ModelState.AddModelError("", "Failed to send verification email. Please verify your SMTP config.");
                    return View(model);
                }

                // 6. Redirect to OTP entry view
                return RedirectToAction("VerifyOtp", new { token = verificationToken });
#endif
            }
            catch (Exception ex)
            {
                // 🔴 Unexpected exception during login
                _logger.LogError(ex,
                    "LOGIN_ERROR | Username: {Username} | IP: {IP} | Time: {Time}",
                    model.Username?.Trim() ?? "unknown", ipAddress, DateTime.Now);

                ModelState.AddModelError("", "An unexpected error occurred. Please try again.");
                return View(model);
            }
        }

        // ─────────────────────────────────────────────
        //  Resolve linked entity by role
        //
        //  SuperAdmin  → UserMaster only, no linked table
        //  Student     → UserMaster.StudentId → Tbl_Student
        //  Teacher     → UserMaster.EmpId → Employees (for EmployeeId)
        //              → UserMaster.TeacherId → Tbl_Teacher (for profile)
        //  Principal   → same as Teacher (EmpId + TeacherId)
        //  Employee    → UserMaster.EmpId → Employees
        //  Admin       → UserMaster.EmpId → Employees
        // ─────────────────────────────────────────────
        private async Task ResolveRoleIdentity(UserMaster user, string roleName, CookieOptions opts)
        {
            try
            {
                switch (roleName.ToLower())
                {
                    // ── SuperAdmin: UserMaster only, no linked table ──
                    case "superadmin":
                        Response.Cookies.Append("EntityId",   user.UserId.ToString(), opts);
                        Response.Cookies.Append("EntityName", user.Username,          opts);
                        Response.Cookies.Append("EntityCode", "SUPERADMIN",           opts);
                        Response.Cookies.Append("IsAdmin",    "true",                 opts);
                        break;

                    // ── Student: UserMaster.StudentId → Tbl_Student ──
                    case "student":
                        if (user.StudentId.HasValue && user.StudentId > 0)
                        {
                            var student = await _context.TblStudents
                                .Where(s => s.StudentId == user.StudentId && s.IsActive == true)
                                .Select(s => new { s.StudentId, s.StudentName, s.AdmissionNo, s.RollNo })
                                .FirstOrDefaultAsync();

                            if (student != null)
                            {
                                Response.Cookies.Append("EntityId",   student.StudentId.ToString(),        opts);
                                Response.Cookies.Append("EntityName", student.StudentName ?? user.Username, opts);
                                Response.Cookies.Append("EntityCode", student.AdmissionNo ?? "",            opts);
                                Response.Cookies.Append("RollNo",     student.RollNo ?? "",                 opts);
                                Response.Cookies.Append("IsAdmin",    "false",                              opts);
                                break;
                            }
                        }
                        SetDefaultEntityCookies(user, opts, isAdmin: false);
                        break;

                    // ── Teacher / Principal: EmpId → Employees + TeacherId → Tbl_Teacher ──
                    case "teacher":
                    case "principal":
                        // Step 1: get Employee record for EmployeeId / EmployeeCode
                        string empCode = "";
                        int    empId   = 0;
                        string empName = user.Username;

                        if (user.EmpId.HasValue && user.EmpId > 0)
                        {
                            var emp = await _context.Employees
                                .Where(e => e.Id == user.EmpId && e.IsActive == true)
                                .Select(e => new { e.Id, e.Name, e.EmployeeCode, e.Designation })
                                .FirstOrDefaultAsync();

                            if (emp != null)
                            {
                                empId   = emp.Id;
                                empCode = emp.EmployeeCode ?? "";
                                empName = emp.Name ?? user.Username;
                                Response.Cookies.Append("EmployeeId",   empId.ToString(), opts);
                                Response.Cookies.Append("EmployeeCode", empCode,          opts);
                                Response.Cookies.Append("Designation",  emp.Designation ?? "", opts);
                            }
                        }

                        // Step 2: get Teacher profile for TeacherId
                        if (user.TeacherId.HasValue && user.TeacherId > 0)
                        {
                            var teacher = await _context.TblTeachers
                                .Where(t => t.TeacherId == user.TeacherId && t.IsActive == true)
                                .Select(t => new { t.TeacherId, t.TeacherName, t.Email, t.Designation, t.MobileNo })
                                .FirstOrDefaultAsync();

                            if (teacher != null)
                            {
                                Response.Cookies.Append("EntityId",      teacher.TeacherId.ToString(),        opts);
                                Response.Cookies.Append("EntityName",    teacher.TeacherName ?? empName,      opts);
                                Response.Cookies.Append("EntityCode",    empCode,                             opts);
                                Response.Cookies.Append("TeacherEmail",  teacher.Email ?? "",                 opts);
                                Response.Cookies.Append("TeacherMobile", teacher.MobileNo ?? "",              opts);
                                Response.Cookies.Append("IsAdmin",       "false",                             opts);
                                break;
                            }
                        }

                        // Fallback: use Employee data if no Teacher record
                        Response.Cookies.Append("EntityId",   empId.ToString(), opts);
                        Response.Cookies.Append("EntityName", empName,          opts);
                        Response.Cookies.Append("EntityCode", empCode,          opts);
                        Response.Cookies.Append("IsAdmin",    "false",          opts);
                        break;

                    // ── Employee / Admin: UserMaster.EmpId → Employees ──
                    case "employee":
                    case "admin":
                    default:
                        if (user.EmpId.HasValue && user.EmpId > 0)
                        {
                            var emp = await _context.Employees
                                .Where(e => e.Id == user.EmpId && e.IsActive == true)
                                .Select(e => new { e.Id, e.Name, e.EmployeeCode, e.Designation })
                                .FirstOrDefaultAsync();

                            if (emp != null)
                            {
                                Response.Cookies.Append("EntityId",   emp.Id.ToString(),          opts);
                                Response.Cookies.Append("EntityName", emp.Name ?? user.Username,  opts);
                                Response.Cookies.Append("EntityCode", emp.EmployeeCode ?? "",     opts);
                                Response.Cookies.Append("EmployeeId",   emp.Id.ToString(),          opts);
                                Response.Cookies.Append("EmployeeCode", emp.EmployeeCode ?? "",     opts);
                                Response.Cookies.Append("Designation", emp.Designation ?? "",     opts);
                                bool isAdminRole = roleName.Equals("Admin", StringComparison.OrdinalIgnoreCase);
                                Response.Cookies.Append("IsAdmin", isAdminRole.ToString().ToLower(), opts);
                                break;
                            }
                        }
                        SetDefaultEntityCookies(user, opts, isAdmin: false);
                        break;
                }
            }
            catch
            {
                // Non-critical — never fail login due to entity lookup
                SetDefaultEntityCookies(user, opts, isAdmin: false);
            }
        }

        private void SetDefaultEntityCookies(UserMaster user, CookieOptions opts, bool isAdmin = false)
        {
            Response.Cookies.Append("EntityId",   user.UserId.ToString(), opts);
            Response.Cookies.Append("EntityName", user.Username,          opts);
            Response.Cookies.Append("EntityCode", "",                     opts);
            Response.Cookies.Append("IsAdmin",    isAdmin.ToString().ToLower(), opts);
        }

        private IActionResult RedirectToRoleLanding(string roleName) =>
            roleName.ToLower() switch
            {
                "superadmin" or "admin" or "principal" => RedirectToAction("SecurePage", "Account"),
                "teacher"  => RedirectToAction("Index", "EmployeeDashboard"),
                "student"  => RedirectToAction("Index", "ReportCard"),
                "employee" => RedirectToAction("Index", "Dashboard"),
                _          => RedirectToAction("Index", "Home")
            };

        // ─────────────────────────────────────────────
        //  GET: /Account/VerifyOtp
        // ─────────────────────────────────────────────
        [AllowAnonymous]
        public async Task<IActionResult> VerifyOtp(string token)
        {
            if (string.IsNullOrEmpty(token))
            {
                return RedirectToAction("Login");
            }

            var otpRecord = await _context.UserOtps
                .Include(o => o.User)
                .Where(o => o.VerificationToken == token && o.IsVerified == false)
                .OrderByDescending(o => o.CreatedDateTime)
                .FirstOrDefaultAsync();

            if (otpRecord == null)
            {
                TempData["ErrorMessage"] = "Invalid or expired verification session.";
                return RedirectToAction("Login");
            }

            // Check if verification session itself is older than 15 minutes (failsafe)
            if (DateTime.Now > otpRecord.CreatedDateTime.AddMinutes(15))
            {
                TempData["ErrorMessage"] = "Session expired. Please log in again.";
                return RedirectToAction("Login");
            }

            ViewBag.Token = token;
            ViewBag.MaskedEmail = MaskEmail(otpRecord.User.Email);
            // Calculate remaining seconds for the UI countdown timer
            var remainingSeconds = (otpRecord.ExpiryDateTime - DateTime.Now).TotalSeconds;
            ViewBag.RemainingSeconds = remainingSeconds > 0 ? (int)remainingSeconds : 0;

            return View();
        }

        // Helper to mask email address: pr***@gmail.com
        private string MaskEmail(string email)
        {
            if (string.IsNullOrEmpty(email)) return string.Empty;
            int atIndex = email.IndexOf('@');
            if (atIndex <= 2) return email;
            
            string name = email.Substring(0, atIndex);
            string domain = email.Substring(atIndex);
            
            return name.Substring(0, 2) + new string('*', Math.Max(3, name.Length - 2)) + domain;
        }

        // ─────────────────────────────────────────────
        //  POST: /Account/VerifyOtp
        // ─────────────────────────────────────────────
        [AllowAnonymous]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> VerifyOtp(string token, string otp)
        {
            var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";

            if (string.IsNullOrEmpty(token) || string.IsNullOrEmpty(otp))
            {
                ModelState.AddModelError("", "Verification token and OTP are required.");
                ViewBag.Token = token;
                return View();
            }

            var otpRecord = await _context.UserOtps
                .Include(o => o.User)
                .Where(o => o.VerificationToken == token && o.IsVerified == false)
                .OrderByDescending(o => o.CreatedDateTime)
                .FirstOrDefaultAsync();

            if (otpRecord == null)
            {
                ModelState.AddModelError("", "Invalid or expired verification session.");
                ViewBag.Token = token;
                return View();
            }

            // Check if OTP is expired
            if (DateTime.Now > otpRecord.ExpiryDateTime)
            {
                ModelState.AddModelError("", "OTP has expired. Please request a new one.");
                ViewBag.Token = token;
                ViewBag.MaskedEmail = MaskEmail(otpRecord.User.Email);
                ViewBag.RemainingSeconds = 0;
                return View();
            }

            // Verify code
            if (otpRecord.OtpCode != otp.Trim())
            {
                ModelState.AddModelError("", "Incorrect verification code. Please try again.");
                ViewBag.Token = token;
                ViewBag.MaskedEmail = MaskEmail(otpRecord.User.Email);
                var remainingSec = (otpRecord.ExpiryDateTime - DateTime.Now).TotalSeconds;
                ViewBag.RemainingSeconds = remainingSec > 0 ? (int)remainingSec : 0;
                return View();
            }

            // Success: Verify OTP record
            otpRecord.IsVerified = true;
            await _context.SaveChangesAsync();

            // Fetch user role info to write authentication cookies
            var user = otpRecord.User;
            var roleAssign = await _context.UserRoleAssigns
                .Include(x => x.Role)
                .Where(x => x.UserId == user.UserId && x.IsActive == true)
                .FirstOrDefaultAsync();

            if (roleAssign == null)
            {
                ModelState.AddModelError("", "No role assigned to this account. Contact administrator.");
                ViewBag.Token = token;
                return View();
            }

            string roleName = roleAssign.Role.RoleName;
            int roleId = roleAssign.RoleId;

            // Generate JWT + refresh token
            var accessToken  = _tokenService.GenerateAccessToken(user, roleName, roleId);
            var refreshToken = _tokenService.GenerateRefreshToken();

            _context.RefreshTokens.Add(new RefreshToken
            {
                UserId    = user.UserId,
                Token     = refreshToken,
                ExpiresAt = _tokenService.GetRefreshTokenExpiry()
            });
            await _context.SaveChangesAsync();

            // Cookie options
            var secureOpts = new CookieOptions
            {
                HttpOnly = true, Secure = true, SameSite = SameSiteMode.Strict
            };
            var persistOpts = new CookieOptions
            {
                HttpOnly = true, Secure = true, SameSite = SameSiteMode.Strict,
                IsEssential = true, Expires = DateTimeOffset.Now.AddDays(7)
            };

            // Write auth cookies
            Response.Cookies.Append("AccessToken",  accessToken,           secureOpts);
            Response.Cookies.Append("RefreshToken", refreshToken,          secureOpts);
            Response.Cookies.Append("roleId",       roleId.ToString(),     persistOpts);
            Response.Cookies.Append("roleName",     roleName,              persistOpts);
            Response.Cookies.Append("userId",       user.UserId.ToString(), persistOpts);
            Response.Cookies.Append("userName",     user.Username,         persistOpts);

            // Log success
            _logger.LogInformation(
                "LOGIN_SUCCESS_2FA | Username: {Username} | UserId: {UserId} | Role: {Role} | IP: {IP} | Time: {Time}",
                user.Username, user.UserId, roleName, ipAddress, DateTime.Now);

            // Resolve entity identity (Student / Teacher / Employee)
            await ResolveRoleIdentity(user, roleName, persistOpts);

            // Redirect
            return RedirectToRoleLanding(roleName);
        }

        // ─────────────────────────────────────────────
        //  POST: /Account/ResendOtp
        // ─────────────────────────────────────────────
        [AllowAnonymous]
        [HttpPost]
        public async Task<IActionResult> ResendOtp(string token)
        {
            var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";

            if (string.IsNullOrEmpty(token))
            {
                return Json(new { success = false, message = "Invalid token session." });
            }

            var otpRecord = await _context.UserOtps
                .Include(o => o.User)
                .Where(o => o.VerificationToken == token && o.IsVerified == false)
                .OrderByDescending(o => o.CreatedDateTime)
                .FirstOrDefaultAsync();

            if (otpRecord == null)
            {
                return Json(new { success = false, message = "Session not found or already verified." });
            }

            // Generate fresh OTP
            string newOtp = Random.Shared.Next(100000, 999999).ToString();
            
            // Update OTP record values
            otpRecord.OtpCode = newOtp;
            otpRecord.CreatedDateTime = DateTime.Now;
            otpRecord.ExpiryDateTime = DateTime.Now.AddMinutes(3); // Reset to 3 minutes
            otpRecord.IpAddress = ipAddress;

            await _context.SaveChangesAsync();

            // Send Email
            try
            {
                await _emailService.SendOtpEmailAsync(otpRecord.User.Email, otpRecord.User.Username, newOtp, 3);
                return Json(new { success = true, message = "OTP resent successfully.", expirySeconds = 180 });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "RESEND_OTP_EMAIL_ERROR | UserId: {UserId} | Time: {Time}",
                    otpRecord.UserId, DateTime.Now);
                return Json(new { success = false, message = "Failed to send email. Please try again." });
            }
        }

        // ─────────────────────────────────────────────
        //  POST: /Account/Logout
        // ─────────────────────────────────────────────
        [AllowAnonymous]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            var username = Request.Cookies["userName"] ?? "unknown";
            var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";

            try
            {
                var refreshToken = Request.Cookies["RefreshToken"];
                if (!string.IsNullOrEmpty(refreshToken))
                {
                    var token = await _context.RefreshTokens
                        .FirstOrDefaultAsync(t => t.Token == refreshToken);
                    if (token != null)
                    {
                        token.IsRevoked = true;
                        await _context.SaveChangesAsync();
                    }
                }
            }
            catch { /* non-critical */ }

            foreach (var cookie in Request.Cookies.Keys)
                Response.Cookies.Delete(cookie);

            _logger.LogInformation(
                "LOGOUT | Username: {Username} | IP: {IP} | Time: {Time}",
                username, ipAddress, DateTime.Now);

            return RedirectToAction("Index");
        }

        // ─────────────────────────────────────────────
        //  POST: /Account/Refresh  (called by middleware)
        // ─────────────────────────────────────────────
        [AllowAnonymous]
        [HttpPost]
        public async Task<IActionResult> Refresh()
        {
            try
            {
                var refreshToken = Request.Cookies["RefreshToken"];
                if (string.IsNullOrEmpty(refreshToken)) return Unauthorized();

                var storedToken = await _context.RefreshTokens
                    .FirstOrDefaultAsync(x => x.Token == refreshToken
                                           && x.IsRevoked == false
                                           && x.ExpiresAt > DateTime.Now);
                if (storedToken == null) return Unauthorized();

                var user = await _context.UserMasters.FindAsync(storedToken.UserId);
                if (user == null) return Unauthorized();

                var roleAssign = await _context.UserRoleAssigns
                    .Include(x => x.Role)
                    .Where(x => x.UserId == user.UserId && x.IsActive == true)
                    .FirstOrDefaultAsync();

                string roleName = roleAssign?.Role?.RoleName ?? "";
                int roleId      = roleAssign?.RoleId ?? -1;

                var newToken = _tokenService.GenerateAccessToken(user, roleName, roleId);
                Response.Cookies.Append("AccessToken", newToken,
                    new CookieOptions { HttpOnly = true, Secure = true, SameSite = SameSiteMode.Strict });

                return Ok();
            }
            catch { return Unauthorized(); }
        }

        // ─────────────────────────────────────────────
        //  Register  (Admin/SuperAdmin only)
        // ─────────────────────────────────────────────
        [Authorize(Roles = "SuperAdmin,Admin")]
        public IActionResult Register()
        {
            ViewBag.Roles = _context.RoleMasters.Where(x => x.IsActive == true).OrderBy(x => x.RoleName).ToList();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "SuperAdmin,Admin")]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            try
            {
                // Conditional manual validations
                if (model.CreateTeacher && string.IsNullOrWhiteSpace(model.TeacherName))
                {
                    ModelState.AddModelError("TeacherName", "Teacher Name is required when creating a teacher profile.");
                }

                if (model.CreateEmployee && string.IsNullOrWhiteSpace(model.EmployeeName))
                {
                    ModelState.AddModelError("EmployeeName", "Employee Name is required when creating an employee profile.");
                }

                if (!ModelState.IsValid)
                {
                    ViewBag.Roles = _context.RoleMasters.Where(x => x.IsActive == true).OrderBy(x => x.RoleName).ToList();
                    return View(model);
                }

                if (await _context.UserMasters.AnyAsync(x => x.Username == model.Username))
                {
                    ModelState.AddModelError("Username", "Username already exists.");
                    ViewBag.Roles = _context.RoleMasters.Where(x => x.IsActive == true).OrderBy(x => x.RoleName).ToList();
                    return View(model);
                }

                if (await _context.UserMasters.AnyAsync(x => x.Email == model.Email))
                {
                    ModelState.AddModelError("Email", "Email already registered.");
                    ViewBag.Roles = _context.RoleMasters.Where(x => x.IsActive == true).OrderBy(x => x.RoleName).ToList();
                    return View(model);
                }

                await using var transaction = await _context.Database.BeginTransactionAsync();

                int? teacherId = model.TeacherId;
                if (model.CreateTeacher)
                {
                    var teacher = new TblTeacher
                    {
                        TeacherName = model.TeacherName!.Trim(),
                        MobileNo = model.TeacherMobileNo?.Trim(),
                        Email = model.TeacherEmail?.Trim() ?? model.Email?.Trim(),
                        Designation = model.TeacherDesignation,
                        JoiningDate = model.TeacherJoiningDate,
                        IsActive = true,
                        CreatedDate = DateTime.Now
                    };
                    _context.TblTeachers.Add(teacher);
                    await _context.SaveChangesAsync();
                    teacherId = teacher.TeacherId;
                }

                int? empId = model.EmpId;
                if (model.CreateEmployee)
                {
                    var employee = new Employee
                    {
                        EmployeeCode = model.EmployeeCode?.Trim(),
                        Name = model.EmployeeName!.Trim(),
                        Designation = model.EmployeeDesignation,
                        Department = model.EmployeeDepartment,
                        BasicSalary = model.EmployeeBasicSalary,
                        IsActive = true,
                        CreatedDate = DateTime.Now
                    };

                    if (employee.BasicSalary.HasValue)
                    {
                        employee.DailyRate = employee.BasicSalary / 30;
                        employee.OvertimeRate = employee.DailyRate * 2;
                    }

                    _context.Employees.Add(employee);
                    await _context.SaveChangesAsync();
                    empId = employee.Id;
                }

                var user = new UserMaster
                {
                    Username     = model.Username.Trim(),
                    Email        = model.Email.Trim(),
                    PasswordHash = Password.Encrypt(model.Password),
                    CreatedDate  = DateTime.Now,
                    IsActive     = true,
                    StudentId    = model.StudentId,
                    TeacherId    = teacherId,
                    EmpId        = empId
                };
                _context.UserMasters.Add(user);
                await _context.SaveChangesAsync();

                _context.UserRoleAssigns.Add(new UserRoleAssign
                {
                    UserId      = user.UserId,
                    RoleId      = model.RoleId,
                    CreatedDate = DateTime.Now,
                    IsActive    = true
                });
                await _context.SaveChangesAsync();

                await transaction.CommitAsync();

                TempData["Success"] = "User registered successfully!";
                return RedirectToAction("UserList");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "Registration failed: " + ex.Message);
                ViewBag.Roles = _context.RoleMasters.Where(x => x.IsActive == true).OrderBy(x => x.RoleName).ToList();
                return View(model);
            }
        }

        // ─────────────────────────────────────────────
        //  AJAX: Get linked entities by role name
        //  GET /Account/GetLinkedEntities?roleName=Student
        // ─────────────────────────────────────────────
        [Authorize(Roles = "SuperAdmin,Admin")]
        [HttpGet]
        public async Task<IActionResult> GetLinkedEntities(string roleName)
        {
            if (string.IsNullOrWhiteSpace(roleName))
                return Json(new List<object>());

            switch (roleName.ToLower())
            {
                case "student":
                    // Students who don't have a user account yet
                    var linkedStudentIds = await _context.UserMasters
                        .Where(u => u.StudentId.HasValue)
                        .Select(u => u.StudentId!.Value)
                        .ToListAsync();

                    var students = await _context.TblStudents
                        .Where(s => s.IsActive == true && !linkedStudentIds.Contains(s.StudentId))
                        .OrderBy(s => s.StudentName)
                        .Select(s => new { id = s.StudentId, name = s.StudentName + " (" + (s.AdmissionNo ?? "No Adm#") + ")" })
                        .ToListAsync();
                    return Json(students);

                case "teacher":
                case "principal":
                    // Teachers who don't have a user account yet
                    var linkedTeacherIds = await _context.UserMasters
                        .Where(u => u.TeacherId.HasValue)
                        .Select(u => u.TeacherId!.Value)
                        .ToListAsync();

                    var teachers = await _context.TblTeachers
                        .Where(t => t.IsActive == true && !linkedTeacherIds.Contains(t.TeacherId))
                        .OrderBy(t => t.TeacherName)
                        .Select(t => new { id = t.TeacherId, name = t.TeacherName + (t.Designation != null ? " - " + t.Designation : "") })
                        .ToListAsync();
                    return Json(teachers);

                case "employee":
                case "admin":
                    // Employees who don't have a user account yet
                    var linkedEmpIds = await _context.UserMasters
                        .Where(u => u.EmpId.HasValue)
                        .Select(u => u.EmpId!.Value)
                        .ToListAsync();

                    var employees = await _context.Employees
                        .Where(e => e.IsActive == true && !linkedEmpIds.Contains(e.Id))
                        .OrderBy(e => e.Name)
                        .Select(e => new { id = e.Id, name = e.Name + " (" + (e.EmployeeCode ?? "No Code") + ")" })
                        .ToListAsync();
                    return Json(employees);

                default:
                    return Json(new List<object>());
            }
        }

        // ─────────────────────────────────────────────
        //  User List  (Admin/SuperAdmin only)
        // ─────────────────────────────────────────────
        [Authorize(Roles = "SuperAdmin,Admin")]
        [HttpGet]
        public async Task<IActionResult> UserList(int? roleId)
        {
            var roles = await _context.RoleMasters
                .Where(r => r.IsActive == true)
                .OrderBy(r => r.RoleName)
                .ToListAsync();
            ViewBag.Roles   = roles;
            ViewBag.FilterRoleId = roleId;

            var query = _context.UserMasters
                .Include(u => u.UserRoleAssigns).ThenInclude(ra => ra.Role)
                .Include(u => u.Student)
                .Include(u => u.Teacher)
                .Include(u => u.Emp)
                .AsQueryable();

            if (roleId.HasValue && roleId > 0)
                query = query.Where(u => u.UserRoleAssigns.Any(ra => ra.RoleId == roleId && ra.IsActive == true));

            var users = await query
                .OrderByDescending(u => u.CreatedDate)
                .ToListAsync();

            var list = users.Select(u =>
            {
                var ra = u.UserRoleAssigns.FirstOrDefault(r => r.IsActive == true);
                var linkages = new List<UserLinkageDto>();

                if (u.StudentId.HasValue && u.Student != null)
                {
                    linkages.Add(new UserLinkageDto { Type = "Student", Name = u.Student.StudentName ?? "-" });
                }
                if (u.TeacherId.HasValue && u.Teacher != null)
                {
                    linkages.Add(new UserLinkageDto { Type = "Teacher", Name = u.Teacher.TeacherName ?? "-" });
                }
                if (u.EmpId.HasValue && u.Emp != null)
                {
                    linkages.Add(new UserLinkageDto { Type = "Employee", Name = u.Emp.Name ?? "-" });
                }

                string linkedTo   = "-";
                string linkedType = "-";
                if (linkages.Any())
                {
                    linkedTo   = linkages.First().Name;
                    linkedType = linkages.First().Type;
                }

                return new UserListViewModel
                {
                    UserId      = u.UserId,
                    Username    = u.Username,
                    Email       = u.Email,
                    RoleName    = ra?.Role?.RoleName ?? "No Role",
                    RoleId      = ra?.RoleId ?? 0,
                    LinkedTo    = linkedTo,
                    LinkedType  = linkedType,
                    Linkages    = linkages,
                    IsActive    = u.IsActive ?? false,
                    CreatedDate = u.CreatedDate
                };
            }).ToList();

            return View(list);
        }

        // ─────────────────────────────────────────────
        //  Toggle User Active/Inactive
        // ─────────────────────────────────────────────
        [Authorize(Roles = "SuperAdmin,Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleUserStatus(int userId)
        {
            var user = await _context.UserMasters.FindAsync(userId);
            if (user == null) return NotFound();

            user.IsActive   = !(user.IsActive ?? false);
            user.UpdatedDate = DateTime.Now;
            await _context.SaveChangesAsync();

            TempData["Success"] = $"User '{user.Username}' has been {(user.IsActive == true ? "activated" : "deactivated")}.";
            return RedirectToAction("UserList");
        }

        // ─────────────────────────────────────────────
        //  AJAX: Get details for managing a user (role & linkage)
        //  GET /Account/GetManageUserData?userId=123
        // ─────────────────────────────────────────────
        [Authorize(Roles = "SuperAdmin,Admin")]
        [HttpGet]
        public async Task<IActionResult> GetManageUserData(int userId)
        {
            var user = await _context.UserMasters
                .Include(u => u.UserRoleAssigns)
                .Where(u => u.UserId == userId)
                .FirstOrDefaultAsync();

            if (user == null) return NotFound(new { message = "User not found!" });

            var activeRole = user.UserRoleAssigns.FirstOrDefault(r => r.IsActive == true);
            int currentRoleId = activeRole?.RoleId ?? 0;

            // Fetch list of linked teacher IDs from other users
            var linkedTeacherIds = await _context.UserMasters
                .Where(u => u.TeacherId.HasValue && u.UserId != userId)
                .Select(u => u.TeacherId!.Value)
                .ToListAsync();

            // Teachers not linked to other users (or currently linked to this user)
            var unlinkedTeachers = await _context.TblTeachers
                .Where(t => t.IsActive == true && (!linkedTeacherIds.Contains(t.TeacherId) || t.TeacherId == user.TeacherId))
                .OrderBy(t => t.TeacherName)
                .Select(t => new { id = t.TeacherId, name = t.TeacherName })
                .ToListAsync();

            // Fetch list of linked employee IDs from other users
            var linkedEmpIds = await _context.UserMasters
                .Where(u => u.EmpId.HasValue && u.UserId != userId)
                .Select(u => u.EmpId!.Value)
                .ToListAsync();

            // Employees not linked to other users (or currently linked to this user)
            var unlinkedEmployees = await _context.Employees
                .Where(e => e.IsActive == true && (!linkedEmpIds.Contains(e.Id) || e.Id == user.EmpId))
                .OrderBy(e => e.Name)
                .Select(e => new { id = e.Id, name = e.Name + " (" + (e.EmployeeCode ?? "No Code") + ")" })
                .ToListAsync();

            return Json(new
            {
                userId = user.UserId,
                username = user.Username,
                roleId = currentRoleId,
                teacherId = user.TeacherId,
                empId = user.EmpId,
                unlinkedTeachers = unlinkedTeachers,
                unlinkedEmployees = unlinkedEmployees
            });
        }

        // ─────────────────────────────────────────────
        //  AJAX POST: Update user role and linkage
        //  POST /Account/UpdateUserRoleAndLinkage
        // ─────────────────────────────────────────────
        [Authorize(Roles = "SuperAdmin,Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateUserRoleAndLinkage(int userId, int roleId, int? teacherId, int? empId)
        {
            try
            {
                var user = await _context.UserMasters
                    .Include(u => u.UserRoleAssigns)
                    .Where(u => u.UserId == userId)
                    .FirstOrDefaultAsync();

                if (user == null) return Json(new { success = false, message = "User not found!" });

                // Validate TeacherId linkage exclusivity
                if (teacherId.HasValue && teacherId.Value > 0)
                {
                    var isLinkedToOther = await _context.UserMasters
                        .AnyAsync(u => u.TeacherId == teacherId.Value && u.UserId != userId);
                    if (isLinkedToOther)
                    {
                        return Json(new { success = false, message = "This Teacher profile is already linked to another user account." });
                    }
                }

                // Validate EmpId linkage exclusivity
                if (empId.HasValue && empId.Value > 0)
                {
                    var isLinkedToOther = await _context.UserMasters
                        .AnyAsync(u => u.EmpId == empId.Value && u.UserId != userId);
                    if (isLinkedToOther)
                    {
                        return Json(new { success = false, message = "This Employee profile is already linked to another user account." });
                    }
                }

                await using var transaction = await _context.Database.BeginTransactionAsync();

                // Update linkage IDs
                user.TeacherId = (teacherId.HasValue && teacherId.Value > 0) ? teacherId : null;
                user.EmpId = (empId.HasValue && empId.Value > 0) ? empId : null;
                user.UpdatedDate = DateTime.Now;

                // Update Role if changed
                var activeRoleAssign = user.UserRoleAssigns.FirstOrDefault(r => r.IsActive == true);
                if (activeRoleAssign == null || activeRoleAssign.RoleId != roleId)
                {
                    // Deactivate existing role(s)
                    foreach (var roleAssign in user.UserRoleAssigns.Where(r => r.IsActive == true))
                    {
                        roleAssign.IsActive = false;
                        roleAssign.UpdatedDate = DateTime.Now;
                    }

                    // Assign new role
                    _context.UserRoleAssigns.Add(new UserRoleAssign
                    {
                        UserId = user.UserId,
                        RoleId = roleId,
                        CreatedDate = DateTime.Now,
                        IsActive = true
                    });
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return Json(new { success = true, message = "User role and linkage updated successfully!" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "An error occurred: " + ex.Message });
            }
        }

        // ─────────────────────────────────────────────
        //  Role Management  (SuperAdmin only)
        // ─────────────────────────────────────────────
        [Authorize]
        public IActionResult RoleRegistration()
        {
            var model = new RoleViewModel
            {
                RoleList = _context.RoleMasters
                    .Where(x => x.IsActive == true)
                    .OrderByDescending(x => x.RoleId)
                    .ToList()
            };
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public IActionResult RoleRegistration(RoleViewModel model)
        {
            if (!ModelState.IsValid)
            {
                model.RoleList = _context.RoleMasters.Where(x => x.IsActive == true).ToList();
                return View(model);
            }

            if (_context.RoleMasters.Any(x => x.RoleName == model.RoleName))
            {
                ModelState.AddModelError("", "Role already exists.");
                model.RoleList = _context.RoleMasters.ToList();
                return View(model);
            }

            _context.RoleMasters.Add(new RoleMaster
            {
                RoleName    = model.RoleName,
                Description = model.Description,
                CreatedDate = DateTime.Now,
                IsActive    = true
            });
            _context.SaveChanges();
            return RedirectToAction("RoleRegistration");
        }

        // ─────────────────────────────────────────────
        //  Menu Permission Assignment
        // ─────────────────────────────────────────────
        [Authorize]
        [HttpGet]
        public async Task<IActionResult> AssignMenu()
        {
            var roles = await _context.RoleMasters
                .Select(r => new SelectListItem { Text = r.RoleName, Value = r.RoleId.ToString() })
                .ToListAsync();

            var vm = new AssignMenuPermissionViewModel
            {
                Roles           = roles,
                MenuPermissions = await GetMenuHierarchy()
            };
            return View(vm);
        }

        [HttpGet]
        public async Task<IActionResult> GetMenuPermissionsByRole(string roleId)
        {
            if (string.IsNullOrEmpty(roleId)) return BadRequest("RoleId is required");

            var menus = await _context.TblMenus
                .Include(m => m.InverseParent)
                .Include(m => m.TblMenuPermissions)
                .Where(m => m.IsActive)
                .ToListAsync();

            var result = new List<MenuPermissionDto>();

            void Map(TblMenu menu)
            {
                var perm = menu.TblMenuPermissions.FirstOrDefault(p => p.RoleId == Convert.ToInt32(roleId));
                result.Add(new MenuPermissionDto
                {
                    MenuId    = menu.MenuId,
                    MenuName  = menu.MenuName,
                    CanRead   = perm?.CanRead   ?? false,
                    CanCreate = perm?.CanCreate ?? false,
                    CanUpdate = perm?.CanUpdate ?? false,
                    CanDelete = perm?.CanDelete ?? false,
                    Children  = new List<MenuPermissionDto>()
                });
                foreach (var child in menu.InverseParent) Map(child);
            }

            foreach (var menu in menus.Where(m => m.ParentId == null)) Map(menu);
            return Json(result);
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> MenuAsign(AssignMenuPermissionViewModel model)
        {
            if (string.IsNullOrEmpty(model.SelectedRole))
            {
                TempData["Error"] = "Please select a role!";
                return RedirectToAction(nameof(AssignMenu));
            }

            var existing = _context.TblMenuPermissions
                .Where(mp => mp.RoleId == Convert.ToInt32(model.SelectedRole));
            _context.TblMenuPermissions.RemoveRange(existing);
            _context.TblMenuPermissions.AddRange(FlattenMenuPermissions(model.MenuPermissions, model.SelectedRole));
            await _context.SaveChangesAsync();

            TempData["Success"] = "Permissions saved successfully!";
            return RedirectToAction(nameof(AssignMenu));
        }

        // ─────────────────────────────────────────────
        //  Secure Page (token debug)
        // ─────────────────────────────────────────────
        [Authorize]
        public async Task<IActionResult> SecurePage()
        {
            // ── Basic token info ──────────────────────────────────────────
            var accessToken = Request.Cookies["AccessToken"];
            DateTime? expiry = null;
            if (!string.IsNullOrEmpty(accessToken))
                expiry = new JwtSecurityTokenHandler().ReadJwtToken(accessToken).ValidTo;

            ViewBag.AccessToken = accessToken;
            ViewBag.Role        = User.FindFirst(ClaimTypes.Role)?.Value;
            ViewBag.UserId      = User.FindFirst("UserId")?.Value;
            ViewBag.Username    = User.Identity?.Name;
            ViewBag.Expiry      = expiry;

            // ── ACADEMIC ──────────────────────────────────────────────────
            ViewBag.TotalStudents  = await _context.TblStudents.CountAsync(s => s.IsActive == true);
            ViewBag.TotalTeachers  = await _context.TblTeachers.CountAsync(t => t.IsActive == true);
            ViewBag.TotalClasses   = await _context.TblClasses.CountAsync(c => c.IsActive == true);
            ViewBag.TotalSessions  = await _context.TblAcademicSessions.CountAsync(s => s.IsActive == true);

            // ── ATTENDANCE (today) ────────────────────────────────────────
            var today = DateOnly.FromDateTime(DateTime.Today);
            var todayAtt = await _context.TblStudentAttendances
                .Where(a => a.AttendanceDate == today)
                .GroupBy(a => a.Status)
                .Select(g => new { Status = g.Key, Count = g.Count() })
                .ToListAsync();
            ViewBag.AttPresent = todayAtt.FirstOrDefault(x => x.Status == "Present")?.Count ?? 0;
            ViewBag.AttAbsent  = todayAtt.FirstOrDefault(x => x.Status == "Absent")?.Count ?? 0;
            ViewBag.AttLate    = todayAtt.FirstOrDefault(x => x.Status == "Late")?.Count ?? 0;
            int totalAtt = (ViewBag.AttPresent + ViewBag.AttAbsent + ViewBag.AttLate);
            ViewBag.AttPct = totalAtt > 0 ? Math.Round((decimal)ViewBag.AttPresent * 100 / totalAtt, 1) : 0m;

            // ── FEES ──────────────────────────────────────────────────────
            var thisMonth = DateTime.Today.Month;
            var thisYear  = DateTime.Today.Year;
            ViewBag.FeeCollectedMonth = await _context.TblFeeCollections
                .Where(f => f.Month == thisMonth && f.Year == thisYear && f.IsActive == true)
                .SumAsync(f => (decimal?)f.PaidAmount) ?? 0;
            ViewBag.FeePending = await _context.TblFeeCollections
                .Where(f => f.IsActive == true && f.TotalAmount > f.PaidAmount)
                .SumAsync(f => (decimal?)(f.TotalAmount - f.PaidAmount)) ?? 0;
            ViewBag.FeeStudentsWithDue = await _context.TblFeeCollections
                .Where(f => f.IsActive == true && f.TotalAmount > f.PaidAmount)
                .Select(f => f.StudentId).Distinct().CountAsync();

            // Monthly fee trend (last 6 months)
            var feeMonths = new List<object>();
            for (int i = 5; i >= 0; i--)
            {
                var d = DateTime.Today.AddMonths(-i);
                var amt = await _context.TblFeeCollections
                    .Where(f => f.Month == d.Month && f.Year == d.Year && f.IsActive == true)
                    .SumAsync(f => (decimal?)f.PaidAmount) ?? 0;
                feeMonths.Add(new { month = d.ToString("MMM"), amount = amt });
            }
            ViewBag.FeeMonthlyTrend = feeMonths;

            // ── LIBRARY ───────────────────────────────────────────────────
            ViewBag.LibTotalBooks    = await _context.LibBooks.CountAsync(b => b.IsActive);
            ViewBag.LibIssuedBooks   = await _context.LibIssueTransactions.CountAsync(t => !t.IsReturned);
            ViewBag.LibOverdueBooks  = await _context.LibIssueTransactions
                .CountAsync(t => !t.IsReturned && t.DueDate < today);
            ViewBag.LibFineCollected = await _context.LibFinePayments
                .Where(p => p.PaymentDate.Month == thisMonth && p.PaymentDate.Year == thisYear)
                .SumAsync(p => (decimal?)p.AmountPaid) ?? 0;

            // ── INVENTORY / STORE ─────────────────────────────────────────
            ViewBag.InvTotalProducts  = await _context.InvProducts.CountAsync(p => p.IsActive);
            ViewBag.InvLowStock       = await _context.InvProducts
                .CountAsync(p => p.IsActive && p.CurrentStock <= p.ReorderLevel);
            ViewBag.InvOutOfStock     = await _context.InvProducts
                .CountAsync(p => p.IsActive && p.CurrentStock == 0);
            ViewBag.InvTodaySales     = await _context.InvSaleTransactions
                .Where(s => s.SaleDate == today && s.BillType == "Sale")
                .SumAsync(s => (decimal?)s.TotalAmount) ?? 0;
            ViewBag.InvPendingCredits = await _context.InvSaleTransactions
                .CountAsync(s => !s.IsPaid && s.BillType == "Sale");

            // Category-wise stock value
            var catStock = await _context.InvProducts
                .Include(p => p.Category)
                .Where(p => p.IsActive)
                .GroupBy(p => p.Category.CategoryName)
                .Select(g => new { cat = g.Key, val = g.Sum(p => p.CurrentStock * p.CostPrice) })
                .OrderByDescending(x => x.val)
                .Take(6)
                .ToListAsync();
            ViewBag.InvCatStock = catStock;

            // ── ASSETS ────────────────────────────────────────────────────
            ViewBag.AsmTotalAssets   = await _context.AsmAssets.Where(a => a.IsActive).SumAsync(a => (int?)a.TotalUnits) ?? 0;
            ViewBag.AsmAvailable     = await _context.AsmAssets.Where(a => a.IsActive).SumAsync(a => (int?)a.AvailableUnits) ?? 0;
            ViewBag.AsmIssued        = await _context.AsmIssueTransactions.CountAsync(t => !t.IsReturned);
            ViewBag.AsmUnderRepair   = await _context.AsmAssetUnits.CountAsync(u => u.UnitCondition == "UnderRepair" && u.IsActive);
            ViewBag.AsmOverdue       = await _context.AsmIssueTransactions
                .CountAsync(t => !t.IsReturned && t.ExpectedReturnDate.HasValue && t.ExpectedReturnDate.Value < today);

            // ── COMMUNICATION ─────────────────────────────────────────────
            ViewBag.CommAnnouncements = await _context.CommAnnouncements.CountAsync(a => a.IsPublished);
            ViewBag.CommEvents        = await _context.CommEvents
                .CountAsync(e => e.IsPublished && e.StartDate >= today && e.StartDate <= today.AddDays(30));
            ViewBag.CommUnreadNoti    = await _context.CommNotifications.CountAsync(n => !n.IsRead);
            ViewBag.CommMessages      = await _context.CommMessages.CountAsync(m => !m.IsRead && !m.IsDeleted);

            // Upcoming events (next 7 days)
            var upcomingEvents = await _context.CommEvents
                .Where(e => e.IsPublished && e.StartDate >= today && e.StartDate <= today.AddDays(7))
                .OrderBy(e => e.StartDate)
                .Take(5)
                .ToListAsync();
            ViewBag.UpcomingEvents = upcomingEvents;

            // ── EMPLOYEES ─────────────────────────────────────────────────
            ViewBag.TotalEmployees = await _context.Employees.CountAsync(e => e.IsActive == true);
            var deptDist = await _context.Employees
                .Where(e => e.IsActive == true && e.Department != null)
                .GroupBy(e => e.Department)
                .Select(g => new { dept = g.Key, count = g.Count() })
                .OrderByDescending(x => x.count)
                .Take(5)
                .ToListAsync();
            ViewBag.DeptDistribution = deptDist;

            // ── RECENT ACTIVITY ───────────────────────────────────────────
            ViewBag.RecentBills = await _context.InvSaleTransactions
                .OrderByDescending(s => s.CreatedAt)
                .Take(5)
                .ToListAsync();

            ViewBag.RecentAnnouncements = await _context.CommAnnouncements
                .Where(a => a.IsPublished)
                .OrderByDescending(a => a.CreatedAt)
                .Take(4)
                .ToListAsync();

            return View();
        }

        // ─────────────────────────────────────────────
        //  Helpers
        // ─────────────────────────────────────────────
        private async Task<List<MenuPermissionDto>> GetMenuHierarchy()
        {
            var menus = await _context.TblMenus.Where(m => m.IsActive).ToListAsync();

            List<MenuPermissionDto> BuildTree(int? parentId) =>
                menus.Where(m => m.ParentId == parentId)
                     .Select(m => new MenuPermissionDto
                     {
                         MenuId   = m.MenuId,
                         MenuName = m.MenuName,
                         Children = BuildTree(m.MenuId)
                     }).ToList();

            return BuildTree(null);
        }

        private List<TblMenuPermission> FlattenMenuPermissions(List<MenuPermissionDto> items, string roleId)
        {
            var list = new List<TblMenuPermission>();

            void Traverse(List<MenuPermissionDto> nodes)
            {
                foreach (var item in nodes)
                {
                    list.Add(new TblMenuPermission
                    {
                        RoleId    = Convert.ToInt32(roleId),
                        MenuId    = item.MenuId,
                        CanRead   = item.CanRead,
                        CanCreate = item.CanCreate,
                        CanUpdate = item.CanUpdate,
                        CanDelete = item.CanDelete
                    });
                    if (item.Children?.Any() == true) Traverse(item.Children);
                }
            }

            Traverse(items);
            return list;
        }
    }
}
