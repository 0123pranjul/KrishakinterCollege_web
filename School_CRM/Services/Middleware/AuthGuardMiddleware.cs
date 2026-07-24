using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using School_CRM.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Text;

/// <summary>
/// AuthGuardMiddleware — har request pe check karta hai:
///   1. Agar public route hai (Login, static files, Account) → allow
///   2. Agar valid AccessToken cookie hai → allow
///   3. Agar AccessToken nahi / invalid AND RefreshToken bhi nahi / revoked → Home/Index redirect
///   4. Agar AccessToken expired BUT valid RefreshToken DB mein hai →
///      RefreshTokenMiddleware handle karega, allow karo
/// </summary>
public class AuthGuardMiddleware
{
    private readonly RequestDelegate _next;
    private readonly IConfiguration  _config;

    // Yeh paths authentication ke bina allow honge
    private static readonly HashSet<string> _publicPaths = new(StringComparer.OrdinalIgnoreCase)
    {
        "/",
        "/Account/Login",
        "/Account/Register",
        "/Account/Refresh",
        "/Account/VerifyOtp",
        "/Account/ResendOtp",
        "/Account/GetCaptchaImage",
        "/Home/Error",
        "/Enquiry/PublicForm",
        "/Enquiry/SubmitPublicForm",
        "/Enquiry/GetClasses",
        "/Enquiry/GetSessions",
        "/Enquiry/GetCaptchaImage",
        "/kic_logo.png",
    };

    // Yeh path prefixes bhi allow honge (static files + Home controller)
    private static readonly string[] _publicPrefixes =
    {
        "/Home/",          // Home controller pura public — bina login ke accessible
        "/Result/",        // Result controller pura public — bina login ke accessible
        "/css/", "/js/", "/lib/", "/images/", "/fonts/",
        "/favicon", "/_framework", "/.well-known",
        "/AssetsCRM/", "/collage/", "/Gprp/"
    };

    public AuthGuardMiddleware(RequestDelegate next, IConfiguration config)
    {
        _next  = next;
        _config = config;
    }

    public async Task Invoke(HttpContext context, LibmanagementContext db)
    {
        var path = context.Request.Path.Value ?? "/";

        // ── 1. Public paths — allow without check ──────────
        if (IsPublicPath(path))
        {
            await _next(context);
            return;
        }

        var accessToken  = context.Request.Cookies["AccessToken"];
        var refreshToken = context.Request.Cookies["RefreshToken"];

        // ── 2. No cookies at all → redirect to login ───────
        if (string.IsNullOrEmpty(accessToken) && string.IsNullOrEmpty(refreshToken))
        {
            RedirectToLogin(context);
            return;
        }

        // ── 3. Validate AccessToken ─────────────────────────
        if (!string.IsNullOrEmpty(accessToken))
        {
            var tokenState = ValidateToken(accessToken);

            if (tokenState == TokenState.Valid)
            {
                // Token valid — allow request
                await _next(context);
                return;
            }

            if (tokenState == TokenState.Expired)
            {
                // Access token expire hua — check karo RefreshToken DB mein hai ya nahi
                if (!string.IsNullOrEmpty(refreshToken))
                {
                    bool refreshValid = await IsRefreshTokenValidAsync(db, refreshToken);
                    if (refreshValid)
                    {
                        // RefreshTokenMiddleware new token banayega — allow karo
                        await _next(context);
                        return;
                    }
                }
                // RefreshToken bhi nahi ya revoked/expired
                ClearAllCookies(context);
                RedirectToLogin(context);
                return;
            }

            // TokenState.Invalid (tampered / wrong signature)
            ClearAllCookies(context);
            RedirectToLogin(context);
            return;
        }

        // ── 4. AccessToken nahi but RefreshToken hai ────────
        // RefreshTokenMiddleware handle karega (ya token invalid hoga)
        if (!string.IsNullOrEmpty(refreshToken))
        {
            bool refreshValid = await IsRefreshTokenValidAsync(db, refreshToken);
            if (refreshValid)
            {
                await _next(context);
                return;
            }
        }

        // ── 5. Sab fail — clear aur redirect ───────────────
        ClearAllCookies(context);
        RedirectToLogin(context);
    }

    // ── Token Validation ──────────────────────────────────
    private TokenState ValidateToken(string token)
    {
        try
        {
            var key       = _config["Jwt:Key"]      ?? "";
            var issuer    = _config["Jwt:Issuer"]   ?? "";
            var audience  = _config["Jwt:Audience"] ?? "";

            var handler = new JwtSecurityTokenHandler();
            var validationParams = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey         = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key)),
                ValidateIssuer           = true,
                ValidIssuer              = issuer,
                ValidateAudience         = true,
                ValidAudience            = audience,
                ValidateLifetime         = true,
                ClockSkew                = TimeSpan.FromSeconds(30)
            };

            handler.ValidateToken(token, validationParams, out _);
            return TokenState.Valid;
        }
        catch (SecurityTokenExpiredException)
        {
            return TokenState.Expired;
        }
        catch
        {
            return TokenState.Invalid;
        }
    }

    // ── Refresh Token DB Check ────────────────────────────
    private static async Task<bool> IsRefreshTokenValidAsync(
        LibmanagementContext db, string refreshToken)
    {
        try
        {
            var stored = await db.RefreshTokens
                .AsNoTracking()
                .FirstOrDefaultAsync(t =>
                    t.Token      == refreshToken &&
                    t.IsRevoked  == false        &&
                    t.ExpiresAt  >  DateTime.Now);

            if (stored == null) return false;

            // User active hona chahiye
            var user = await db.UserMasters
                .AsNoTracking()
                .FirstOrDefaultAsync(u =>
                    u.UserId   == stored.UserId &&
                    u.IsActive == true);

            return user != null;
        }
        catch
        {
            return false;
        }
    }

    // ── Helpers ───────────────────────────────────────────
    private static bool IsPublicPath(string path)
    {
        // Exact match
        if (_publicPaths.Contains(path))
            return true;

        // Prefix match (static files)
        foreach (var prefix in _publicPrefixes)
            if (path.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
                return true;

        return false;
    }

    private static void RedirectToLogin(HttpContext context)
    {
        // AJAX requests ko JSON response do
        if (context.Request.Headers["X-Requested-With"] == "XMLHttpRequest" ||
            context.Request.Headers["Accept"].ToString().Contains("application/json"))
        {
            context.Response.StatusCode  = 401;
            context.Response.ContentType = "application/json";
            context.Response.WriteAsync("{\"success\":false,\"message\":\"Session expired. Please login again.\",\"redirect\":\"/Account/Login\"}");
            return;
        }

        context.Response.Redirect("/Home/Index");
    }

    private static void ClearAllCookies(HttpContext context)
    {
        foreach (var cookie in context.Request.Cookies.Keys)
        {
            context.Response.Cookies.Delete(cookie, new CookieOptions
            {
                Secure   = true,
                HttpOnly = true,
                SameSite = SameSiteMode.Strict
            });
        }
    }

    private enum TokenState { Valid, Expired, Invalid }
}
