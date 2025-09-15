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

        // Filtrar el servicio 'ui' de los resultados
        var filteredEntries = report.Entries
            .Where(e => e.Key != "ui")
            .ToDictionary(e => e.Key, e => e.Value);

        var unhealthyChecks = filteredEntries.Where(e => e.Value.Status != HealthStatus.Healthy).ToList();
        var response = new
        {
            status = unhealthyChecks.Any() ? "Degraded" : "Healthy",
            checks = filteredEntries.Select(e => new
            {
                name = e.Key,
                status = e.Value.Status.ToString(),
                description = e.Value.Exception != null ? e.Value.Exception.ToString() : null,
                duration = e.Value.Duration.TotalMilliseconds
            }),
            totalDuration = report.TotalDuration.TotalMilliseconds
        };

        var jsonResponse = JsonSerializer.Serialize(response, new JsonSerializerOptions { WriteIndented = true });

        return unhealthyChecks.Any() ? StatusCode(200, jsonResponse) : Ok(jsonResponse);
    }

    private string GetServiceUrl(string serviceName)
    {
        // Hardcoded IP for the webui_ui service
        if (serviceName == "webui_ui")
        {
            return "http://172.19.0.4:80";
        }
        return string.Empty;
    }
}
