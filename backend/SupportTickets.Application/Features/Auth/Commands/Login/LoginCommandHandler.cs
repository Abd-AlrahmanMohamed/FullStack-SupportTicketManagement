using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SupportTickets.Application.Common.Dtos;
using SupportTickets.Application.Common.Interfaces;
using SupportTickets.Domain.Entities;

namespace SupportTickets.Application.Features.Auth.Commands.Login;

public class LoginCommandHandler : IRequestHandler<LoginCommand, LoginResultDto>
{
    private readonly IGenericRepository<User> _userRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IJwtTokenService _jwtTokenService;
    private readonly IMapper _mapper;
    private readonly ILogger<LoginCommandHandler> _logger;

    public LoginCommandHandler(
        IGenericRepository<User> userRepository,
        IPasswordHasher passwordHasher,
        IJwtTokenService jwtTokenService,
        IMapper mapper,
        ILogger<LoginCommandHandler> logger)
    {
        _userRepository = userRepository;
        _passwordHasher = passwordHasher;
        _jwtTokenService = jwtTokenService;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<LoginResultDto> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        var normalizedEmail = request.Email.Trim().ToLowerInvariant();

        var user = await _userRepository.Query()
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Email.ToLower() == normalizedEmail, cancellationToken);

        if (user is null || !user.IsActive || !_passwordHasher.VerifyPassword(user.PasswordHash, request.Password))
        {
            _logger.LogWarning("Login failed for email {Email}", request.Email);
            throw new UnauthorizedAccessException("Invalid email or password.");
        }

        var token = _jwtTokenService.GenerateToken(user);

        _logger.LogInformation("User {UserId} logged in successfully", user.Id);

        return new LoginResultDto
        {
            Token = token,
            User = _mapper.Map<UserDto>(user)
        };
    }
}
