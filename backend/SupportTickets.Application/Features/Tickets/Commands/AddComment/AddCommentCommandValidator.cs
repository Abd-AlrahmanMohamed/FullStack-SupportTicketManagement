using FluentValidation;

namespace SupportTickets.Application.Features.Tickets.Commands.AddComment;

public class AddCommentCommandValidator : AbstractValidator<AddCommentCommand>
{
    public AddCommentCommandValidator()
    {
        RuleFor(x => x.Message).NotEmpty().MaximumLength(2000);
    }
}
