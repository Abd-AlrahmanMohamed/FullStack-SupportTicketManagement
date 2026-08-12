using MediatR;
using SupportTickets.Application.Common.Dtos;

namespace SupportTickets.Application.Features.Tickets.Commands.UpdateTicketStatus;

public class UpdateTicketStatusCommand : IRequest<TicketDto>
{
    public Guid TicketId { get; set; }
    public string Status { get; set; } = string.Empty;
}
