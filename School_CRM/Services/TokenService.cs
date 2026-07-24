using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using School_CRM.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

public class TokenService
{
    private readonly IConfiguration _config;
    private readonly LibmanagementContext dbContext;

    public TokenService(IConfiguration config, LibmanagementContext dbContext)
    {
        _config = config;
        this.dbContext = dbContext;
    }

    // ACCESS TOKEN
    public string GenerateAccessToken(UserMaster user, string role, int roleId)
    {
        if(roleId== -1)
        {
             roleId = dbContext.RoleMasters
             .Where(x => x.RoleName == role && x.IsActive == true)
             .Select(x => x.RoleId)
             .FirstOrDefault();
        }

        var claims = new[]
        {
            new Claim(ClaimTypes.Name, user.Username),
            new Claim(ClaimTypes.Role, role),
            new Claim("RoleId", roleId.ToString()),
            new Claim("UserId", user.UserId.ToString())
        };

        var key = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(_config["Jwt:Key"]));

        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _config["Jwt:Issuer"],
            audience: _config["Jwt:Audience"],      
            claims: claims,
           expires: DateTime.UtcNow.AddMinutes(
    Convert.ToDouble(_config["Jwt:AccessTokenExpiryMinutes"])),
            signingCredentials: creds);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    // REFRESH TOKEN
    public string GenerateRefreshToken()
    {
        var randomBytes = new byte[64];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomBytes);
        return Convert.ToBase64String(randomBytes);
    }
    public DateTime GetRefreshTokenExpiry()
    {
        return DateTime.Now.AddDays(
            Convert.ToDouble(_config["Jwt:RefreshTokenExpiryDays"]));
    }   
}