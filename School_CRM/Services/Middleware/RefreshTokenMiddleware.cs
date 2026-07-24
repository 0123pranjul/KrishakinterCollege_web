using Microsoft.EntityFrameworkCore;
using School_CRM.Models;
using System.IdentityModel.Tokens.Jwt;

public class RefreshTokenMiddleware
{
    private readonly RequestDelegate _next;

    public RefreshTokenMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task Invoke(HttpContext context,
                             LibmanagementContext db,
                             TokenService tokenService)
    {
        var accessToken = context.Request.Cookies["AccessToken"];
        var refreshToken = context.Request.Cookies["RefreshToken"];

        if (!string.IsNullOrEmpty(accessToken) &&
            !string.IsNullOrEmpty(refreshToken))
        {
            var handler = new JwtSecurityTokenHandler();

            try
            {
                var jwtToken = handler.ReadJwtToken(accessToken);

                // If Access Token Expired
                if (jwtToken.ValidTo < DateTime.UtcNow)
                {
                    var storedToken = await db.RefreshTokens
                        .FirstOrDefaultAsync(x =>
                            x.Token == refreshToken &&
                            x.IsRevoked == false &&
                            x.ExpiresAt > DateTime.Now);

                    if (storedToken != null)
                    {
                        var user = await db.UserMasters
                            .FirstOrDefaultAsync(x =>
                                x.UserId == storedToken.UserId &&
                                x.IsActive== true);

                        if (user != null)
                        {
                            var role = await db.UserRoleAssigns
                                .Where(x => x.UserId == user.UserId && x.IsActive==true)
                                .Select(x => x.Role.RoleName)
                                .FirstOrDefaultAsync();
                            var roleId = db.UserRoleAssigns
            .Where(x => x.UserId == user.UserId && x.IsActive == true)
            .Select(x => x.RoleId)
            .FirstOrDefault();

                            // 🔹 Generate New Tokens
                            var newAccessToken =
                                tokenService.GenerateAccessToken(user, role, roleId);

                            var newRefreshToken =
                                tokenService.GenerateRefreshToken();

                            // 🔹 Revoke Old Refresh Token
                            storedToken.IsRevoked = true;

                            // 🔹 Save New Refresh Token
                            db.RefreshTokens.Add(new RefreshToken
                            {
                                UserId = user.UserId,
                                Token = newRefreshToken,
                                ExpiresAt = tokenService.GetRefreshTokenExpiry(),
                                IsRevoked = false,
                                CreatedDate = DateTime.Now
                            });

                            await db.SaveChangesAsync();

                            // 🔹 Update Cookies
                            context.Response.Cookies.Append("AccessToken",
                                newAccessToken,
                                new CookieOptions
                                {
                                    HttpOnly = true,
                                    Secure   = true,
                                    SameSite = SameSiteMode.Strict
                                });

                            context.Response.Cookies.Append("RefreshToken",
                                newRefreshToken,
                                new CookieOptions
                                {
                                    HttpOnly = true,
                                    Secure   = true,
                                    SameSite = SameSiteMode.Strict
                                });
                        }
                    }
                }
            }
            catch
            {
                // Invalid token ignore
            }
        }

        await _next(context);
    }
}