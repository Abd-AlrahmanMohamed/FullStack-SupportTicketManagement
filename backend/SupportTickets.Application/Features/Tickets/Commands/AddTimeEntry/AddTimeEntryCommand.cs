using MediatR;
using SupportTickets.Application.Common.Dtos;

namespace SupportTickets.Application.Features.Tickets.Commands.AddTimeEntry;

public class AddTimeEntryCommand : IRequest<TimeEntryDto>
{
    public Guid TicketId { get; set; }
    public DateTime WorkDate { get; set; }
    public int DurationMinutes { get; set; }
    public string? Description { get; set; }
}
