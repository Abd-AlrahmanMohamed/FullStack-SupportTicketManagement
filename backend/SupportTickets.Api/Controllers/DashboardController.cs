using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SupportTickets.Application.Common.Dtos;
using SupportTickets.Application.Features.Dashboard.Queries.GetDashboard;

namespace SupportTickets.Api.Controllers;

[ApiController]
[Route("api/dashboard")]
[Authorize(Roles = "Admin")]
public class DashboardController : ControllerBase
{
    private readonly IMediator _mediator;

    public DashboardController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<ActionResult<DashboardDto>> GetDashboard(CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetDashboardQuery(), cancellationToken);
        return Ok(result);
    }
}
