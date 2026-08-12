using FluentValidation;
using SupportTickets.Domain.Enums;

namespace SupportTickets.Application.Features.Tickets.Commands.UpdateTicketPriority;

public class UpdateTicketPriorityCommandValidator : AbstractValidator<UpdateTicketPriorityCommand>
{
    public UpdateTicketPriorityCommandValidator()
    {
        RuleFor(x => x.Priority)
            .NotEmpty()
            .Must(p => Enum.TryParse<TicketPriority>(p, true, out _))
            .WithMessage("Priority must be one of: Low, Medium, High, Critical.");
    }
}
