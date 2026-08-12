using AutoMapper;
using MediatR;
using Microsoft.Extensions.Logging;
using SupportTickets.Application.Common.Dtos;
using SupportTickets.Application.Common.Exceptions;
using SupportTickets.Application.Common.Interfaces;
using SupportTickets.Application.Common.Services;
using SupportTickets.Domain.Entities;

namespace SupportTickets.Application.Features.Tickets.Commands.AddComment;

public class AddCommentCommandHandler : IRequestHandler<AddCommentCommand, CommentDto>
{
    private readonly IGenericRepository<Ticket> _ticketRepository;
    private readonly IGenericRepository<TicketComment> _commentRepository;
    private readonly IGenericRepository<TicketActivity> _activityRepository;
    private readonly IGenericRepository<User> _userRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUser;
    private readonly IMapper _mapper;
    private readonly ILogger<AddCommentCommandHandler> _logger;

    public AddCommentCommandHandler(
        IGenericRepository<Ticket> ticketRepository,
        IGenericRepository<TicketComment> commentRepository,
        IGenericRepository<TicketActivity> activityRepository,
        IGenericRepository<User> userRepository,
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUser,
        IMapper mapper,
        ILogger<AddCommentCommandHandler> logger)
    {
        _ticketRepository = ticketRepository;
        _commentRepository = commentRepository;
        _activityRepository = activityRepository;
        _userRepository = userRepository;
        _unitOfWork = unitOfWork;
        _currentUser = currentUser;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<CommentDto> Handle(AddCommentCommand request, CancellationToken cancellationToken)
    {
        var ticket = await _ticketRepository.GetByIdAsync(request.TicketId, cancellationToken)
            ?? throw new NotFoundException(nameof(Ticket), request.TicketId);

        TicketAccessGuard.EnsureCanAccess(ticket, _currentUser.UserId, _currentUser.Role);

        var actingUser = await _userRepository.GetByIdAsync(_currentUser.UserId!.Value, cancellationToken)
            ?? throw new NotFoundException(nameof(User), _currentUser.UserId!.Value);

        var comment = new TicketComment
        {
            TicketId = ticket.Id,
            UserId = actingUser.Id,
            Message = request.Message.Trim()
        };

        await _commentRepository.AddAsync(comment, cancellationToken);

        await _activityRepository.AddAsync(new TicketActivity
        {
            TicketId = ticket.Id,
            UserId = actingUser.Id,
            Action = "CommentAdded"
        }, cancellationToken);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Comment added to ticket {TicketId} by user {UserId}", ticket.Id, actingUser.Id);

        comment.User = actingUser;
        return _mapper.Map<CommentDto>(comment);
    }
}
