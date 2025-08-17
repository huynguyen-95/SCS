using System;

namespace SCS.Api.App.Services;

public interface ICurrentUserAccessor
{
    string GetUserEmpNo();
}

public class CurrentUserAccessor : ICurrentUserAccessor
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentUserAccessor(IHttpContextAccessor httpContextAccessor)
    {
        ArgumentNullException.ThrowIfNull(httpContextAccessor, nameof(httpContextAccessor));

        _httpContextAccessor = httpContextAccessor;
    }

    public string GetUserEmpNo()
    {
        return _httpContextAccessor.HttpContext?.User?.FindFirst(Constants.AppClaims.EmpNo)?.Value;
    }
}
