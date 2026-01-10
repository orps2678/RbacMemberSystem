namespace RbacMemberSystem.Api.DTOs.Auth;

public record LoginResponseDto(
    int Id,
    string Username,
    string Email,
    string Token,
    DateTime ExpiresAt
);