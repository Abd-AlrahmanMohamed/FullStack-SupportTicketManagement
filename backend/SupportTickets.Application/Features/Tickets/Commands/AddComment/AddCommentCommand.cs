using MediatR;
using SupportTickets.Application.Common.Dtos;

namespace SupportTickets.Application.Features.Tickets.Commands.AddComment;

public class AddCommentCommand : IRequest<CommentDto>
{
    public Guid TicketId { get; set; }
    public string Message { get; set; } = string.Empty;
}
