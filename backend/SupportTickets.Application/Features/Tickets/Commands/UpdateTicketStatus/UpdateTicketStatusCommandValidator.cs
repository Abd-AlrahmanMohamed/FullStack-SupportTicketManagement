using FluentValidation;
using SupportTickets.Domain.Enums;

namespace SupportTickets.Application.Features.Tickets.Commands.UpdateTicketStatus;

public class UpdateTicketStatusCommandValidator : AbstractValidator<UpdateTicketStatusCommand>
{
    public UpdateTicketStatusCommandValidator()
    {
        RuleFor(x => x.Status)
            .NotEmpty()
            .Must(s => Enum.TryParse<TicketStatus>(s, true, out _))
            .WithMessage("Status must be one of: Open, InProgress, Resolved, Closed.");
    }
}
