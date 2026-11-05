using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartPalm.IngestionService.Application.Contracts;
using SmartPalm.IngestionService.Application.Services;
using SmartPalm.IngestionService.Domain.Model.Queries;

namespace SmartPalm.IngestionService.Api.Controllers;

[ApiController, Authorize, Route("api/v1/devices")]
public class DeviceSensorReadingsController(IngestionQueryService queries) : ControllerBase
{
    [HttpGet("{deviceMac}/sensor-readings")]
    public async Task<ActionResult<IReadOnlyList<SensorReadingResponse>>> GetDeviceSensorReadings(
        string deviceMac, [FromQuery] DateTime? from, [FromQuery] DateTime? to,
        [FromQuery] int page = 1, [FromQuery] int size = 100, CancellationToken token = default) =>
        Ok(await queries.HandleAsync(new GetDeviceReadingsQuery(deviceMac, from ?? DateTime.MinValue, to ?? DateTime.MaxValue, page, size), token));
}
