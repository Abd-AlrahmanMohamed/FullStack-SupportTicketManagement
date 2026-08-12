using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using SupportTickets.Application.Common.Dtos;
using SupportTickets.Application.Common.Exceptions;
using SupportTickets.Application.Common.Interfaces;
using SupportTickets.Application.Common.Services;
using SupportTickets.Domain.Entities;
using SupportTickets.Domain.Enums;

namespace SupportTickets.Application.Features.Tickets.Queries.GetTicketById;

public class GetTicketByIdQueryHandler : IRequestHandler<GetTicketByIdQuery, TicketDetailsDto>
{
    private readonly IGenericRepository<Ticket> _ticketRepository;
    private readonly ICurrentUserService _currentUser;
    private readonly IMapper _mapper;

    public GetTicketByIdQueryHandler(
        IGenericRepository<Ticket> ticketRepository,
        ICurrentUserService currentUser,
        IMapper mapper)
    {
        _ticketRepository = ticketRepository;
        _currentUser = currentUser;
        _mapper = mapper;
    }

    public async Task<TicketDetailsDto> Handle(GetTicketByIdQuery request, CancellationToken cancellationToken)
    {
        var ticket = await _ticketRepository.Query()
            .Include(t => t.Customer)
            .Include(t => t.AssignedAgent)
            .Include(t => t.Comments).ThenInclude(c => c.User)
            .Include(t => t.Activities).ThenInclude(a => a.User)
            .Include(t => t.TimeEntries).ThenInclude(te => te.Agent)
            .AsNoTracking()
            .FirstOrDefaultAsync(t => t.Id == request.TicketId, cancellationToken)
            ?? throw new NotFoundException(nameof(Ticket), request.TicketId);

        TicketAccessGuard.EnsureCanAccess(ticket, _currentUser.UserId, _currentUser.Role);

        var dto = _mapper.Map<TicketDetailsDto>(ticket);

        // Individual time-entry line items are internal support detail; customers only see the total.
        if (_currentUser.Role == UserRole.Customer)
        {
            dto.TimeEntries = new List<TimeEntryDto>();
        }

        return dto;
    }
}
