using MediatR;
using SupportTickets.Application.Common.Dtos;

namespace SupportTickets.Application.Features.Users.Queries.GetUsers;

public class GetUsersQuery : IRequest<List<UserDto>>
{
}
