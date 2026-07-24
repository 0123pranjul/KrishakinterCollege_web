using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using System.Text.Json;

namespace School_CRM.Models
{
    /// <summary>
    /// LibmanagementContext — Audit Extension (Partial Class)
    ///
    /// Scaffolded file (LibmanagementContext.cs) bilkul touch nahi kiya.
    /// Extra constructor IHttpContextAccessor accept karta hai —
    /// DI automatically isko prefer karega.
    ///
    /// Result: Har INSERT/UPDATE/DELETE par automatically log banega
    /// jisme user info (name, role, IP) bhi hoga — ZERO controller changes.
    /// </summary>
    public partial class LibmanagementContext
    {
        private readonly IHttpContextAccessor? _httpContextAccessor;

        // ── Extra constructor — IHttpContextAccessor ke saath ────────────────
        public LibmanagementContext(
            DbContextOptions<LibmanagementContext> options,
            IHttpContextAccessor httpContextAccessor)
            : base(options)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        // ── Tables jo audit NAHI karni hain ──────────────────────────────────
        private static readonly HashSet<string> _excludedFromAudit =
            new(StringComparer.OrdinalIgnoreCase)
            {
                nameof(AuditLog),
                nameof(RefreshToken)
            };

        // ─────────────────────────────────────────────────────────────────────
        //  SaveChangesAsync Override
        // ─────────────────────────────────────────────────────────────────────
        public override async Task<int> SaveChangesAsync(
            CancellationToken cancellationToken = default)
        {
            var auditEntries = BuildAuditEntries();
            int result       = await base.SaveChangesAsync(cancellationToken);
            await FinalizeAndSaveAuditEntries(auditEntries, cancellationToken);
            return result;
        }

        // ─────────────────────────────────────────────────────────────────────
        //  Step 1 — save se PEHLE: old values + user info capture
        // ─────────────────────────────────────────────────────────────────────
        private List<AuditEntryBuilder> BuildAuditEntries()
        {
            var httpContext = _httpContextAccessor?.HttpContext;
            var userId      = httpContext?.Request.Cookies["userId"];
            var userName    = httpContext?.Request.Cookies["userName"];
            var roleName    = httpContext?.Request.Cookies["roleName"];
            var ipAddress   = httpContext?.Connection.RemoteIpAddress?.ToString() ?? "unknown";
            var routeData   = httpContext?.GetRouteData();
            var controller  = routeData?.Values["controller"]?.ToString();
            var actionName  = routeData?.Values["action"]?.ToString();
            var requestUrl  = httpContext != null
                ? $"{httpContext.Request.Path}{httpContext.Request.QueryString}"
                : null;

            var entries = new List<AuditEntryBuilder>();

            foreach (var entry in ChangeTracker.Entries())
            {
                if (entry.State == EntityState.Detached ||
                    entry.State == EntityState.Unchanged)
                    continue;

                var entityName = entry.Entity.GetType().Name;
                if (_excludedFromAudit.Contains(entityName))
                    continue;

                var builder = new AuditEntryBuilder
                {
                    Entry           = entry,
                    TableName       = entityName,
                    Action          = entry.State switch
                    {
                        EntityState.Added    => "INSERT",
                        EntityState.Modified => "UPDATE",
                        EntityState.Deleted  => "DELETE",
                        _                    => "UNKNOWN"
                    },
                    ChangedByUserId = int.TryParse(userId, out int uid) ? uid : null,
                    ChangedByName   = userName,
                    UserRole        = roleName,
                    IpAddress       = ipAddress,
                    ControllerName  = controller,
                    ActionName      = actionName,
                    RequestUrl      = requestUrl
                };

                foreach (var prop in entry.Properties)
                {
                    var propName = prop.Metadata.Name;
                    if (IsSensitiveProperty(propName)) continue;

                    if (prop.IsTemporary)
                    {
                        builder.HasTemporaryProperties = true;
                        continue;
                    }

                    if (prop.Metadata.IsPrimaryKey())
                        builder.KeyValues[propName] = prop.CurrentValue;

                    switch (entry.State)
                    {
                        case EntityState.Added:
                            builder.NewValues[propName] = prop.CurrentValue;
                            break;
                        case EntityState.Deleted:
                            builder.OldValues[propName] = prop.OriginalValue;
                            break;
                        case EntityState.Modified:
                            if (prop.IsModified)
                            {
                                builder.OldValues[propName] = prop.OriginalValue;
                                builder.NewValues[propName] = prop.CurrentValue;
                            }
                            break;
                    }
                }

                entries.Add(builder);
            }

            return entries;
        }

        // ─────────────────────────────────────────────────────────────────────
        //  Step 2 — save ke BAAD: INSERT ka generated PK fill karo
        // ─────────────────────────────────────────────────────────────────────
        private async Task FinalizeAndSaveAuditEntries(
            List<AuditEntryBuilder> builders,
            CancellationToken cancellationToken)
        {
            if (builders.Count == 0) return;

            var auditLogs = new List<AuditLog>();

            foreach (var builder in builders)
            {
                if (builder.HasTemporaryProperties)
                {
                    foreach (var prop in builder.Entry.Properties)
                    {
                        if (IsSensitiveProperty(prop.Metadata.Name)) continue;
                        if (prop.Metadata.IsPrimaryKey())
                            builder.KeyValues[prop.Metadata.Name] = prop.CurrentValue;
                        if (builder.Action == "INSERT")
                            builder.NewValues[prop.Metadata.Name] = prop.CurrentValue;
                    }
                }

                auditLogs.Add(new AuditLog
                {
                    TableName       = builder.TableName,
                    Action          = builder.Action,
                    RecordId        = builder.KeyValues.Values.FirstOrDefault()?.ToString(),
                    OldValues       = builder.OldValues.Count > 0
                                        ? JsonSerializer.Serialize(builder.OldValues) : null,
                    NewValues       = builder.NewValues.Count > 0
                                        ? JsonSerializer.Serialize(builder.NewValues) : null,
                    ChangedByUserId = builder.ChangedByUserId,
                    ChangedByName   = builder.ChangedByName,
                    UserRole        = builder.UserRole,
                    IpAddress       = builder.IpAddress,
                    ControllerName  = builder.ControllerName,
                    ActionName      = builder.ActionName,
                    RequestUrl      = builder.RequestUrl,
                    ChangedAt       = DateTime.Now
                });
            }

            if (auditLogs.Count > 0)
            {
                AuditLogs.AddRange(auditLogs);
                await base.SaveChangesAsync(cancellationToken); // base — infinite loop nahi
            }
        }

        // ─────────────────────────────────────────────────────────────────────
        //  Helper
        // ─────────────────────────────────────────────────────────────────────
        private static bool IsSensitiveProperty(string propertyName)
        {
            var sensitiveNames = new[]
            {
                "password", "passwordhash", "token",
                "secret", "key", "hash", "salt", "credential"
            };
            return sensitiveNames.Any(s =>
                propertyName.Contains(s, StringComparison.OrdinalIgnoreCase));
        }

        private class AuditEntryBuilder
        {
            public EntityEntry Entry                  { get; set; } = null!;
            public string      TableName              { get; set; } = "";
            public string      Action                 { get; set; } = "";
            public bool        HasTemporaryProperties { get; set; }
            public int?    ChangedByUserId { get; set; }
            public string? ChangedByName   { get; set; }
            public string? UserRole        { get; set; }
            public string? IpAddress       { get; set; }
            public string? ControllerName  { get; set; }
            public string? ActionName      { get; set; }
            public string? RequestUrl      { get; set; }
            public Dictionary<string, object?> KeyValues { get; } = new();
            public Dictionary<string, object?> OldValues { get; } = new();
            public Dictionary<string, object?> NewValues { get; } = new();
        }
    }
}
