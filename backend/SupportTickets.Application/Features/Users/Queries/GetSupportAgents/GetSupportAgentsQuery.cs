using MediatR;
using SupportTickets.Application.Common.Dtos;

namespace SupportTickets.Application.Features.Users.Queries.GetSupportAgents;

public class GetSupportAgentsQuery : IRequest<List<UserDto>>
{
}
