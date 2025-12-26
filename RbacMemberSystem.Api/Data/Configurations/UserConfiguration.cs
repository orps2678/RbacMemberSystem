namespace RbacMemberSystem.Api.Data.Configurations;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> entity)
    {
        entity.HasKey(e => e.Id);
        
        entity.Property(e => e.Username)
            .IsRequired()
            .HasMaxLength(50);
        
        entity.Property(e => e.Email)
            .IsRequired()
            .HasMaxLength(100);
        
        entity.Property(e => e.PasswordHash)
            .IsRequired()
            .HasMaxLength(256);
        
        entity.HasIndex(e => e.Username).IsUnique();
        entity.HasIndex(e => e.Email).IsUnique();
    }
}