using AutoMapper;
using MediatR;
using Microsoft.Extensions.Logging;
using SupportTickets.Application.Common.Dtos;
using SupportTickets.Application.Common.Exceptions;
using SupportTickets.Application.Common.Interfaces;
using SupportTickets.Domain.Entities;
using SupportTickets.Domain.Enums;

namespace SupportTickets.Application.Features.Tickets.Commands.AddTimeEntry;

public class AddTimeEntryCommandHandler : IRequestHandler<AddTimeEntryCommand, TimeEntryDto>
{
    private readonly IGenericRepository<Ticket> _ticketRepository;
    private readonly IGenericRepository<TimeEntry> _timeEntryRepository;
    private readonly IGenericRepository<TicketActivity> _activityRepository;
    private readonly IGenericRepository<User> _userRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUser;
    private readonly IMapper _mapper;
    private readonly ILogger<AddTimeEntryCommandHandler> _logger;

    public AddTimeEntryCommandHandler(
        IGenericRepository<Ticket> ticketRepository,
        IGenericRepository<TimeEntry> timeEntryRepository,
        IGenericRepository<TicketActivity> activityRepository,
        IGenericRepository<User> userRepository,
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUser,
        IMapper mapper,
        ILogger<AddTimeEntryCommandHandler> logger)
    {
        _ticketRepository = ticketRepository;
        _timeEntryRepository = timeEntryRepository;
        _activityRepository = activityRepository;
        _userRepository = userRepository;
        _unitOfWork = unitOfWork;
        _currentUser = currentUser;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<TimeEntryDto> Handle(AddTimeEntryCommand request, CancellationToken cancellationToken)
    {
        var ticket = await _ticketRepository.GetByIdAsync(request.TicketId, cancellationToken)
            ?? throw new NotFoundException(nameof(Ticket), request.TicketId);

        var agentId = _currentUser.UserId!.Value;

        // Agents may only log time against tickets currently assigned to them.
        if (_currentUser.Role != UserRole.SupportAgent || ticket.AssignedAgentId != agentId)
        {
            throw new NotFoundException(nameof(Ticket), request.TicketId);
        }

        var agent = await _userRepository.GetByIdAsync(agentId, cancellationToken)
            ?? throw new NotFoundException(nameof(User), agentId);

        var timeEntry = new TimeEntry
        {
            TicketId = ticket.Id,
            AgentId = agentId,
            WorkDate = request.WorkDate,
            DurationMinutes = request.DurationMinutes,
            Description = request.Description?.Trim()
        };

        await _timeEntryRepository.AddAsync(timeEntry, cancellationToken);

        await _activityRepository.AddAsync(new TicketActivity
        {
            TicketId = ticket.Id,
            UserId = agentId,
            Action = "TimeLogged",
            NewValue = $"{request.DurationMinutes} minutes"
        }, cancellationToken);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Time entry logged for ticket {TicketId} by agent {AgentId}: {Minutes} minutes", ticket.Id, agentId, request.DurationMinutes);

        timeEntry.Agent = agent;
        return _mapper.Map<TimeEntryDto>(timeEntry);
    }
}
