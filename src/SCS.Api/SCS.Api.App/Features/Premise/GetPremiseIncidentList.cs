using ErrorOr;
using FluentValidation;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SCS.Api.App.Abstraction.Messaging;
using SCS.Api.App.Abstraction.Routing;
using SCS.Api.App.Extensions;
using SCS.Api.App.Persistences;

namespace SCS.Api.App.Features.Premise;

public static class GetPremiseIncidentList
{
    public class Endpoint : IEndpoint
    {
        public void MapEndpoint(IEndpointRouteBuilder app)
        {
            var group = app.PremiseGroup();

            group.MapGet("/incidents/{premiseId:int}", HandleAsync)
                .WithName("GetPremiseIncidentList")
                .WithSummary("Get incidents for a premise")
                .WithDescription("Retrieves a list of incidents associated with a specific premise.")
                .Produces<Ok<IEnumerable<PremiseIncidentDto>>>()
                .Produces<BadRequest>()
                .WithOpenApi();
        }

        private async Task<Results<BadRequest, Ok<IEnumerable<PremiseIncidentDto>>>> HandleAsync(
            [FromRoute] int premiseId,
            IRequestHandler<Query, ErrorOr<IEnumerable<PremiseIncidentDto>>> handler,
            CancellationToken cancellationToken
        )
        {
            var query = new Query(premiseId);
            var result = await handler.Handle(query, cancellationToken);

            return result.IsError ? TypedResults.BadRequest() : TypedResults.Ok(result.Value);
        }
    }

    public record PremiseIncidentDto(string Description, DateTimeOffset Date, string FilePath);

    public record Query(int PremiseId) : IRequest<ErrorOr<IEnumerable<PremiseIncidentDto>>>;

    public class Validator : AbstractValidator<Query>
    {
        public Validator()
        {
            RuleFor(x => x.PremiseId).GreaterThan(0).WithMessage("Premise ID must be greater than 0.");
        }
    }

    public sealed class Handler(
        ApplicationDbContext context,
        IValidator<Query> validator
    ) : IRequestHandler<Query, ErrorOr<IEnumerable<PremiseIncidentDto>>>
    {
        private readonly ApplicationDbContext _context = context;
        private readonly IValidator<Query> _validator = validator;

        public async Task<ErrorOr<IEnumerable<PremiseIncidentDto>>> Handle(Query request, CancellationToken cancellationToken)
        {
            var validationResult = await _validator.ValidateAsync(request, cancellationToken);
            if (!validationResult.IsValid)
            {
                return Error.Validation("GetPremiseIncidentList.Validation", "validation failed");
            }

            var incidents = await _context.Incidents
                .Where(i => i.PremiseId == request.PremiseId)
                .OrderByDescending(i => i.Date)
                .Select(i => new PremiseIncidentDto(i.Description, i.Date, i.FilePath))
                .ToListAsync(cancellationToken);

            return incidents;
        }
    }
}
