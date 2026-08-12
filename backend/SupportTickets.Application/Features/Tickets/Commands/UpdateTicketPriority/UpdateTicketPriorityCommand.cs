using MediatR;
using SupportTickets.Application.Common.Dtos;

namespace SupportTickets.Application.Features.Tickets.Commands.UpdateTicketPriority;

public class UpdateTicketPriorityCommand : IRequest<TicketDto>
{
    public Guid TicketId { get; set; }
    public string Priority { get; set; } = string.Empty;
}
