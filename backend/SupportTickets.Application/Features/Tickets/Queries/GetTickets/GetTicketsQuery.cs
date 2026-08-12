using MediatR;
using SupportTickets.Application.Common.Dtos;
using SupportTickets.Application.Common.Models;

namespace SupportTickets.Application.Features.Tickets.Queries.GetTickets;

public class GetTicketsQuery : IRequest<PaginatedResult<TicketDto>>
{
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    public string? Status { get; set; }
    public string? Priority { get; set; }
    public Guid? AssignedAgentId { get; set; }
    public string? Search { get; set; }
    public string? SortBy { get; set; }
    public string? SortDirection { get; set; }
}
