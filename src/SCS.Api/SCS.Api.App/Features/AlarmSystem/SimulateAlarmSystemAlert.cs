using ErrorOr;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using SCS.Api.App.Abstraction.Messaging;
using SCS.Api.App.Abstraction.Routing;
using SCS.Api.App.Extensions;
using SCS.Api.App.Messaging;

namespace SCS.Api.App.Features.AlarmSystem;

public class SimulateAlarmSystemAlert
{
    public class Endpoint : IEndpoint
    {
        public void MapEndpoint(IEndpointRouteBuilder endpoints)
        {
            var group = endpoints.AlarmSystemGroup();
            group.MapPost("/alarm-system/simulate-alert", Handle)
                .WithName("SimulateAlarmSystemAlert")
                .WithTags("Alarm System")
                .Produces(StatusCodes.Status200OK)
                .Produces(StatusCodes.Status400BadRequest)
                .WithOpenApi();
        }

        private static async Task<IResult> Handle(
            [FromBody] Command command,
            [FromServices] IRequestHandler<Command, ErrorOr<Unit>> handler,
            CancellationToken cancellationToken)
        {
            var result = await handler.Handle(command, cancellationToken);
            return result.IsError ? Results.BadRequest(result) : Results.Ok(result.Value);
        }
    }
    public record Command(int PremiseId, string Message) : IRequest<ErrorOr<Unit>> { }

    public class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            RuleFor(x => x.PremiseId).GreaterThan(0).WithMessage("Premise ID must be greater than 0.");
            RuleFor(x => x.Message).NotEmpty().WithMessage("Message cannot be empty.");
        }
    }

    public sealed class Handler : IRequestHandler<Command, ErrorOr<Unit>>
    {
        private readonly IHubContext<AlarmSystemHub> _hubContext;
        private readonly IValidator<Command> _validator;

        public Handler(IHubContext<AlarmSystemHub> hubContext, IValidator<Command> validator)
        {
            ArgumentNullException.ThrowIfNull(hubContext, nameof(hubContext));
            ArgumentNullException.ThrowIfNull(validator, nameof(validator));

            _hubContext = hubContext;
            _validator = validator;
        }

        public async Task<ErrorOr<Unit>> Handle(Command request, CancellationToken cancellationToken)
        {
            var validationResult = await _validator.ValidateAsync(request, cancellationToken);
            if (!validationResult.IsValid)
            {
                return Error.Validation("SimulateAlarmSystemAlert.Validation", "Validation failed");
            }

            // Simulate sending an alert to the specified premise
            await _hubContext.Clients.Group(request.PremiseId.ToString()).SendAsync("ReceiveAlert", request.Message, cancellationToken);

            return Unit.Value;
        }
    }
}
