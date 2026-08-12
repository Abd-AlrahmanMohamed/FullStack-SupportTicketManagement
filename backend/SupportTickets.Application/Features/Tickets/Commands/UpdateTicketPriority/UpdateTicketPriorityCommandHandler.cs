using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SupportTickets.Application.Common.Dtos;
using SupportTickets.Application.Common.Exceptions;
using SupportTickets.Application.Common.Interfaces;
using SupportTickets.Domain.Entities;
using SupportTickets.Domain.Enums;

namespace SupportTickets.Application.Features.Tickets.Commands.UpdateTicketPriority;

public class UpdateTicketPriorityCommandHandler : IRequestHandler<UpdateTicketPriorityCommand, TicketDto>
{
    private readonly IGenericRepository<Ticket> _ticketRepository;
    private readonly IGenericRepository<TicketActivity> _activityRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUser;
    private readonly IMapper _mapper;
    private readonly ILogger<UpdateTicketPriorityCommandHandler> _logger;

    public UpdateTicketPriorityCommandHandler(
        IGenericRepository<Ticket> ticketRepository,
        IGenericRepository<TicketActivity> activityRepository,
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUser,
        IMapper mapper,
        ILogger<UpdateTicketPriorityCommandHandler> logger)
    {
        _ticketRepository = ticketRepository;
        _activityRepository = activityRepository;
        _unitOfWork = unitOfWork;
        _currentUser = currentUser;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<TicketDto> Handle(UpdateTicketPriorityCommand request, CancellationToken cancellationToken)
    {
        var ticket = await _ticketRepository.GetByIdAsync(request.TicketId, cancellationToken)
            ?? throw new NotFoundException(nameof(Ticket), request.TicketId);

        var targetPriority = Enum.Parse<TicketPriority>(request.Priority, true);
        var oldPriority = ticket.Priority;

        ticket.Priority = targetPriority;
        ticket.UpdatedAt = DateTime.UtcNow;
        _ticketRepository.Update(ticket);

        await _activityRepository.AddAsync(new TicketActivity
        {
            TicketId = ticket.Id,
            UserId = _currentUser.UserId!.Value,
            Action = "PriorityChanged",
            OldValue = oldPriority.ToString(),
            NewValue = targetPriority.ToString()
        }, cancellationToken);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Ticket {TicketId} priority changed from {OldPriority} to {NewPriority}", ticket.Id, oldPriority, targetPriority);

        var updated = await _ticketRepository.Query()
            .Include(t => t.Customer)
            .Include(t => t.AssignedAgent)
            .AsNoTracking()
            .FirstAsync(t => t.Id == ticket.Id, cancellationToken);

        return _mapper.Map<TicketDto>(updated);
    }
}
