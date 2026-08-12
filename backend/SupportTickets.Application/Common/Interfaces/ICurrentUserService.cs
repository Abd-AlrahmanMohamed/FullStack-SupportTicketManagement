using SupportTickets.Domain.Enums;

namespace SupportTickets.Application.Common.Interfaces;

public interface ICurrentUserService
{
    Guid? UserId { get; }
    string? Email { get; }
    UserRole? Role { get; }
}
