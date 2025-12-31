namespace RbacMemberSystem.Api.DTOs.Auth;

public record AuthResponseDto(
    int Id,
    string Username,
    string Email,
    string Message
);