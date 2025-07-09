using System.Text.Json.Nodes;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.Extensions.Caching.Distributed;

namespace ProjectTemplate.Web.Endpoints;

public class TestRedisCache : EndpointGroupBase
{
    public override void Map(RouteGroupBuilder group)
    {
        group.MapGet(GetValue);
        group.MapPost(SetValue);
        group.MapDelete(RemoveValue);
    }

    public async Task<Results<Ok<string>, NotFound>> GetValue(IDistributedCache cache, string key, CancellationToken ct)
    {
        var value = await cache.GetStringAsync(key, ct);

        return value is not null ? TypedResults.Ok(value) : TypedResults.NotFound();
    }

    public async Task<Created> SetValue(IDistributedCache cache, string key, JsonValue value, CancellationToken ct)
    {

        var valueToString = value?.ToJsonString() ?? string.Empty;
        await cache.SetStringAsync(key, valueToString, ct);
        return TypedResults.Created();

    }

    public async Task<NoContent> RemoveValue(IDistributedCache cache, string key, CancellationToken ct)
    {
        await cache.RemoveAsync(key, ct);
        return TypedResults.NoContent();
    }


}
