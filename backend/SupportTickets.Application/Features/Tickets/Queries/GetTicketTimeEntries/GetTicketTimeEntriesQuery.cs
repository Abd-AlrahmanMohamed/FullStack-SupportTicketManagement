using MediatR;
using SupportTickets.Application.Common.Dtos;

namespace SupportTickets.Application.Features.Tickets.Queries.GetTicketTimeEntries;

public class GetTicketTimeEntriesQuery : IRequest<List<TimeEntryDto>>
{
    public Guid TicketId { get; set; }
}
