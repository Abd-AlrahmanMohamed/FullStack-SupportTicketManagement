using FluentValidation.TestHelper;
using SupportTickets.Application.Features.Tickets.Commands.AddTimeEntry;
using Xunit;

namespace SupportTickets.Tests.Unit.Tickets;

public class AddTimeEntryCommandValidatorTests
{
    private readonly AddTimeEntryCommandValidator _validator = new();

    [Theory]
    [InlineData(0)]
    [InlineData(-15)]
    public void DurationMinutes_MustBeGreaterThanZero(int duration)
    {
        var command = new AddTimeEntryCommand
        {
            TicketId = Guid.NewGuid(),
            WorkDate = DateTime.UtcNow,
            DurationMinutes = duration
        };

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.DurationMinutes);
    }

    [Fact]
    public void ValidCommand_HasNoErrors()
    {
        var command = new AddTimeEntryCommand
        {
            TicketId = Guid.NewGuid(),
            WorkDate = DateTime.UtcNow,
            DurationMinutes = 30,
            Description = "Investigated the issue."
        };

        var result = _validator.TestValidate(command);

        result.ShouldNotHaveAnyValidationErrors();
    }
}
