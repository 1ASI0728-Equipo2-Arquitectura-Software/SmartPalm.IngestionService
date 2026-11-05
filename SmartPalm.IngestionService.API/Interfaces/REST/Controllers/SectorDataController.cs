using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartPalm.IngestionService.Application.Contracts;
using SmartPalm.IngestionService.Application.Services;
using SmartPalm.IngestionService.Domain.Model.Queries;

namespace SmartPalm.IngestionService.Api.Controllers;

[ApiController, Authorize, Route("api/v1/sectors")]
public class SectorDataController(IngestionQueryService queries) : ControllerBase
{
    [HttpGet("{sectorId:long}/health")]
    public async Task<ActionResult<SectorHealthResponse>> GetSectorHealth(long sectorId, CancellationToken token)
    {
        var result = await queries.HandleAsync(new GetSectorHealthQuery(sectorId), token);
        return result is null ? NotFound(new { message = "Sector not found or has no IoT device assigned." }) : Ok(result);
    }

    [HttpGet("{sectorId:long}/sensor-readings")]
    public async Task<ActionResult<IReadOnlyList<SensorReadingResponse>>> GetSectorSensorReadings(
        long sectorId, [FromQuery] DateTime? from, [FromQuery] DateTime? to, CancellationToken token) =>
        Ok(await queries.HandleAsync(new GetSectorReadingsQuery(sectorId, from ?? DateTime.MinValue, to ?? DateTime.MaxValue), token));
}
