using AutoMapper;
using AutoMapper.QueryableExtensions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using SupportTickets.Application.Common.Dtos;
using SupportTickets.Application.Common.Interfaces;
using SupportTickets.Domain.Entities;

namespace SupportTickets.Application.Features.Users.Queries.GetUsers;

public class GetUsersQueryHandler : IRequestHandler<GetUsersQuery, List<UserDto>>
{
    private readonly IGenericRepository<User> _userRepository;
    private readonly IMapper _mapper;

    public GetUsersQueryHandler(IGenericRepository<User> userRepository, IMapper mapper)
    {
        _userRepository = userRepository;
        _mapper = mapper;
    }

    public async Task<List<UserDto>> Handle(GetUsersQuery request, CancellationToken cancellationToken)
    {
        return await _userRepository.Query()
            .AsNoTracking()
            .OrderBy(u => u.FullName)
            .ProjectTo<UserDto>(_mapper.ConfigurationProvider)
            .ToListAsync(cancellationToken);
    }
}
