using System.Text.Json;
using System.Text.Json.Serialization;
using Amazon;
using Amazon.SQS;
using Amazon.SQS.Model;
using ErrorOr;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using SCS.Api.App.Abstraction.Messaging;
using SCS.Api.App.Abstraction.Routing;
using SCS.Api.App.Events;
using SCS.Api.App.Extensions;
using SCS.Api.App.Settings;

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
        private readonly IAmazonSQS _sqsClient;
        private readonly string _queueUrl;
        private readonly IValidator<Command> _validator;

        public Handler(IValidator<Command> validator, IOptions<AwsOptions> awsOptions)
        {
            ArgumentNullException.ThrowIfNull(validator, nameof(validator));
            ArgumentNullException.ThrowIfNull(awsOptions, nameof(awsOptions));

            _validator = validator;
            _queueUrl = awsOptions.Value.QueueUrl;
            _sqsClient = new AmazonSQSClient(
                awsOptions.Value.AccessKey,
                awsOptions.Value.SecretKey,
                RegionEndpoint.GetBySystemName(awsOptions.Value.Region));
        }

        public async Task<ErrorOr<Unit>> Handle(Command request, CancellationToken cancellationToken)
        {
            var validationResult = await _validator.ValidateAsync(request, cancellationToken);
            if (!validationResult.IsValid)
            {
                return Error.Validation("SimulateAlarmSystemAlert.Validation", "Validation failed");
            }

            var @event = new AlarmSystemAlertEvent(request.PremiseId, request.Message);
            var sendMessageRequest = new SendMessageRequest
            {
                QueueUrl = _queueUrl,
                MessageBody = JsonSerializer.Serialize(@event, Constants.DefaultJsonSerializerOptions),
            };
            var response = await _sqsClient.SendMessageAsync(sendMessageRequest, cancellationToken);
            if (response.HttpStatusCode != System.Net.HttpStatusCode.OK)
            {
                return Error.Failure("SimulateAlarmSystemAlert.Failure", "Failed to send message to SQS");
            }

            return Unit.Value;
        }
    }
}
