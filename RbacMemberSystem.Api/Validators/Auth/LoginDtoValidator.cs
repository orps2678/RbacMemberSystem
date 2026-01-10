namespace RbacMemberSystem.Api.Validators.Auth;

public class LoginDtoValidator : AbstractValidator<LoginDto>
{
    public LoginDtoValidator()
    {
        RuleFor(x => x.Username)
        .NotEmpty().WithMessage("使用者名稱為必填");

        RuleFor(x => x.Password)
        .NotEmpty().WithMessage("密碼為必填");
    }
}