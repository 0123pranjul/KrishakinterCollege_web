using School_CRM.Models;

namespace School_CRM.Services.Interface
{
    /// <summary>
    /// Audit Service Interface — manual audit log likhne ke liye
    /// (EF Core automatic tracking ke alawa, jab extra context chahiye)
    /// </summary>
    public interface IAuditService
    {
        /// <summary>
        /// Ek audit record manually save karo
        /// </summary>
        Task LogAsync(AuditLog entry);

        /// <summary>
        /// Simple helper — sirf basic info se log banao
        /// </summary>
        Task LogAsync(
            string tableName,
            string action,
            string? recordId,
            string? oldValues,
            string? newValues,
            HttpContext httpContext,
            string? remarks = null);
    }
}
