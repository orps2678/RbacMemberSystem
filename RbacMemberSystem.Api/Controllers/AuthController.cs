namespace RbacMemberSystem.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController(AuthService authService, IValidator<RegisterDto> validator) : ControllerBase
{
    [HttpPost("register")]
    public async Task<IActionResult> Register(RegisterDto dto)
    {
        // 驗證輸入
        var validationResult = await validator.ValidateAsync(dto);
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
}