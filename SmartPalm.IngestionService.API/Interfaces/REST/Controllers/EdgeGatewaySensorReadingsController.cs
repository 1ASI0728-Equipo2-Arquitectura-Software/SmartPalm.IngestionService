using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartPalm.IngestionService.Api.Resources;
using SmartPalm.IngestionService.Api.Transform;
using SmartPalm.IngestionService.Application.Contracts;
using SmartPalm.IngestionService.Application.Services;
using SmartPalm.IngestionService.Domain.Model.Queries;

namespace SmartPalm.IngestionService.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/v1/edge-gateways")]
public class EdgeGatewaySensorReadingsController(IngestionCommandService commands, IngestionQueryService queries) : ControllerBase
{
    [AllowAnonymous]
    [HttpPost("{gatewayMac}/sensor-readings")]
    public async Task<IActionResult> SubmitSensorReadings(string gatewayMac, ReadDeviceSensorsDataResource resource, CancellationToken token)
    {
        var credential = Request.Headers["X-Gateway-Key"].ToString();
        var result = await commands.IngestAsync(resource.ToCommand(gatewayMac, credential), token);
        return Created($"/api/v1/edge-gateways/{gatewayMac}/sensor-readings", result);
    }

    [HttpGet("{gatewayMac}/sensor-readings")]
    public async Task<ActionResult<IReadOnlyList<SensorReadingResponse>>> GetGatewaySensorReadings(
        string gatewayMac, [FromQuery] DateTime? from, [FromQuery] DateTime? to,
        [FromQuery] string? deviceMac, [FromQuery] int page = 1, [FromQuery] int size = 100,
        CancellationToken token = default) => Ok(await queries.HandleAsync(new GetGatewayReadingsQuery(
            gatewayMac, from ?? DateTime.MinValue, to ?? DateTime.MaxValue, deviceMac, page, size), token));
}
