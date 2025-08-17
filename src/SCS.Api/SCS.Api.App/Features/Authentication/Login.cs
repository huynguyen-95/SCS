using ErrorOr;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SCS.Api.App.Abstraction.Messaging;
using SCS.Api.App.Abstraction.Routing;
using SCS.Api.App.Extensions;
using SCS.Api.App.Helpers;
using SCS.Api.App.Persistences;

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

    public sealed class Handler(
        ApplicationDbContext context,
        IValidator<Command> validator,
        IJwtTokenGenerator jwtTokenGenerator
    ) : IRequestHandler<Command, ErrorOr<AuthenticationResponse>>
    {
        private readonly ApplicationDbContext _context = context;
        private readonly IValidator<Command> _validator = validator;
        private readonly IJwtTokenGenerator _jwtTokenGenerator = jwtTokenGenerator;

        public async Task<ErrorOr<AuthenticationResponse>> Handle(Command request, CancellationToken cancellationToken)
        {
            var validationResult = _validator.Validate(request);
            if (!validationResult.IsValid)
            {
                return Error.Validation("Login.Validation", validationResult.Errors.Select(e => e.ErrorMessage).FirstOrDefault() ?? "Validation failed.");
            }

            var user = await _context.Users.FirstOrDefaultAsync(x => x.EmpNo == request.EmpNo, cancellationToken);
            if (user == null)
            {
                return Error.NotFound("Login.UserNotFound", "User not found.");
            }

            var token = _jwtTokenGenerator.GenerateToken(user);
            return new AuthenticationResponse(token);
        }
    }
}
