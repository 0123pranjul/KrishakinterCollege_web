using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using School_CRM.Models;
using School_CRM.Services.Interface;
using System.Text.Json;

namespace School_CRM.Services.Filters
{
    /// <summary>
    /// AuditActionFilter — Attribute ke roop mein use karo kisi bhi Controller Action par.
    ///
    /// Usage examples:
    ///   [AuditAction("Student", "INSERT")]
    ///   [AuditAction("Student", "UPDATE", captureForm: true)]
    ///   [AuditAction("Employee", "DELETE")]
    ///
    /// Ye filter automatically capture karta hai:
    ///   - User info (cookies se)
    ///   - IP address
    ///   - Controller / Action name
    ///   - Request URL
    ///   - Form data (optional — captureForm: true karo)
    ///   - Action result status (success/failure)
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public class AuditActionFilter : ActionFilterAttribute
    {
        private readonly string _tableName;
        private readonly string _action;          // INSERT / UPDATE / DELETE
        private readonly bool   _captureForm;     // Form data bhi save karein?
        private readonly string? _recordIdParam;  // URL/form mein konsa param hai PK ke liye

        /// <param name="tableName">DB table name — e.g. "TblStudent"</param>
        /// <param name="action">INSERT / UPDATE / DELETE</param>
        /// <param name="captureForm">true karo toh form fields bhi NewValues mein save hongi</param>
        /// <param name="recordIdParam">Primary key param name — e.g. "id", "studentId"</param>
        public AuditActionFilter(
            string tableName,
            string action,
            bool   captureForm    = false,
            string recordIdParam  = "id")
        {
            _tableName      = tableName;
            _action         = action.ToUpper();
            _captureForm    = captureForm;
            _recordIdParam  = recordIdParam;
        }

        public override async Task OnActionExecutionAsync(
            ActionExecutingContext context,
            ActionExecutionDelegate next)
        {
            // ── Form data capture karo (execute HONE SE PEHLE) ───────────────
            string? newValuesJson = null;

            if (_captureForm && context.HttpContext.Request.HasFormContentType)
            {
                try
                {
                    var form = await context.HttpContext.Request.ReadFormAsync();

                    // Sensitive fields KABHI log mat karo
                    var safeFields = form
                        .Where(f => !IsSensitiveField(f.Key))
                        .ToDictionary(f => f.Key, f => f.Value.ToString());

                    newValuesJson = JsonSerializer.Serialize(safeFields,
                        new JsonSerializerOptions { WriteIndented = false });
                }
                catch { /* form read fail ho toh ignore */ }
            }

            // ── Action execute karo ──────────────────────────────────────────
            var executedContext = await next();

            // ── Sirf successful operations log karo ─────────────────────────
            // Agar action fail hua (exception ya redirect to error) toh log skip karo
            if (executedContext.Exception != null && !executedContext.ExceptionHandled)
                return;

            // Redirect result ko success maano (form submit ke baad redirect hota hai)
            bool isSuccess = executedContext.Result is RedirectToActionResult
                          || executedContext.Result is RedirectResult
                          || executedContext.Result is OkResult
                          || executedContext.Result is OkObjectResult
                          || executedContext.Result is JsonResult jsonR && IsSuccessJson(jsonR);

            if (!isSuccess) return;

            try
            {
                var httpContext = context.HttpContext;

                // ── User info cookies se ─────────────────────────────────────
                var userId   = httpContext.Request.Cookies["userId"];
                var userName = httpContext.Request.Cookies["userName"];
                var roleName = httpContext.Request.Cookies["roleName"];
                var ip       = httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";

                // ── Record ID nikalo (route ya form se) ──────────────────────
                string? recordId = null;

                // Route values se try karo pehle
                if (context.ActionArguments.TryGetValue(_recordIdParam, out var routeId))
                    recordId = routeId?.ToString();

                // Route data se try karo
                if (recordId == null)
                {
                    var routeData = httpContext.GetRouteData();
                    recordId = routeData?.Values[_recordIdParam]?.ToString()
                            ?? routeData?.Values["id"]?.ToString();
                }

                // ── Controller / Action ──────────────────────────────────────
                var controllerName = context.RouteData.Values["controller"]?.ToString();
                var actionName     = context.RouteData.Values["action"]?.ToString();

                // ── AuditLog entry banao ─────────────────────────────────────
                var auditEntry = new AuditLog
                {
                    TableName       = _tableName,
                    Action          = _action,
                    RecordId        = recordId,
                    OldValues       = null,        // EF Core SaveChanges override mein milega
                    NewValues       = newValuesJson,
                    ChangedByUserId = int.TryParse(userId, out int uid) ? uid : null,
                    ChangedByName   = userName,
                    UserRole        = roleName,
                    IpAddress       = ip,
                    ControllerName  = controllerName,
                    ActionName      = actionName,
                    RequestUrl      = $"{httpContext.Request.Path}{httpContext.Request.QueryString}",
                    ChangedAt       = DateTime.Now,
                    Remarks         = null
                };

                // ── AuditService se save karo ────────────────────────────────
                var auditService = httpContext.RequestServices
                    .GetRequiredService<IAuditService>();

                await auditService.LogAsync(auditEntry);
            }
            catch
            {
                // Audit failure se main operation affect na ho — silently ignore
            }
        }

        // ── Helper: Sensitive field check ─────────────────────────────────────
        private static bool IsSensitiveField(string fieldName)
        {
            var sensitiveNames = new[]
            {
                "password", "passwordhash", "confirmpassword",
                "token", "accesstoken", "refreshtoken",
                "secret", "key", "credential", "__requestverificationtoken"
            };
            return sensitiveNames.Any(s =>
                fieldName.Contains(s, StringComparison.OrdinalIgnoreCase));
        }

        // ── Helper: JSON result success check ─────────────────────────────────
        private static bool IsSuccessJson(JsonResult result)
        {
            try
            {
                var json = JsonSerializer.Serialize(result.Value);
                return json.Contains("\"success\":true", StringComparison.OrdinalIgnoreCase)
                    || json.Contains("\"Success\":true", StringComparison.OrdinalIgnoreCase);
            }
            catch { return false; }
        }
    }
}
