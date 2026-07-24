using School_CRM.Models;
using School_CRM.Services.Interface;

namespace School_CRM.Services
{
    /// <summary>
    /// AuditService — AuditLogs table mein records save karta hai.
    /// Ye service Action Filter aur manually dono jagah se call hoti hai.
    /// </summary>
    public class AuditService : IAuditService
    {
        private readonly LibmanagementContext _context;

        public AuditService(LibmanagementContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Pura AuditLog object directly save karo
        /// </summary>
        public async Task LogAsync(AuditLog entry)
        {
            try
            {
                entry.ChangedAt = DateTime.Now;
                _context.AuditLogs.Add(entry);
                await _context.SaveChangesAsync();
            }
            catch
            {
                // Audit logging failure se main operation affect na ho
            }
        }

        /// <summary>
        /// Simple helper — HttpContext se user/IP automatically extract karta hai
        /// </summary>
        public async Task LogAsync(
            string tableName,
            string action,
            string? recordId,
            string? oldValues,
            string? newValues,
            HttpContext httpContext,
            string? remarks = null)
        {
            try
            {
                // Cookies se user info nikalo (aapka existing cookie system)
                var userId   = httpContext.Request.Cookies["userId"];
                var userName = httpContext.Request.Cookies["userName"];
                var roleName = httpContext.Request.Cookies["roleName"];
                var ip       = httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";

                // Controller aur Action name URL se nikalo
                var routeData      = httpContext.GetRouteData();
                var controllerName = routeData?.Values["controller"]?.ToString();
                var actionName     = routeData?.Values["action"]?.ToString();

                var entry = new AuditLog
                {
                    TableName       = tableName,
                    Action          = action,                              // INSERT / UPDATE / DELETE
                    RecordId        = recordId,
                    OldValues       = oldValues,
                    NewValues       = newValues,
                    ChangedByUserId = int.TryParse(userId, out int uid) ? uid : null,
                    ChangedByName   = userName,
                    UserRole        = roleName,
                    IpAddress       = ip,
                    ControllerName  = controllerName,
                    ActionName      = actionName,
                    RequestUrl      = $"{httpContext.Request.Path}{httpContext.Request.QueryString}",
                    ChangedAt       = DateTime.Now,
                    Remarks         = remarks
                };

                _context.AuditLogs.Add(entry);
                await _context.SaveChangesAsync();
            }
            catch
            {
                // Audit logging failure se main operation affect na ho
            }
        }
    }
}
