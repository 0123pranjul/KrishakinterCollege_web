/// <summary>
/// SecurityHeadersMiddleware — har HTTP response par security headers inject karta hai.
///
/// Headers jo add hote hain:
///   1. X-Frame-Options          → Clickjacking se bachao (DENY)
///   2. X-Content-Type-Options   → MIME sniffing band karo (nosniff)
///   3. X-XSS-Protection         → Legacy browser XSS filter enable karo
///   4. Referrer-Policy          → Referrer info limit karo
///   5. Permissions-Policy       → Sensitive browser APIs disable karo
///   6. Content-Security-Policy  → Trusted sources se hi JS/CSS load ho
/// </summary>
public class SecurityHeadersMiddleware
{
    private readonly RequestDelegate _next;
    private readonly IWebHostEnvironment _env;

    public SecurityHeadersMiddleware(RequestDelegate next, IWebHostEnvironment env)
    {
        _next = next;
        _env  = env;
    }

    public async Task Invoke(HttpContext context)
    {
        var headers = context.Response.Headers;

        // ── 1. X-Frame-Options ─────────────────────────────────────────────────
        headers["X-Frame-Options"] = "DENY";

        // ── 2. X-Content-Type-Options ──────────────────────────────────────────
        headers["X-Content-Type-Options"] = "nosniff";

        // ── 3. X-XSS-Protection ────────────────────────────────────────────────
        headers["X-XSS-Protection"] = "1; mode=block";

        // ── 4. Referrer-Policy ─────────────────────────────────────────────────
        headers["Referrer-Policy"] = "strict-origin-when-cross-origin";

        // ── 5. Permissions-Policy ──────────────────────────────────────────────
        headers["Permissions-Policy"] =
            "camera=(), microphone=(), geolocation=(), payment=(), usb=(), magnetometer=(), accelerometer=()";

        // ── 6. Content-Security-Policy ─────────────────────────────────────────
        //
        // Project mein use hone wale CDN sources:
        //   Bootstrap CSS/JS  : cdn.jsdelivr.net
        //   Bootstrap Icons   : cdn.jsdelivr.net
        //   Font Awesome      : cdnjs.cloudflare.com
        //   DataTables        : cdn.datatables.net
        //   SweetAlert2       : cdn.jsdelivr.net
        //   Toastr            : cdnjs.cloudflare.com
        //   Chart.js etc      : cdn.jsdelivr.net / cdnjs.cloudflare.com
        //
        // connect-src: Development mein BrowserLink + Hot Reload WebSocket allow karna
        // zarooori hai, Production mein ye nahi chahiye

        // Development mein VS BrowserLink aur Hot Reload ke liye extra sources
        var connectSrc = _env.IsDevelopment()
            ? "connect-src 'self' ws: wss: http://localhost:* https://localhost:* " +
              "https://cdn.jsdelivr.net https://cdnjs.cloudflare.com https://cdn.datatables.net " +
              "https://*.tile.openstreetmap.org https://*.basemaps.cartocdn.com; "
            : "connect-src 'self' https: " +
              "https://*.tile.openstreetmap.org https://*.basemaps.cartocdn.com; ";

        headers["Content-Security-Policy"] =
            "default-src 'self'; " +

            // ── Scripts ────────────────────────────────────────────────────────
            // self + inline (sidebar toggle, onclick handlers) +
            // CDN sources jahan se JS load hoti hai
            "script-src 'self' 'unsafe-inline' 'unsafe-eval' " +
                "https://cdn.jsdelivr.net " +          // Bootstrap JS, SweetAlert2
                "https://cdnjs.cloudflare.com " +       // Toastr, Font Awesome
                "https://cdn.datatables.net " +        // DataTables JS
                "https://cdn.ckeditor.com; " +          // CKEditor

            // ── Styles ─────────────────────────────────────────────────────────
            "style-src 'self' 'unsafe-inline' " +
                "https://cdn.jsdelivr.net " +           // Bootstrap CSS, Bootstrap Icons
                "https://cdnjs.cloudflare.com " +       // Font Awesome CSS, Toastr CSS
                "https://cdn.datatables.net " +        // DataTables Bootstrap5 CSS
                "https://cdn.ckeditor.com " +           // CKEditor CSS
                "https://fonts.googleapis.com; " +      // Google Fonts

            // ── Fonts ──────────────────────────────────────────────────────────
            "font-src 'self' " +
                "https://cdnjs.cloudflare.com " +       // Font Awesome fonts
                "https://cdn.jsdelivr.net " +
                "https://fonts.gstatic.com " +          // Google Fonts
                "data:; " +                             // base64 embedded fonts

            // ── Images ─────────────────────────────────────────────────────────
            // Map tiles ke liye OpenStreetMap, CartoDB aur jsDelivr allow karo
            "img-src 'self' data: blob: " +
                "https://*.tile.openstreetmap.org " +   // OpenStreetMap tiles
                "https://*.basemaps.cartocdn.com " +    // CartoDB tiles
                "https://cdn.jsdelivr.net " +           // OL icons/markers
                "https://api.qrserver.com " +           // QR Code Generation
                "https://cdn.datatables.net; " +        // DataTables sort icons

            // ── Connect (XHR/fetch/WebSocket/Source Maps) ──────────────────────
            // Development: BrowserLink (localhost:50813) + Hot Reload (ws://localhost)
            // Production: sirf same-origin + HTTPS
            connectSrc +

            // ── Forms ──────────────────────────────────────────────────────────
            "form-action 'self'; " +

            // ── Frames ─────────────────────────────────────────────────────────
            "frame-src 'self' https://www.google.com/ https://maps.google.com/; " +
            "frame-ancestors 'none'; " +

            // ── HTTPS enforce ──────────────────────────────────────────────────
            "upgrade-insecure-requests;";

        await _next(context);
    }
}
