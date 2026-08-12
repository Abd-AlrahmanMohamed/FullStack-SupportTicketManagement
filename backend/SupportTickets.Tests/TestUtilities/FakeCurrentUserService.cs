using SupportTickets.Application.Common.Interfaces;
using SupportTickets.Domain.Enums;

namespace SupportTickets.Tests.TestUtilities;

public class FakeCurrentUserService : ICurrentUserService
{
    public Guid? UserId { get; set; }
    public string? Email { get; set; }
    public UserRole? Role { get; set; }
}
