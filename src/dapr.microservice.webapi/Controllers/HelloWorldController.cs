using Microsoft.AspNetCore.Mvc;
using Dapr;

namespace dapr.microservice.webapi.Controllers;

[ApiController]
[Route("[controller]")]
public class HelloController : ControllerBase
{
    private readonly ILogger<HelloController> _logger;

    public HelloController(ILogger<HelloController> logger)
    {
        _logger = logger;
    }

    [HttpGet()]
    public ActionResult<string> Get(string value)
    {
        Console.WriteLine("Hello, World.");
        _logger.LogInformation("Hello, World.");
        return "Received: " + value;
    }
}
