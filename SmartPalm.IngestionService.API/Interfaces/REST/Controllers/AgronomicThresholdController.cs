using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartPalm.IngestionService.Api.Resources;
using SmartPalm.IngestionService.Api.Transform;
using SmartPalm.IngestionService.Application.Contracts;
using SmartPalm.IngestionService.Application.Services;
using SmartPalm.IngestionService.Domain.Model.Queries;

namespace SmartPalm.IngestionService.Api.Controllers;

[ApiController, Authorize(Roles = "Administrator,Agronomist"), Route("api/v1/devices")]
public class AgronomicThresholdController(IngestionCommandService commands, IngestionQueryService queries) : ControllerBase
{
    [HttpGet("{deviceMac}/thresholds")]
    public async Task<ActionResult<IReadOnlyList<ThresholdResponse>>> GetAgronomicThreshold(string deviceMac, CancellationToken token) =>
        Ok(await queries.HandleAsync(new GetDeviceThresholdsQuery(OwnerId(), deviceMac), token));

    [HttpPatch("{deviceMac}/thresholds")]
    public async Task<IActionResult> UpdateAgronomicThreshold(string deviceMac, UpdateAgronomicThresholdResource resource, CancellationToken token)
    {
        await commands.UpdateThresholdAsync(resource.ToCommand(deviceMac), OwnerId(), token); return Ok();
    }

    private long OwnerId() => long.TryParse(User.FindFirst("sub")?.Value, out var ownerId) && ownerId > 0
        ? ownerId : throw new UnauthorizedAccessException("JWT subject is required.");
}
