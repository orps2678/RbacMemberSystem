namespace RbacMemberSystem.Api.Entities;

public class Role
{
    public int Id { get; set; }
    
    public string Name { get; set; } = string.Empty;
    
    public string? Description { get; set; }
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    // 導航屬性：一個 Role 有多個 UserRole
    public ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();
}