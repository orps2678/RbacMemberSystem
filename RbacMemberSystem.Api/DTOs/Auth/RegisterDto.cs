namespace RbacMemberSystem.Api.DTOs.Auth;

public record RegisterDto(
    string Username,
    string Email,
    string Password,
    string ConfirmPassword
);