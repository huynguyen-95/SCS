using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.Extensions.Caching.Memory;
using SCS.Api.App.Abstraction.Messaging;
using SCS.Api.App.Abstraction.Routing;
using SCS.Api.App.Extensions;

namespace SCS.Api.App.Features.Premise;

public class GetPremiseList
{
    public class Endpoint : IEndpoint
    {
        public void MapEndpoint(IEndpointRouteBuilder app)
        {
            var group = app.PremiseGroup();
            group.MapGet("", HandleAsync)
                .WithName("GetPremiseList")
                .WithSummary("Retrieves a list of premises.")
                .Produces<IEnumerable<PremiseDto>>()
                .WithTags("Premise")
                .WithDescription("This endpoint retrieves a list of premises from the cache or database if not cached.");
        }

        private async Task<Ok<IEnumerable<PremiseDto>>> HandleAsync(
            IMemoryCache cache,
            CancellationToken cancellationToken)
        {
            var handler = new Handler(cache);
            var result = await handler.Handle(new Query(), cancellationToken);
            return TypedResults.Ok(result);
        }
    }

    public record Query : IRequest<IEnumerable<PremiseDto>>;

    public record PremiseDto(int Id, string Name);

    public sealed class Handler(IMemoryCache cache) : IRequestHandler<Query, IEnumerable<PremiseDto>>
    {
        private const string CacheKey = "PremiseList";

        private readonly IMemoryCache _cache = cache;

        private readonly IEnumerable<PremiseDto> _data = new List<PremiseDto>
        {
            new(1, "Premise 1"),
            new(2, "Premise 2"),
        };

        public async Task<IEnumerable<PremiseDto>> Handle(Query request, CancellationToken cancellationToken)
        {
            var cacheValue = await _cache.GetOrCreateAsync(CacheKey, entry =>
            {
                entry.SetAbsoluteExpiration(DateTimeOffset.UtcNow.AddMinutes(5));
                return Task.FromResult(_data);
            });

            return cacheValue ?? [];
        }
    }
}
