using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SupportTickets.Application.Common.Dtos;
using SupportTickets.Application.Common.Exceptions;
using SupportTickets.Application.Common.Interfaces;
using SupportTickets.Domain.Entities;
using SupportTickets.Domain.Enums;

namespace SupportTickets.Application.Features.Tickets.Commands.AssignTicket;

public class AssignTicketCommandHandler : IRequestHandler<AssignTicketCommand, TicketDto>
{
    private readonly IGenericRepository<Ticket> _ticketRepository;
    private readonly IGenericRepository<User> _userRepository;
    private readonly IGenericRepository<TicketActivity> _activityRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUser;
    private readonly IMapper _mapper;
    private readonly ILogger<AssignTicketCommandHandler> _logger;

    public AssignTicketCommandHandler(
        IGenericRepository<Ticket> ticketRepository,
        IGenericRepository<User> userRepository,
        IGenericRepository<TicketActivity> activityRepository,
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUser,
        IMapper mapper,
        ILogger<AssignTicketCommandHandler> logger)
    {
        _ticketRepository = ticketRepository;
        _userRepository = userRepository;
        _activityRepository = activityRepository;
        _unitOfWork = unitOfWork;
        _currentUser = currentUser;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<TicketDto> Handle(AssignTicketCommand request, CancellationToken cancellationToken)
    {
        var ticket = await _ticketRepository.GetByIdAsync(request.TicketId, cancellationToken)
            ?? throw new NotFoundException(nameof(Ticket), request.TicketId);

        var agent = await _userRepository.GetByIdAsync(request.AgentId, cancellationToken);
        if (agent is null || agent.Role != UserRole.SupportAgent || !agent.IsActive)
        {
            throw new BusinessRuleException("Selected user is not an active support agent.");
        }

        var oldAgentId = ticket.AssignedAgentId;
        ticket.AssignedAgentId = agent.Id;
        ticket.UpdatedAt = DateTime.UtcNow;
        _ticketRepository.Update(ticket);

        await _activityRepository.AddAsync(new TicketActivity
        {
            TicketId = ticket.Id,
            UserId = _currentUser.UserId!.Value,
            Action = "TicketAssigned",
            OldValue = oldAgentId?.ToString(),
            NewValue = agent.FullName
        }, cancellationToken);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Ticket {TicketId} assigned to agent {AgentId}", ticket.Id, agent.Id);

        var updated = await _ticketRepository.Query()
            .Include(t => t.Customer)
            .Include(t => t.AssignedAgent)
            .AsNoTracking()
            .FirstAsync(t => t.Id == ticket.Id, cancellationToken);

        return _mapper.Map<TicketDto>(updated);
    }
}
