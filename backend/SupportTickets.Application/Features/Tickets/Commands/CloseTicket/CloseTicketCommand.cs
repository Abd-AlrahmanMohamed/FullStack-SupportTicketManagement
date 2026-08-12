using MediatR;
using SupportTickets.Application.Common.Dtos;

namespace SupportTickets.Application.Features.Tickets.Commands.CloseTicket;

public class CloseTicketCommand : IRequest<TicketDto>
{
    public Guid TicketId { get; set; }
}
