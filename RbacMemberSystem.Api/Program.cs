
using RbacMemberSystem.Api.Data.Seeds;
using Serilog;

// === 設定Serilog ===
Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateLogger();

try
{
    Log.Information("Starting application...");

    var builder = WebApplication.CreateBuilder(args);

    // 使用 Serilog 取代內建的 Logger
    builder.Host.UseSerilog();

    // === 註冊服務 ===
    builder.Services.AddControllers();
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen();

    // === 註冊 DbContext
    builder.Services.AddDbContext<AppDbContext>(options =>
        options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

    // === 加入 FluentValidation
    builder.Services.AddValidatorsFromAssemblyContaining<RegisterDtoValidator>();

    // === 加入 AuthService
    builder.Services.AddScoped<AuthService>();

    var app = builder.Build();

    // === Seed Data ===
    using (var scope = app.Services.CreateScope())
    {
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        await DataSeeder.SeedAsync(context);
    }

    // === 設定 HTTP Pipeline ===
    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI();
    }

    app.UseHttpsRedirection();
    app.UseAuthorization();
    app.MapControllers();

    Log.Information("Application started successfully!");
    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}