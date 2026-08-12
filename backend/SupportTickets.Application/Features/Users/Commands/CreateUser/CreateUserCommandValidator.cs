using FluentValidation;
using Microsoft.EntityFrameworkCore;
using SupportTickets.Application.Common.Interfaces;
using SupportTickets.Domain.Entities;
using SupportTickets.Domain.Enums;

namespace SupportTickets.Application.Features.Users.Commands.CreateUser;

public class CreateUserCommandValidator : AbstractValidator<CreateUserCommand>
{
    private readonly IGenericRepository<User> _userRepository;

    public CreateUserCommandValidator(IGenericRepository<User> userRepository)
    {
        _userRepository = userRepository;

        RuleFor(x => x.FullName).NotEmpty().MaximumLength(200);

        RuleFor(x => x.Email)
            .NotEmpty()
            .EmailAddress()
            .MaximumLength(256)
            .MustAsync(BeUniqueEmail).WithMessage("Email is already in use.");

        RuleFor(x => x.Password).NotEmpty().MinimumLength(6);

        RuleFor(x => x.Role)
            .NotEmpty()
            .Must(r => Enum.TryParse<UserRole>(r, true, out _))
            .WithMessage("Role must be one of: Admin, SupportAgent, Customer.");
    }

    private async Task<bool> BeUniqueEmail(string email, CancellationToken cancellationToken)
    {
        var normalized = email.Trim().ToLowerInvariant();
        return !await _userRepository.Query()
            .AnyAsync(u => u.Email.ToLower() == normalized, cancellationToken);
    }
}
