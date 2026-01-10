using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace RbacMemberSystem.Api.Services;

public class JwtService(IOptions<JwtSettings> options)  // 注入設定
{
    private readonly JwtSettings _settings = options.Value; // 取出設定值

    public string GenerateToken(User user, List<string> roles) 
    {
        // 建立 Claims（Token 裡要放的資訊）
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),   // 使用者ID
            new Claim(ClaimTypes.Name, user.Username),                  // 使用者名稱
            new Claim(ClaimTypes.Email, user.Email)                     // Email
        };

        // 加入角色Claims
        foreach (var role in roles)
        {
            // ClaimTypes.Role = 角色，[Authorize(Roles = "Admin")] 會用到
            claims.Add(new Claim(ClaimTypes.Role, role));               // 角色
        }

        // 建立簽名金鑰：把 Secret 字串轉成 byte[]，再建立對稱金鑰
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_settings.Secret));

        // 用 HmacSha256 演算法簽名
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        // 建立Token
        var token = new JwtSecurityToken(
            issuer: _settings.Issuer,                                       // 發行者（誰發的）
            audience: _settings.Audience,                                   // 接收者（給誰用）
            claims: claims,                                                 // 使用者資訊
            expires: DateTime.UtcNow.AddMinutes(_settings.ExpiryMinutes),   // 過期時間
            signingCredentials: credentials                                 // 簽名
        );

        // 回傳 Token 字串
        return new JwtSecurityTokenHandler().WriteToken(token);
        // 回傳類似：eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIi...
    }

    public DateTime GetExpiryTime()
    {
        return DateTime.UtcNow.AddMinutes(_settings.ExpiryMinutes);
    }
}