namespace RbacMemberSystem.Api.Entities;

public class UserRole
{
    public int UserId { get; set; }
    
    public int RoleId { get; set; }
    
    // 導航屬性
    public User User { get; set; } = null!;
    
    public Role Role { get; set; } = null!;
}