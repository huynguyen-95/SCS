using Microsoft.AspNetCore.SignalR;
using SCS.Api.App.Abstraction.Messaging;
using SCS.Api.App.Features.Premise;

namespace SCS.Api.App.Messaging;

public class AlarmSystemHub : Hub
{
    private readonly IRequestHandler<GetPremiseList.Query, IEnumerable<GetPremiseList.PremiseDto>> _getPremiseListHandler;

    public AlarmSystemHub(IRequestHandler<GetPremiseList.Query, IEnumerable<GetPremiseList.PremiseDto>> getPremiseListHandler)
    {
        ArgumentNullException.ThrowIfNull(getPremiseListHandler, nameof(getPremiseListHandler));

        _getPremiseListHandler = getPremiseListHandler;
    }

    public override async Task OnConnectedAsync()
    {
        // Get "groupId" from query string
        var httpContext = Context.GetHttpContext();
        var cancellationToken = httpContext?.RequestAborted ?? CancellationToken.None;
        var groupId = httpContext.Request.Query["groupId"].ToString();

        if (string.IsNullOrEmpty(groupId))
        {
            return;
        }

        var getPremiseList = await _getPremiseListHandler.Handle(new GetPremiseList.Query(), cancellationToken);
        if (!getPremiseList.Any())
        {
            return;
        }

        if (!getPremiseList.Any(p => p.Id.ToString() == groupId))
        {
            return;
        }

        await Groups.AddToGroupAsync(Context.ConnectionId, groupId);

        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception exception)
    {
        var httpContext = Context.GetHttpContext();
        var groupId = httpContext.Request.Query["groupId"].ToString();

        if (!string.IsNullOrEmpty(groupId))
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, groupId);
        }

        await base.OnDisconnectedAsync(exception);
    }
}
