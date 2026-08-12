using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SupportTickets.Application.Common.Dtos;
using SupportTickets.Application.Features.Users.Commands.CreateUser;
using SupportTickets.Application.Features.Users.Queries.GetSupportAgents;
using SupportTickets.Application.Features.Users.Queries.GetUsers;

namespace SupportTickets.Api.Controllers;

[ApiController]
[Route("api/users")]
[Authorize(Roles = "Admin")]
public class UsersController : ControllerBase
{
    private readonly IMediator _mediator;

    public UsersController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<ActionResult<List<UserDto>>> GetUsers(CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetUsersQuery(), cancellationToken);
        return Ok(result);
    }

    [HttpGet("support-agents")]
    public async Task<ActionResult<List<UserDto>>> GetSupportAgents(CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetSupportAgentsQuery(), cancellationToken);
        return Ok(result);
    }

    [HttpPost]
    public async Task<ActionResult<UserDto>> CreateUser(CreateUserCommand command, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(command, cancellationToken);
        return Ok(result);
    }
}
