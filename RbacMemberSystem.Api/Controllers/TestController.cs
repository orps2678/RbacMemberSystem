namespace RbacMemberSystem.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TestController : ControllerBase
{
    private readonly ILogger<TestController> _logger;

    // 建構子注入logger
    public TestController(ILogger<TestController> logger)
    {
        _logger = logger;
    }

    [HttpGet]
    public IActionResult Get()
    {
        _logger.LogInformation("GET /api/test was called at {Time}", DateTime.Now);
        return Ok(new { message = "Hello from ASP.NET Core!", timestamp = DateTime.Now });
    }
}