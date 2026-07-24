using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using School_CRM.Infrastructure.DI;
using School_CRM.Models;
using School_CRM.Services;
using School_CRM.Services.Interface;
using Serilog;
using System.Text;
using System.Threading.RateLimiting;

// ── Serilog Bootstrap Logger ──────────────────────────────────────────────────
// App start hone se pehle bhi errors capture ho sakein (early startup errors)
Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateBootstrapLogger();

Log.Information("School CRM starting up...");

try
{

var builder = WebApplication.CreateBuilder(args);
var config = builder.Configuration;

// ── Serilog — appsettings.json se config load karo ───────────────────────────
builder.Host.UseSerilog((context, services, configuration) =>
    configuration.ReadFrom.Configuration(context.Configuration)
                 .ReadFrom.Services(services)
                 .Enrich.FromLogContext());

builder.Services.AddDbContext<LibmanagementContext>(opts =>
    opts.UseSqlServer(config.GetConnectionString("con")));

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = config["Jwt:Issuer"],
        ValidAudience = config["Jwt:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(config["Jwt:Key"]))
    };

    options.Events = new JwtBearerEvents
    {
        OnMessageReceived = context =>
        {
            var token = context.Request.Cookies["AccessToken"];
            if (!string.IsNullOrEmpty(token))
            {
                context.Token = token;
            }
            return Task.CompletedTask;
        }
    };

});
builder.Services.AddScoped<TokenService>();
builder.Services.AddScoped<EmailService>();
builder.Services.AddScoped<IMenuService, MenuService>();
builder.Services.AddScoped<IAuditService, AuditService>();  // 📋 Audit Logging
builder.Services.AddScoped<IDocumentBuilderService, DocumentBuilderService>(); // 📝 Document Builder
builder.Services.AddScoped<IIdCardService, IdCardService>(); // 📝 ID Card Generator
builder.Services.AddHttpContextAccessor();                   // 🌐 HttpContext DI ke liye

// ── Library Management Module ─────────────────────────────────────────────────
builder.Services.AddLibraryServices();

// ── Asset Management Module ───────────────────────────────────────────────────
builder.Services.AddAssetServices();

// ── Inventory / Store Module ──────────────────────────────────────────────────
builder.Services.AddInventoryServices();

// ── Rate Limiting — Brute Force Protection ────────────────────────────────────
// Login endpoint par: ek IP se 1 minute mein max 5 attempts allowed
// 6th attempt par 429 Too Many Requests milega (1 minute cooldown)
builder.Services.AddRateLimiter(options =>
{
    options.AddFixedWindowLimiter("LoginPolicy", limiterOptions =>
    {
        limiterOptions.PermitLimit         = 5;                          // Max 5 attempts
        limiterOptions.Window              = TimeSpan.FromMinutes(1);    // Per 1 minute window
        limiterOptions.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
        limiterOptions.QueueLimit          = 0;                          // No queuing — reject immediately
    });

    // Custom 429 response — Login page par wapis bhejo with error message
    options.OnRejected = async (context, cancellationToken) =>
    {
        context.HttpContext.Response.StatusCode = StatusCodes.Status429TooManyRequests;

        // AJAX request hai toh JSON mein reply karo
        if (context.HttpContext.Request.Headers["X-Requested-With"] == "XMLHttpRequest")
        {
            context.HttpContext.Response.ContentType = "application/json";
            await context.HttpContext.Response.WriteAsync(
                "{\"success\":false,\"message\":\"Too many login attempts. Please wait 1 minute and try again.\"}",
                cancellationToken);
            return;
        }

        // Retry-After header set karo taaki browser/client ko pata chale
        if (context.Lease.TryGetMetadata(MetadataName.RetryAfter, out var retryAfter))
        {
            context.HttpContext.Response.Headers.RetryAfter =
                ((int)retryAfter.TotalSeconds).ToString();
        }

        // Normal browser request — Login page par redirect with TempData message
        context.HttpContext.Response.Redirect("/Account/Login?rateLimited=true");
    };
});

// Add services to the container.
builder.Services.AddControllersWithViews(options =>
{
    // Global auth filter — har controller automatically protected rahega
    // Sirf [AllowAnonymous] wale controllers/actions bypass honge
    var policy = new Microsoft.AspNetCore.Authorization.AuthorizationPolicyBuilder()
        .RequireAuthenticatedUser()
        .Build();
    options.Filters.Add(new Microsoft.AspNetCore.Mvc.Authorization.AuthorizeFilter(policy));
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}
app.UseMiddleware<SecurityHeadersMiddleware>(); // 🛡️ Security Headers — X-Frame, CSP, nosniff etc.
app.UseStaticFiles();
app.UseHttpsRedirection();
app.UseRouting();
app.UseRateLimiter();   // 🚦 Rate Limiting — brute force protection
app.UseSerilogRequestLogging(opts =>
{
    // Sirf useful info log karo — static files skip karo
    opts.GetLevel = (httpContext, elapsed, ex) =>
    {
        if (httpContext.Request.Path.StartsWithSegments("/css") ||
            httpContext.Request.Path.StartsWithSegments("/js")  ||
            httpContext.Request.Path.StartsWithSegments("/lib") ||
            httpContext.Request.Path.StartsWithSegments("/images") ||
            httpContext.Request.Path.StartsWithSegments("/fonts"))
            return Serilog.Events.LogEventLevel.Verbose; // static files skip (nahi dikhenge)

        return ex != null
            ? Serilog.Events.LogEventLevel.Error
            : Serilog.Events.LogEventLevel.Information;
    };
});  // 📋 HTTP request logging

app.UseAuthentication();
app.UseMiddleware<AuthGuardMiddleware>();   // 🔒 Session guard — login check
app.UseMiddleware<RefreshTokenMiddleware>(); // 🔄 Auto token refresh
app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();


    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "School CRM terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}
