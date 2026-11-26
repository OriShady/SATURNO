namespace NoSqlU.Api.Models;

internal enum ApiOutcome { Ok, Created, NoContent, BadRequest, NotFound, Conflict, Unavailable, Error }

internal sealed record ApiResult(ApiOutcome Outcome, object? Body = null, int? Id = null)
{
    public static ApiResult Ok(object body) => new(ApiOutcome.Ok, body);
    public static ApiResult Created(int id, object body) => new(ApiOutcome.Created, body, id);
    public static ApiResult NoContent() => new(ApiOutcome.NoContent);
    public static ApiResult BadRequest(object body) => new(ApiOutcome.BadRequest, body);
    public static ApiResult NotFound(object? body = null) => new(ApiOutcome.NotFound, body);
    public static ApiResult Conflict(object body) => new(ApiOutcome.Conflict, body);
}
