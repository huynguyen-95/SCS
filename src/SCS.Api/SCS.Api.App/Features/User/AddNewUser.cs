using ErrorOr;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SCS.Api.App.Abstraction.Messaging;
using SCS.Api.App.Abstraction.Routing;
using SCS.Api.App.Extensions;
using SCS.Api.App.Persistences;

namespace SCS.Api.App.Features.User;

public static class AddNewUser
{
    public class Endpoint : IEndpoint
    {
        public void MapEndpoint(IEndpointRouteBuilder app)
        {
            var group = app.UserGroup();

            group.MapPost("", HandleAsync)
                .WithName("AddNewUser")
                .WithSummary("Add a new user")
                .WithDescription("Creates a new user with the provided employee number, username, and admin status.")
                .Accepts<Command>("application/json")
                .ProducesProblem(StatusCodes.Status200OK)
                .ProducesProblem(StatusCodes.Status400BadRequest)
                .ProducesProblem(StatusCodes.Status500InternalServerError);
        }

        private async Task<Results<BadRequest, Ok>> HandleAsync(
            [FromBody] Command command,
            IRequestHandler<Command, ErrorOr<Unit>> handler,
            CancellationToken cancellationToken)
        {
            var result = await handler.Handle(command, cancellationToken);

            return result.IsError ? TypedResults.BadRequest() : TypedResults.Ok();
        }
    }

    public record Command(string EmpNo, string Username, bool IsAdmin) : IRequest<ErrorOr<Unit>>;

    public class Handler(ApplicationDbContext context) : IRequestHandler<Command, ErrorOr<Unit>>
    {
        private readonly ApplicationDbContext _context = context;

        public async Task<ErrorOr<Unit>> Handle(Command request, CancellationToken cancellationToken)
        {
            var isExisted = await _context.Users.AnyAsync(u => u.EmpNo == request.EmpNo, cancellationToken);
            if (isExisted)
            {
                return Error.Validation("AddNewUser.UserAlreadyExists", "A user with this employee number already exists.");
            }

            var user = new Domain.User(
                empNo: request.EmpNo,
                username: request.Username,
                isAdmin: request.IsAdmin);

            await _context.Users.AddAsync(user, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);

            return Unit.Value;
        }
    }
}
