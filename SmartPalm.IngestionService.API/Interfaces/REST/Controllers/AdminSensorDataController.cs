using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartPalm.IngestionService.Api.Resources;
using SmartPalm.IngestionService.Api.Transform;
using SmartPalm.IngestionService.Application.Contracts;
using SmartPalm.IngestionService.Application.Services;
using SmartPalm.IngestionService.Domain.Model.Queries;

namespace SmartPalm.IngestionService.Api.Controllers;

[ApiController, Authorize(Roles = "Administrator"), Route("api/v1/admin/devices")]
public class AdminSensorDataController(IngestionCommandService commands, IngestionQueryService queries) : ControllerBase
{
    [HttpGet("{deviceMac}/thresholds")]
    public async Task<ActionResult<IReadOnlyList<ThresholdResponse>>> GetDeviceThresholds(string deviceMac, CancellationToken token) => Ok(await queries.HandleAdminAsync(new GetDeviceThresholdsQuery(0, deviceMac), token));
    [HttpPatch("{deviceMac}/thresholds")]
    public async Task<IActionResult> UpdateDeviceThreshold(string deviceMac, UpdateAgronomicThresholdResource resource, CancellationToken token) { await commands.UpdateThresholdAsync(resource.ToCommand(deviceMac), token); return Ok(); }
}
