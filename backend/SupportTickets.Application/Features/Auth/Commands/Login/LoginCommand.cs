using MediatR;
using SupportTickets.Application.Common.Dtos;

namespace SupportTickets.Application.Features.Auth.Commands.Login;

public class LoginCommand : IRequest<LoginResultDto>
{
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}

public class LoginResultDto
{
    public string Token { get; set; } = string.Empty;
    public UserDto User { get; set; } = null!;
}
