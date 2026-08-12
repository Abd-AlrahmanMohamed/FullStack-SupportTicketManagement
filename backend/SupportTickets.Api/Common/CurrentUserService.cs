using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using SupportTickets.Application.Common.Interfaces;
using SupportTickets.Domain.Enums;

namespace SupportTickets.Api.Common;

public class CurrentUserService : ICurrentUserService
{
    private readonly ClaimsPrincipal? _user;

    public CurrentUserService(IHttpContextAccessor httpContextAccessor)
    {
        _user = httpContextAccessor.HttpContext?.User;
    }

    public Guid? UserId
    {
        get
        {
            var value = _user?.FindFirstValue(ClaimTypes.NameIdentifier);
            return Guid.TryParse(value, out var id) ? id : null;
        }
    }

    public string? Email => _user?.FindFirstValue(ClaimTypes.Email);

    public UserRole? Role
    {
        get
        {
            var value = _user?.FindFirstValue(ClaimTypes.Role);
            return Enum.TryParse<UserRole>(value, true, out var role) ? role : null;
        }
    }
}
