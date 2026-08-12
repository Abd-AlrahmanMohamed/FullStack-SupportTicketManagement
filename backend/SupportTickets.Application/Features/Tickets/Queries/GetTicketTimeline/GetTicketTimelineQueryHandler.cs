using AutoMapper;
using AutoMapper.QueryableExtensions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using SupportTickets.Application.Common.Dtos;
using SupportTickets.Application.Common.Exceptions;
using SupportTickets.Application.Common.Interfaces;
using SupportTickets.Application.Common.Services;
using SupportTickets.Domain.Entities;

namespace SupportTickets.Application.Features.Tickets.Queries.GetTicketTimeline;

public class GetTicketTimelineQueryHandler : IRequestHandler<GetTicketTimelineQuery, List<ActivityDto>>
{
    private readonly IGenericRepository<Ticket> _ticketRepository;
    private readonly IGenericRepository<TicketActivity> _activityRepository;
    private readonly ICurrentUserService _currentUser;
    private readonly IMapper _mapper;

    public GetTicketTimelineQueryHandler(
        IGenericRepository<Ticket> ticketRepository,
        IGenericRepository<TicketActivity> activityRepository,
        ICurrentUserService currentUser,
        IMapper mapper)
    {
        _ticketRepository = ticketRepository;
        _activityRepository = activityRepository;
        _currentUser = currentUser;
        _mapper = mapper;
    }

    public async Task<List<ActivityDto>> Handle(GetTicketTimelineQuery request, CancellationToken cancellationToken)
    {
        var ticket = await _ticketRepository.GetByIdAsync(request.TicketId, cancellationToken)
            ?? throw new NotFoundException(nameof(Ticket), request.TicketId);

        TicketAccessGuard.EnsureCanAccess(ticket, _currentUser.UserId, _currentUser.Role);

        return await _activityRepository.Query()
            .AsNoTracking()
            .Where(a => a.TicketId == request.TicketId)
            .OrderByDescending(a => a.CreatedAt)
            .ProjectTo<ActivityDto>(_mapper.ConfigurationProvider)
            .ToListAsync(cancellationToken);
    }
}
