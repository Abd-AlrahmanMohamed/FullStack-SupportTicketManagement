using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SupportTickets.Application.Common.Dtos;
using SupportTickets.Application.Common.Exceptions;
using SupportTickets.Application.Common.Interfaces;
using SupportTickets.Application.Common.Services;
using SupportTickets.Domain.Entities;
using SupportTickets.Domain.Enums;

namespace SupportTickets.Application.Features.Tickets.Commands.CloseTicket;

public class CloseTicketCommandHandler : IRequestHandler<CloseTicketCommand, TicketDto>
{
    private readonly IGenericRepository<Ticket> _ticketRepository;
    private readonly IGenericRepository<TicketActivity> _activityRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUser;
    private readonly IMapper _mapper;
    private readonly ILogger<CloseTicketCommandHandler> _logger;

    public CloseTicketCommandHandler(
        IGenericRepository<Ticket> ticketRepository,
        IGenericRepository<TicketActivity> activityRepository,
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUser,
        IMapper mapper,
        ILogger<CloseTicketCommandHandler> logger)
    {
        _ticketRepository = ticketRepository;
        _activityRepository = activityRepository;
        _unitOfWork = unitOfWork;
        _currentUser = currentUser;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<TicketDto> Handle(CloseTicketCommand request, CancellationToken cancellationToken)
    {
        var ticket = await _ticketRepository.GetByIdAsync(request.TicketId, cancellationToken)
            ?? throw new NotFoundException(nameof(Ticket), request.TicketId);

        TicketAccessGuard.EnsureCanAccess(ticket, _currentUser.UserId, _currentUser.Role);

        if (!TicketStatusRules.CanClose(ticket.Status))
        {
            throw new BusinessRuleException("A ticket can only be closed when its status is Resolved.");
        }

        var oldStatus = ticket.Status;
        ticket.Status = TicketStatus.Closed;
        ticket.ClosedAt = DateTime.UtcNow;
        ticket.UpdatedAt = DateTime.UtcNow;
        _ticketRepository.Update(ticket);

        await _activityRepository.AddAsync(new TicketActivity
        {
            TicketId = ticket.Id,
            UserId = _currentUser.UserId!.Value,
            Action = "TicketClosed",
            OldValue = oldStatus.ToString(),
            NewValue = TicketStatus.Closed.ToString()
        }, cancellationToken);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Ticket {TicketId} closed by user {UserId}", ticket.Id, _currentUser.UserId);

        var updated = await _ticketRepository.Query()
            .Include(t => t.Customer)
            .Include(t => t.AssignedAgent)
            .AsNoTracking()
            .FirstAsync(t => t.Id == ticket.Id, cancellationToken);

        return _mapper.Map<TicketDto>(updated);
    }
}
