using SupportTickets.Domain.Entities;

namespace SupportTickets.Application.Common.Interfaces;

public interface IJwtTokenService
{
    string GenerateToken(User user);
}
