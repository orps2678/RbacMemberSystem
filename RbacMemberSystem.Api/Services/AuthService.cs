using Microsoft.AspNetCore.Identity;

namespace RbacMemberSystem.Api.Services;

public class AuthService(AppDbContext context, ILogger<AuthService> logger)
{
    private readonly PasswordHasher<User> _passwordHasher = new();

    public async Task<User?> RegisterAsync(RegisterDto dto)
    {
        // 檢查 Username 是否已存在
        if (await context.Users.AnyAsync(u => u.Username == dto.Username))
        {
            logger.LogWarning("註冊失敗：Username {Username} 已存在", dto.Username);
            return null;
        }

        // 檢查 Email 是否已存在
        if (await context.Users.AnyAsync(u => u.Email == dto.Email))
        {
            logger.LogWarning("註冊失敗：Email {Email} 已存在", dto.Email);
            return null;
        }

        // 建立新使用者
        var user = new User
        {
            Username = dto.Username,
            Email = dto.Email
        };

        // 密碼加密
        user.PasswordHash = _passwordHasher.HashPassword(user, dto.Password);

        // 儲存到資料庫
        await context.Users.AddAsync(user);
        await context.SaveChangesAsync();

        // 指派預設角色 (User)
        var defaultRole = await context.Roles.FirstOrDefaultAsync(r => r.Name == "User");
        if (defaultRole != null)
        {
            var userRole = new UserRole
            {
                UserId = user.Id,
                RoleId = defaultRole.Id
            };
            await context.UserRoles.AddAsync(userRole);
            await context.SaveChangesAsync();
        }

        logger.LogInformation("使用者 {Username} 註冊成功", user.Username);
        return user;
    }
}