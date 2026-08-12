using FluentValidation;
using SupportTickets.Domain.Enums;

namespace SupportTickets.Application.Features.Tickets.Commands.CreateTicket;

public class CreateTicketCommandValidator : AbstractValidator<CreateTicketCommand>
{
    public CreateTicketCommandValidator()
    {
        RuleFor(x => x.Title).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Description).NotEmpty().MaximumLength(4000);
        RuleFor(x => x.Priority)
            .NotEmpty()
            .Must(p => Enum.TryParse<TicketPriority>(p, true, out _))
            .WithMessage("Priority must be one of: Low, Medium, High, Critical.");
    }
}
