using FluentValidation;

namespace SupportTickets.Application.Features.Tickets.Commands.AddTimeEntry;

public class AddTimeEntryCommandValidator : AbstractValidator<AddTimeEntryCommand>
{
    public AddTimeEntryCommandValidator()
    {
        RuleFor(x => x.DurationMinutes).GreaterThan(0);
        RuleFor(x => x.WorkDate).NotEqual(default(DateTime));
        RuleFor(x => x.Description).MaximumLength(1000);
    }
}
