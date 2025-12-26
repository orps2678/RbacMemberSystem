namespace RbacMemberSystem.Api.Data.Configurations;

public class RoleConfiguration : IEntityTypeConfiguration<Role>
{
    public void Configure(EntityTypeBuilder<Role> entity)
    {
        entity.HasKey(e => e.Id);
        
        entity.Property(e => e.Name)
            .IsRequired()
            .HasMaxLength(50);
        
        entity.Property(e => e.Description)
            .HasMaxLength(200);
        
        entity.HasIndex(e => e.Name).IsUnique();
    }
}