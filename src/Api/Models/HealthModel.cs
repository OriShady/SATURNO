using NoSqlU.Data;

namespace NoSqlU.Api.Models;

public sealed class HealthModel
{
    private readonly SaturnoStore _db;
    private readonly ILogger<HealthModel> _logger;

    public HealthModel(SaturnoStore db, ILogger<HealthModel> logger)
    {
        _db = db;
        _logger = logger;
    }

    internal async Task<ApiResult> GetAsync()
    {
        try
        {
            var canConnect = await _db.CanConnectAsync();
            if (canConnect) return ApiResult.Ok(new { db = "ok" });
            return new ApiResult(ApiOutcome.Unavailable, new { db = "unavailable" });
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error al verificar la conexión con MongoDB.");
            return new ApiResult(ApiOutcome.Error, new { db = "error" });
        }
    }
}
