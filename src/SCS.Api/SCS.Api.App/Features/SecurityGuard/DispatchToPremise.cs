using ErrorOr;
using FluentValidation;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using SCS.Api.App.Abstraction.Messaging;
using SCS.Api.App.Abstraction.Routing;
using SCS.Api.App.Extensions;
using SCS.Api.App.Services;

namespace SCS.Api.App.Features.SecurityGuard;

public static class DispatchToPremise
{
    public class Endpoint : IEndpoint
    {
        public void MapEndpoint(IEndpointRouteBuilder app)
        {
            var group = app.SecurityGuardGroup();

            group.MapPost("dispatch-to-premise", HandleAsync)
                .WithName("DispatchToPremise")
                .WithTags("SecurityGuard")
                .Produces<ErrorOr<Unit>>(StatusCodes.Status200OK)
                .Produces<ErrorOr<Unit>>(StatusCodes.Status400BadRequest);
        }

        private async Task<Results<Ok, BadRequest>> HandleAsync(
            [FromBody] Command command,
            IRequestHandler<Command, ErrorOr<Unit>> handler,
            CancellationToken cancellationToken)
        {
            var result = await handler.Handle(command, cancellationToken);

            return result.IsError ? TypedResults.BadRequest() : TypedResults.Ok();
        }
    }

    public record Command(int PremiseId, string GuardEmail) : IRequest<ErrorOr<Unit>> { }

    public class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            RuleFor(x => x.PremiseId).GreaterThan(0).WithMessage("Premise ID must be greater than 0.");
            RuleFor(x => x.GuardEmail).NotEmpty().EmailAddress().WithMessage("Guard email must be a valid email address.");
        }
    }

    public sealed class Handler : IRequestHandler<Command, ErrorOr<Unit>>
    {
        private readonly IEmailService _emailService;
        private readonly IValidator<Command> _validator;

        public Handler(IEmailService emailService, IValidator<Command> validator)
        {
            ArgumentNullException.ThrowIfNull(emailService);
            ArgumentNullException.ThrowIfNull(validator);

            _emailService = emailService;
            _validator = validator;
        }

        public async Task<ErrorOr<Unit>> Handle(Command request, CancellationToken cancellationToken)
        {
            var validationResult = _validator.Validate(request);
            if (!validationResult.IsValid)
            {
                return Error.Validation("DispatchToPremise.Validation", "Validation failed");
            }

            // Simulate sending an email to the security guard
            await _emailService.SendEmailAsync(request.GuardEmail, "Dispatch Notification", $"You have been dispatched to premise ID {request.PremiseId}");

            return Unit.Value;
        }
    }
}
