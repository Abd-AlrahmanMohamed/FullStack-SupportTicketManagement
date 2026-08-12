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

namespace SupportTickets.Application.Features.Tickets.Commands.UpdateTicketStatus;

public class UpdateTicketStatusCommandHandler : IRequestHandler<UpdateTicketStatusCommand, TicketDto>
{
    private readonly IGenericRepository<Ticket> _ticketRepository;
    private readonly IGenericRepository<TicketActivity> _activityRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUser;
    private readonly IMapper _mapper;
    private readonly ILogger<UpdateTicketStatusCommandHandler> _logger;

    public UpdateTicketStatusCommandHandler(
        IGenericRepository<Ticket> ticketRepository,
        IGenericRepository<TicketActivity> activityRepository,
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUser,
        IMapper mapper,
        ILogger<UpdateTicketStatusCommandHandler> logger)
    {
        _ticketRepository = ticketRepository;
        _activityRepository = activityRepository;
        _unitOfWork = unitOfWork;
        _currentUser = currentUser;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<TicketDto> Handle(UpdateTicketStatusCommand request, CancellationToken cancellationToken)
    {
        var ticket = await _ticketRepository.GetByIdAsync(request.TicketId, cancellationToken)
            ?? throw new NotFoundException(nameof(Ticket), request.TicketId);

        // A support agent may only move tickets that are assigned to them; admins are unrestricted.
        if (_currentUser.Role == UserRole.SupportAgent && ticket.AssignedAgentId != _currentUser.UserId)
        {
            throw new NotFoundException(nameof(Ticket), request.TicketId);
        }

        var targetStatus = Enum.Parse<TicketStatus>(request.Status, true);

        if (!TicketStatusRules.IsValidTransition(ticket.Status, targetStatus))
        {
            throw new BusinessRuleException($"Cannot change ticket status from {ticket.Status} to {targetStatus}.");
        }

        var oldStatus = ticket.Status;
        ticket.Status = targetStatus;
        ticket.UpdatedAt = DateTime.UtcNow;

        if (targetStatus == TicketStatus.Resolved)
        {
            ticket.ResolvedAt = DateTime.UtcNow;
        }
        else if (oldStatus == TicketStatus.Resolved)
        {
            // Moved back out of Resolved (e.g. re-opened for more work).
            ticket.ResolvedAt = null;
        }

        _ticketRepository.Update(ticket);

        await _activityRepository.AddAsync(new TicketActivity
        {
            TicketId = ticket.Id,
            UserId = _currentUser.UserId!.Value,
            Action = "StatusChanged",
            OldValue = oldStatus.ToString(),
            NewValue = targetStatus.ToString()
        }, cancellationToken);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Ticket {TicketId} status changed from {OldStatus} to {NewStatus}", ticket.Id, oldStatus, targetStatus);

        var updated = await _ticketRepository.Query()
            .Include(t => t.Customer)
            .Include(t => t.AssignedAgent)
            .AsNoTracking()
            .FirstAsync(t => t.Id == ticket.Id, cancellationToken);

        return _mapper.Map<TicketDto>(updated);
    }
}
