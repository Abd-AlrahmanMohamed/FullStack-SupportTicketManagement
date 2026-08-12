using MediatR;
using SupportTickets.Application.Common.Dtos;

namespace SupportTickets.Application.Features.Tickets.Commands.AssignTicket;

public class AssignTicketCommand : IRequest<TicketDto>
{
    public Guid TicketId { get; set; }
    public Guid AgentId { get; set; }
}
