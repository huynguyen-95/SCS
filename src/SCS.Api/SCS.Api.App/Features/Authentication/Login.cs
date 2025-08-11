using ErrorOr;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using SCS.Api.App.Abstraction.Messaging;
using SCS.Api.App.Abstraction.Routing;
using SCS.Api.App.Extensions;

namespace SCS.Api.App.Features.Authentication;

public static class Login
{
    public class Endpoint : IEndpoint
    {
        public void MapEndpoint(IEndpointRouteBuilder app)
        {
            var group = app.AuthenticationGroup();
            group.MapPost("/login", HandleAsync)
                .WithName("Login")
                .WithSummary("User login endpoint")
                .WithDescription("Allows users to log in using their employee number.")
                .Produces(StatusCodes.Status200OK)
                .Produces(StatusCodes.Status400BadRequest)
                .WithTags("Authentication")
                .WithOpenApi(options =>
                {
                    options.OperationId = "LoginUser";
                    options.Description = "Logs in a user with the provided employee number.";
                    return options;
                });
        }

        private async Task<IResult> HandleAsync(
            [FromBody] Command command,
            IRequestHandler<Command, ErrorOr<AuthenticationResponse>> handler,
            CancellationToken cancellationToken)
        {
            var result = await handler.Handle(command, cancellationToken);

            if (result.IsError)
            {
                return Results.BadRequest(result.FirstError);
            }

            return Results.Ok(result.Value);
        }
    }

    public record Command(string EmpNo) : IRequest<ErrorOr<AuthenticationResponse>>;

    public record AuthenticationResponse(string Token);

    public class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            RuleFor(x => x.EmpNo)
                .NotEmpty()
                .WithMessage("Employee number is required.");
        }
    }

    public sealed class Handler(IValidator<Command> validator, IJwtTokenGenerator jwtTokenGenerator) : IRequestHandler<Command, ErrorOr<AuthenticationResponse>>
    {
        private readonly IValidator<Command> _validator = validator;
        private readonly IJwtTokenGenerator _jwtTokenGenerator = jwtTokenGenerator;

        private IEnumerable<Domain.User> _users = [
            new Domain.User("88907299", "Huy")
        ];

        public Task<ErrorOr<AuthenticationResponse>> Handle(Command request, CancellationToken cancellationToken)
        {
            var validationResult = _validator.Validate(request);
            if (!validationResult.IsValid)
            {
                return Task.FromResult<ErrorOr<AuthenticationResponse>>(
                    Error.Validation("Login.Validation", validationResult.Errors.Select(e => e.ErrorMessage).FirstOrDefault() ?? "Validation failed."));
            }

            var user = _users.FirstOrDefault(x => x.EmpNo == request.EmpNo);
            if (user == null)
            {
                return Task.FromResult<ErrorOr<AuthenticationResponse>>(
                    Error.NotFound("Login.UserNotFound", "User not found."));
            }

            var token = _jwtTokenGenerator.GenerateToken(user);
            return Task.FromResult<ErrorOr<AuthenticationResponse>>(new AuthenticationResponse(token));
        }
    }
}
