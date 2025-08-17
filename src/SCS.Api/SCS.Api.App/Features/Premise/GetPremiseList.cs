using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using SCS.Api.App.Abstraction.Messaging;
using SCS.Api.App.Abstraction.Routing;
using SCS.Api.App.Extensions;
using SCS.Api.App.Persistences;

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
            IRequestHandler<Query, IEnumerable<PremiseDto>> handler,
            CancellationToken cancellationToken)
        {
            var result = await handler.Handle(new Query(), cancellationToken);

            return TypedResults.Ok(result);
        }
    }

    public record Query : IRequest<IEnumerable<PremiseDto>>;

    public record PremiseDto(int Id, string Name);

    public sealed class Handler(
        ApplicationDbContext context,
        IMemoryCache cache
    ) : IRequestHandler<Query, IEnumerable<PremiseDto>>
    {
        private readonly ApplicationDbContext _context = context;

        private readonly IMemoryCache _cache = cache;

        private const string CACHE_KEY = "PremiseList";

        private const int CACHE_DURATION_MINUTES = 30;

        public async Task<IEnumerable<PremiseDto>> Handle(Query request, CancellationToken cancellationToken)
        {
            var cacheValue = await _cache.GetOrCreateAsync(CACHE_KEY, async entry =>
            {
                var data = await _context.Premises
                    .Select(p => new PremiseDto(p.Id, p.Name))
                    .ToListAsync(cancellationToken);

                entry.SetAbsoluteExpiration(DateTimeOffset.UtcNow.AddMinutes(CACHE_DURATION_MINUTES));
                return data;
            });

            return cacheValue ?? [];
        }
    }
}
