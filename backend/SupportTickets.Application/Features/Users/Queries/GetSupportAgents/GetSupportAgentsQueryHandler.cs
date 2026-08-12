using AutoMapper;
using AutoMapper.QueryableExtensions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using SupportTickets.Application.Common.Dtos;
using SupportTickets.Application.Common.Interfaces;
using SupportTickets.Domain.Entities;
using SupportTickets.Domain.Enums;

namespace SupportTickets.Application.Features.Users.Queries.GetSupportAgents;

public class GetSupportAgentsQueryHandler : IRequestHandler<GetSupportAgentsQuery, List<UserDto>>
{
    private readonly IGenericRepository<User> _userRepository;
    private readonly IMapper _mapper;

    public GetSupportAgentsQueryHandler(IGenericRepository<User> userRepository, IMapper mapper)
    {
        _userRepository = userRepository;
        _mapper = mapper;
    }

    public async Task<List<UserDto>> Handle(GetSupportAgentsQuery request, CancellationToken cancellationToken)
    {
        return await _userRepository.Query()
            .AsNoTracking()
            .Where(u => u.Role == UserRole.SupportAgent && u.IsActive)
            .OrderBy(u => u.FullName)
            .ProjectTo<UserDto>(_mapper.ConfigurationProvider)
            .ToListAsync(cancellationToken);
    }
}
