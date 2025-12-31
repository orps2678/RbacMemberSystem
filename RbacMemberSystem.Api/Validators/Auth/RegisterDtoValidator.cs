namespace RbacMemberSystem.Api.Validators.Auth;

public class RegisterDtoValidator : AbstractValidator<RegisterDto>
{
    public RegisterDtoValidator()
    {
        RuleFor(x => x.Username)
            .NotEmpty().WithMessage("使用者名稱為必填")
            .Length(3, 30).WithMessage("使用者名稱長度需在3-50字元之間");

        RuleFor(X => X.Email)
            .NotEmpty().WithMessage("Email為必填")
            .EmailAddress().WithMessage("Email格式不正確");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("密碼為必填")
            .MinimumLength(8).WithMessage("密碼至少需要8個字元");

        RuleFor(x => x.ConfirmPassword)
            .Equal(x => x.Password).WithMessage("確認密碼與密碼不一致");
    }
}