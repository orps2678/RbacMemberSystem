namespace RbacMemberSystem.Api.DTOs.Auth;

public record LoginDto(
    string Username,
    string Password
);
