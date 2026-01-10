namespace RbacMemberSystem.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController(
    AuthService authService,
    IValidator<RegisterDto> registerValidator,
    IValidator<LoginDto> loginValidator) : ControllerBase
{
    [HttpPost("register")]
    public async Task<IActionResult> Register(RegisterDto dto)
    {
        // 驗證輸入
        var validationResult = await registerValidator.ValidateAsync(dto);
        if (!validationResult.IsValid)
        {
            return BadRequest(new
            {
                message = "驗證失敗",
                errors = validationResult.Errors.Select(e => new
                {
                    field = e.PropertyName,
                    error = e.ErrorMessage
                })
            });
        }

        // 執行註冊
        var user = await authService.RegisterAsync(dto);

        if (user == null)
        {
            return Conflict(new { message = "Username 或 Email 已存在" });
        }

        // 回傳成功結果
        var response = new AuthResponseDto(
            Id: user.Id,
            Username: user.Username,
            Email: user.Email,
            Message: "註冊成功"
        );

        return Ok(response);
    }

    // ===登入===
    [HttpPost("login")]
    public async Task<IActionResult> login(LoginDto dto)
    {
        var validationResult = await loginValidator.ValidateAsync(dto);
        if (!validationResult.IsValid)
        {
            return BadRequest(new
            {
                message = "驗證失敗",
                errors = validationResult.Errors.Select(e => new
                {
                    field = e.PropertyName,
                    error = e.ErrorMessage
                })
            });
        }

        var response = await authService.LoginAsync(dto);
        if (response == null)
        {
            return Unauthorized(new { message = "帳號或密碼錯誤" });
        }

        return Ok(response);
    }

    // === 取得個人資料 (需要登入) ===
    [HttpGet("profile")]
    [Authorize]
    public IActionResult GetProfile()
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var username = User.FindFirst(ClaimTypes.Name)?.Value;
        var email = User.FindFirst(ClaimTypes.Email)?.Value;
        var roles = User.FindAll(ClaimTypes.Role).Select(c => c.Value).ToList();

        return Ok(new
        {
            userId,
            username,
            email,
            roles
        });
    }
}