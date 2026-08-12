using AutoMapper;
using AutoMapper.QueryableExtensions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using SupportTickets.Application.Common.Dtos;
using SupportTickets.Application.Common.Exceptions;
using SupportTickets.Application.Common.Interfaces;
using SupportTickets.Application.Common.Services;
using SupportTickets.Domain.Entities;

namespace SupportTickets.Application.Features.Tickets.Queries.GetTicketTimeEntries;

public class GetTicketTimeEntriesQueryHandler : IRequestHandler<GetTicketTimeEntriesQuery, List<TimeEntryDto>>
{
    private readonly IGenericRepository<Ticket> _ticketRepository;
    private readonly IGenericRepository<TimeEntry> _timeEntryRepository;
    private readonly ICurrentUserService _currentUser;
    private readonly IMapper _mapper;

    public GetTicketTimeEntriesQueryHandler(
        IGenericRepository<Ticket> ticketRepository,
        IGenericRepository<TimeEntry> timeEntryRepository,
        ICurrentUserService currentUser,
        IMapper mapper)
    {
        _ticketRepository = ticketRepository;
        _timeEntryRepository = timeEntryRepository;
        _currentUser = currentUser;
        _mapper = mapper;
    }

    public async Task<List<TimeEntryDto>> Handle(GetTicketTimeEntriesQuery request, CancellationToken cancellationToken)
    {
        var ticket = await _ticketRepository.GetByIdAsync(request.TicketId, cancellationToken)
            ?? throw new NotFoundException(nameof(Ticket), request.TicketId);

        // Time entries are internal support detail - never exposed to the owning customer.
        TicketAccessGuard.EnsureCanAccess(ticket, _currentUser.UserId, _currentUser.Role);

        return await _timeEntryRepository.Query()
            .AsNoTracking()
            .Where(t => t.TicketId == request.TicketId)
            .OrderByDescending(t => t.WorkDate)
            .ProjectTo<TimeEntryDto>(_mapper.ConfigurationProvider)
            .ToListAsync(cancellationToken);
    }
}
