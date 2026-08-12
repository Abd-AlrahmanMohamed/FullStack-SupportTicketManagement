using AutoMapper;
using AutoMapper.QueryableExtensions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using SupportTickets.Application.Common.Dtos;
using SupportTickets.Application.Common.Interfaces;
using SupportTickets.Application.Common.Models;
using SupportTickets.Application.Common.Services;
using SupportTickets.Domain.Entities;
using SupportTickets.Domain.Enums;

namespace SupportTickets.Application.Features.Tickets.Queries.GetTickets;

public class GetTicketsQueryHandler : IRequestHandler<GetTicketsQuery, PaginatedResult<TicketDto>>
{
    private const int MaxPageSize = 100;

    private readonly IGenericRepository<Ticket> _ticketRepository;
    private readonly ICurrentUserService _currentUser;
    private readonly IMapper _mapper;

    public GetTicketsQueryHandler(
        IGenericRepository<Ticket> ticketRepository,
        ICurrentUserService currentUser,
        IMapper mapper)
    {
        _ticketRepository = ticketRepository;
        _currentUser = currentUser;
        _mapper = mapper;
    }

    public async Task<PaginatedResult<TicketDto>> Handle(GetTicketsQuery request, CancellationToken cancellationToken)
    {
        var page = request.Page < 1 ? 1 : request.Page;
        var pageSize = request.PageSize < 1 ? 10 : Math.Min(request.PageSize, MaxPageSize);

        var query = _ticketRepository.Query().AsNoTracking();

        // Ownership scoping is applied first and every later filter can only narrow it further,
        // so a customer or agent can never widen their view beyond their own tickets.
        query = TicketAccessGuard.ScopeToCurrentUser(query, _currentUser.UserId, _currentUser.Role);

        if (!string.IsNullOrWhiteSpace(request.Status) && Enum.TryParse<TicketStatus>(request.Status, true, out var status))
        {
            query = query.Where(t => t.Status == status);
        }

        if (!string.IsNullOrWhiteSpace(request.Priority) && Enum.TryParse<TicketPriority>(request.Priority, true, out var priority))
        {
            query = query.Where(t => t.Priority == priority);
        }

        if (request.AssignedAgentId.HasValue)
        {
            query = query.Where(t => t.AssignedAgentId == request.AssignedAgentId);
        }

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var search = request.Search.Trim();
            query = query.Where(t =>
                t.TicketNumber.Contains(search) ||
                t.Title.Contains(search) ||
                t.Description.Contains(search));
        }

        var totalCount = await query.CountAsync(cancellationToken);

        query = ApplySort(query, request.SortBy, request.SortDirection);

        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ProjectTo<TicketDto>(_mapper.ConfigurationProvider)
            .ToListAsync(cancellationToken);

        return PaginatedResult<TicketDto>.Create(items, totalCount, page, pageSize);
    }

    private static IQueryable<Ticket> ApplySort(IQueryable<Ticket> query, string? sortBy, string? sortDirection)
    {
        var descending = string.Equals(sortDirection, "desc", StringComparison.OrdinalIgnoreCase);

        // Only these columns are sortable - never build an ORDER BY from raw client input.
        return (sortBy?.Trim().ToLowerInvariant()) switch
        {
            "priority" => descending ? query.OrderByDescending(t => t.Priority) : query.OrderBy(t => t.Priority),
            "status" => descending ? query.OrderByDescending(t => t.Status) : query.OrderBy(t => t.Status),
            "ticketnumber" => descending ? query.OrderByDescending(t => t.TicketNumber) : query.OrderBy(t => t.TicketNumber),
            "createdat" => descending ? query.OrderByDescending(t => t.CreatedAt) : query.OrderBy(t => t.CreatedAt),
            _ => query.OrderByDescending(t => t.CreatedAt)
        };
    }
}
