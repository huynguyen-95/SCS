using ErrorOr;
using FluentValidation;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using SCS.Api.App.Abstraction.Messaging;
using SCS.Api.App.Abstraction.Routing;
using SCS.Api.App.Extensions;
using SCS.Api.App.Persistences;
using SCS.Api.App.Services;

namespace SCS.Api.App.Features.SecurityGuard;

public static class CaptureIncident
{
    public class Endpoint : IEndpoint
    {
        public void MapEndpoint(IEndpointRouteBuilder app)
        {
            var group = app.SecurityGuardGroup();
            group.MapPost("/incidents/{premiseId:int}", HandleAsync)
                .WithName("CaptureIncident")
                .WithSummary("Capture an incident for a premise")
                .WithDescription("Captures an incident with a file upload and stores it in the database.")
                .Produces<Ok>()
                .Produces<BadRequest>()
                .DisableAntiforgery()
                .WithOpenApi();
        }

        private async Task<Results<BadRequest, Ok>> HandleAsync(
            [FromRoute] int premiseId,
            [FromForm] IFormFile file,
            [FromForm] string description,
            [FromForm] DateTimeOffset incidentDate,
            IRequestHandler<Command, ErrorOr<Unit>> handler,
            CancellationToken cancellationToken
        )
        {
            var command = new Command(premiseId, file, description, incidentDate);
            var result = await handler.Handle(command, cancellationToken);

            return result.IsError ? TypedResults.BadRequest() : TypedResults.Ok();
        }
    }

    public record Command(int PremiseId, IFormFile File, string Description, DateTimeOffset IncidentDate) : IRequest<ErrorOr<Unit>>;

    public class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            RuleFor(x => x.PremiseId).GreaterThan(0).WithMessage("Premise ID must be greater than 0.");
            RuleFor(x => x.File).NotNull().WithMessage("File is required.");
            RuleFor(x => x.Description).NotEmpty().WithMessage("Description cannot be empty.");
        }
    }

    public sealed class Handler(
        ApplicationDbContext context,
        IUploadFileService uploadFileService,
        ICurrentUserAccessor currentUserAccessor,
        IValidator<Command> validator
    ) : IRequestHandler<Command, ErrorOr<Unit>>
    {
        private readonly ApplicationDbContext _context = context;
        private readonly IUploadFileService _uploadFileService = uploadFileService;
        private readonly ICurrentUserAccessor _currentUserAccessor = currentUserAccessor;
        private readonly IValidator<Command> _validator = validator;

        public async Task<ErrorOr<Unit>> Handle(Command request, CancellationToken cancellationToken)
        {
            var validationResult = await _validator.ValidateAsync(request, cancellationToken);
            if (!validationResult.IsValid)
            {
                return Error.Validation("CaptureIncident.Validation", "validation failed");
            }

            var result = await _uploadFileService.UploadFileAsync(request.File, cancellationToken);
            if (result.IsError)
            {
                return Error.Failure("CaptureIncident.UploadFailed", "Failed to upload file.");
            }

            var incident = new Domain.Incident(
                request.PremiseId,
                request.Description,
                request.IncidentDate,
                result.Value,
                _currentUserAccessor.GetUserEmpNo()
            );

            await _context.Incidents.AddAsync(incident, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);

            return Unit.Value;
        }
    }
}
