using AutoMapper;
using MediatR;
using Microsoft.Extensions.Logging;
using SupportTickets.Application.Common.Dtos;
using SupportTickets.Application.Common.Interfaces;
using SupportTickets.Domain.Entities;
using SupportTickets.Domain.Enums;

namespace SupportTickets.Application.Features.Users.Commands.CreateUser;

public class CreateUserCommandHandler : IRequestHandler<CreateUserCommand, UserDto>
{
    private readonly IGenericRepository<User> _userRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IMapper _mapper;
    private readonly ILogger<CreateUserCommandHandler> _logger;

    public CreateUserCommandHandler(
        IGenericRepository<User> userRepository,
        IUnitOfWork unitOfWork,
        IPasswordHasher passwordHasher,
        IMapper mapper,
        ILogger<CreateUserCommandHandler> logger)
    {
        _userRepository = userRepository;
        _unitOfWork = unitOfWork;
        _passwordHasher = passwordHasher;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<UserDto> Handle(CreateUserCommand request, CancellationToken cancellationToken)
    {
        var user = new User
        {
            FullName = request.FullName.Trim(),
            Email = request.Email.Trim(),
            PasswordHash = _passwordHasher.HashPassword(request.Password),
            Role = Enum.Parse<UserRole>(request.Role, true),
            IsActive = true
        };

        await _userRepository.AddAsync(user, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("User {UserId} created with role {Role}", user.Id, user.Role);

        return _mapper.Map<UserDto>(user);
    }
}
