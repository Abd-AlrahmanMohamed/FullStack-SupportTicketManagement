using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SupportTickets.Application.Common.Dtos;
using SupportTickets.Application.Common.Interfaces;
using SupportTickets.Domain.Entities;
using SupportTickets.Domain.Enums;

namespace SupportTickets.Application.Features.Tickets.Commands.CreateTicket;

public class CreateTicketCommandHandler : IRequestHandler<CreateTicketCommand, TicketDto>
{
    private readonly IGenericRepository<Ticket> _ticketRepository;
    private readonly IGenericRepository<TicketActivity> _activityRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUser;
    private readonly IMapper _mapper;
    private readonly ILogger<CreateTicketCommandHandler> _logger;

    public CreateTicketCommandHandler(
        IGenericRepository<Ticket> ticketRepository,
        IGenericRepository<TicketActivity> activityRepository,
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUser,
        IMapper mapper,
        ILogger<CreateTicketCommandHandler> logger)
    {
        _ticketRepository = ticketRepository;
        _activityRepository = activityRepository;
        _unitOfWork = unitOfWork;
        _currentUser = currentUser;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<TicketDto> Handle(CreateTicketCommand request, CancellationToken cancellationToken)
    {
        // CustomerId always comes from the authenticated user, never from client input.
        var customerId = _currentUser.UserId!.Value;

        var ticketCount = await _ticketRepository.Query().CountAsync(cancellationToken);

        var ticket = new Ticket
        {
            TicketNumber = $"TKT-{ticketCount + 1:D6}",
            Title = request.Title.Trim(),
            Description = request.Description.Trim(),
            Priority = Enum.Parse<TicketPriority>(request.Priority, true),
            Status = TicketStatus.Open,
            CustomerId = customerId
        };

        await _ticketRepository.AddAsync(ticket, cancellationToken);

        await _activityRepository.AddAsync(new TicketActivity
        {
            TicketId = ticket.Id,
            UserId = customerId,
            Action = "TicketCreated",
            NewValue = ticket.Status.ToString()
        }, cancellationToken);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Ticket {TicketNumber} created by customer {CustomerId}", ticket.TicketNumber, customerId);

        var created = await _ticketRepository.Query()
            .Include(t => t.Customer)
            .AsNoTracking()
            .FirstAsync(t => t.Id == ticket.Id, cancellationToken);

        return _mapper.Map<TicketDto>(created);
    }
}
