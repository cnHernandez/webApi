using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using System.Text.Json;
using System.Linq;
using System.Threading.Tasks;

[ApiController]
[Route("api/health")]
public class HealthCheckController : ControllerBase
{
    private readonly HealthCheckService _healthCheckService;

    public HealthCheckController(HealthCheckService healthCheckService)
    {
        _healthCheckService = healthCheckService;
    }

    [HttpGet]
    public async Task<IActionResult> CheckHealth()
    {
        var report = await _healthCheckService.CheckHealthAsync();

        var response = new
        {
            status = report.Status.ToString(),
            checks = report.Entries.Select(e => new
            {
                name = e.Key,
                status = e.Value.Status.ToString(),
                description = e.Value.Exception != null ? e.Value.Exception.ToString() : null,
                duration = e.Value.Duration.TotalMilliseconds
            }),
            totalDuration = report.TotalDuration.TotalMilliseconds
        };

        var jsonResponse = JsonSerializer.Serialize(response, new JsonSerializerOptions { WriteIndented = true });

        return report.Status == HealthStatus.Healthy ? Ok(jsonResponse) : StatusCode(503, jsonResponse);
    }
}
