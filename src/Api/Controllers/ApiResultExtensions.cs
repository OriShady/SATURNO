using Microsoft.AspNetCore.Mvc;
using NoSqlU.Api.Models;

namespace NoSqlU.Api.Controllers;

internal static class ApiResultExtensions
{
    public static IActionResult ToActionResult(this ApiResult result, ControllerBase controller, string? getAction = null) =>
        result.Outcome switch
        {
            ApiOutcome.Ok => controller.Ok(result.Body),
            ApiOutcome.Created => controller.CreatedAtAction(getAction, new { id = result.Id }, result.Body),
            ApiOutcome.NoContent => controller.NoContent(),
            ApiOutcome.BadRequest => controller.BadRequest(result.Body),
            ApiOutcome.NotFound => result.Body is null ? controller.NotFound() : controller.NotFound(result.Body),
            ApiOutcome.Conflict => controller.Conflict(result.Body),
            ApiOutcome.Unavailable => controller.StatusCode(503, result.Body),
            ApiOutcome.Error => controller.StatusCode(500, result.Body),
            _ => throw new ArgumentOutOfRangeException(nameof(result))
        };
}
