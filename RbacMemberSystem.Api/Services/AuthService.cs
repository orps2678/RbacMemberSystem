using Microsoft.AspNetCore.Identity;

namespace RbacMemberSystem.Api.Services;

public class AuthService(AppDbContext context, JwtService jwtService, ILogger<AuthService> logger)
{
    private readonly PasswordHasher<User> _passwordHasher = new();

    // === 註冊 ===
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

    public async Task<LoginResponseDto?> LoginAsync(LoginDto dto)
    {
        // 查詢使用者，同時仔入角色(Include = JOIN)
        var user = await context.Users
            .Include(u => u.UserRoles)
            .ThenInclude(ur => ur.Role)
            .FirstOrDefaultAsync(u => u.Username == dto.Username);

        if (user == null)
        {
            logger.LogWarning("登入失敗:Username {Username} 不存在", dto.Username);
            return null;
        }

        // 檢查帳號是否啟用
        if (!user.IsActive)
        {
            logger.LogWarning("登入失敗:Username {Username} 帳號已停用", dto.Username);
            return null;
        }

        // 驗證密碼
        var result = _passwordHasher.VerifyHashedPassword(user, user.PasswordHash, dto.Password);
        if (result == PasswordVerificationResult.Failed)
        {
            logger.LogWarning("登入失敗:Username {Username} 密碼錯誤", dto.Username);
            return null;
        }

        // 取得角色列表
        var roles = user.UserRoles.Select(ur => ur.Role.Name).ToList();

        // 產生JWT Token
        var token = jwtService.GenerateToken(user, roles);
        var expiresAt = jwtService.GetExpiryTime();

        logger.LogInformation("使用者 {Username} 登入成功", user.Username);

        return new LoginResponseDto(
            Id: user.Id,
            Username: user.Username,
            Email: user.Email,
            Token: token,
            ExpiresAt: expiresAt
        );
    }
}