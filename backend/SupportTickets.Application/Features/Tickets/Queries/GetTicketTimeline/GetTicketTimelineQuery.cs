using MediatR;
using SupportTickets.Application.Common.Dtos;

namespace SupportTickets.Application.Features.Tickets.Queries.GetTicketTimeline;

public class GetTicketTimelineQuery : IRequest<List<ActivityDto>>
{
    public Guid TicketId { get; set; }
}
