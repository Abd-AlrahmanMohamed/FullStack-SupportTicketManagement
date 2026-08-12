using MediatR;
using SupportTickets.Application.Common.Dtos;

namespace SupportTickets.Application.Features.Tickets.Queries.GetTicketById;

public class GetTicketByIdQuery : IRequest<TicketDetailsDto>
{
    public Guid TicketId { get; set; }
}
