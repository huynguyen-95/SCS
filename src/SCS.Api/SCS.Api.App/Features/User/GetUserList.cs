using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using SCS.Api.App.Abstraction.Messaging;
using SCS.Api.App.Abstraction.Routing;
using SCS.Api.App.Extensions;
using SCS.Api.App.Persistences;

namespace SCS.Api.App.Features.User;

public static class GetUserList
{
    public class Endpoint : IEndpoint
    {
        public void MapEndpoint(IEndpointRouteBuilder app)
        {
            var group = app.UserGroup();

            group.MapGet("", HandleAsync)
                .WithName("GetUserList")
                .WithSummary("Get a list of users")
                .WithDescription("Retrieves a list of users with their employee number, name, and admin status.")
                .Produces<IEnumerable<UserDto>>(StatusCodes.Status200OK)
                .ProducesProblem(StatusCodes.Status500InternalServerError);
        }

        private async Task<Ok<IEnumerable<UserDto>>> HandleAsync(
            IRequestHandler<Query, IEnumerable<UserDto>> handler,
            CancellationToken cancellationToken)
        {
            var users = await handler.Handle(new Query(), cancellationToken);

            return TypedResults.Ok(users);
        }
    }

    public record UserDto(string EmpNo, string Name, bool IsAdmin);

    public record Query : IRequest<IEnumerable<UserDto>>;

    public class Handler(ApplicationDbContext context) : IRequestHandler<Query, IEnumerable<UserDto>>
    {
        private readonly ApplicationDbContext _context = context;

        public async Task<IEnumerable<UserDto>> Handle(Query request, CancellationToken cancellationToken)
        {
            var users = await _context.Users
                .Select(u => new UserDto(u.EmpNo, u.Username, u.IsAdmin))
                .ToListAsync(cancellationToken);

            return users;
        }
    }
}
