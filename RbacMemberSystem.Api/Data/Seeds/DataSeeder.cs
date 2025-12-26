namespace RbacMemberSystem.Api.Data.Seeds;

public static class DataSeeder
{
    public static async Task SeedAsync(AppDbContext context)
    {
        // 如果已經有資料就跳過
        if (await context.Roles.AnyAsync())
        {
            return;
        }

        // === 建立預設角色 ===
        var roles = new List<Role>
        {
            new Role { Name = "Admin", Description = "系統管理員，擁有所有權限" },
            new Role { Name = "Manager", Description = "管理者，可查看使用者資料" },
            new Role { Name = "User", Description = "一般使用者，僅能管理自己的資料" }
        };

        await context.Roles.AddRangeAsync(roles);
        await context.SaveChangesAsync();

        Log.Information("Seed data created: {Count} roles", roles.Count);
    }
}