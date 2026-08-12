using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SupportTickets.Application.Common.Dtos;
using SupportTickets.Application.Common.Models;
using SupportTickets.Application.Features.Tickets.Commands.AddComment;
using SupportTickets.Application.Features.Tickets.Commands.AddTimeEntry;
using SupportTickets.Application.Features.Tickets.Commands.AssignTicket;
using SupportTickets.Application.Features.Tickets.Commands.CloseTicket;
using SupportTickets.Application.Features.Tickets.Commands.CreateTicket;
using SupportTickets.Application.Features.Tickets.Commands.UpdateTicketPriority;
using SupportTickets.Application.Features.Tickets.Commands.UpdateTicketStatus;
using SupportTickets.Application.Features.Tickets.Queries.GetTicketById;
using SupportTickets.Application.Features.Tickets.Queries.GetTickets;
using SupportTickets.Application.Features.Tickets.Queries.GetTicketTimeEntries;
using SupportTickets.Application.Features.Tickets.Queries.GetTicketTimeline;

namespace SupportTickets.Api.Controllers;

[ApiController]
[Route("api/tickets")]
[Authorize]
public class TicketsController : ControllerBase
{
    private readonly IMediator _mediator;

    public TicketsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost]
    [Authorize(Roles = "Customer")]
    public async Task<ActionResult<TicketDto>> Create(CreateTicketCommand command, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(command, cancellationToken);
        return Ok(result);
    }

    [HttpGet]
    public async Task<ActionResult<PaginatedResult<TicketDto>>> GetTickets(
        [FromQuery] GetTicketsQuery query, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(query, cancellationToken);
        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<TicketDetailsDto>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetTicketByIdQuery { TicketId = id }, cancellationToken);
        return Ok(result);
    }

    [HttpPatch("{id:guid}/assign")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<TicketDto>> Assign(Guid id, AssignTicketCommand command, CancellationToken cancellationToken)
    {
        command.TicketId = id;
        var result = await _mediator.Send(command, cancellationToken);
        return Ok(result);
    }

    [HttpPatch("{id:guid}/status")]
    [Authorize(Roles = "Admin,SupportAgent")]
    public async Task<ActionResult<TicketDto>> UpdateStatus(Guid id, UpdateTicketStatusCommand command, CancellationToken cancellationToken)
    {
        command.TicketId = id;
        var result = await _mediator.Send(command, cancellationToken);
        return Ok(result);
    }

    [HttpPatch("{id:guid}/priority")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<TicketDto>> UpdatePriority(Guid id, UpdateTicketPriorityCommand command, CancellationToken cancellationToken)
    {
        command.TicketId = id;
        var result = await _mediator.Send(command, cancellationToken);
        return Ok(result);
    }

    [HttpPost("{id:guid}/comments")]
    public async Task<ActionResult<CommentDto>> AddComment(Guid id, AddCommentCommand command, CancellationToken cancellationToken)
    {
        command.TicketId = id;
        var result = await _mediator.Send(command, cancellationToken);
        return Ok(result);
    }

    [HttpPost("{id:guid}/time-entries")]
    [Authorize(Roles = "SupportAgent")]
    public async Task<ActionResult<TimeEntryDto>> AddTimeEntry(Guid id, AddTimeEntryCommand command, CancellationToken cancellationToken)
    {
        command.TicketId = id;
        var result = await _mediator.Send(command, cancellationToken);
        return Ok(result);
    }

    [HttpGet("{id:guid}/time-entries")]
    [Authorize(Roles = "Admin,SupportAgent")]
    public async Task<ActionResult<List<TimeEntryDto>>> GetTimeEntries(Guid id, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetTicketTimeEntriesQuery { TicketId = id }, cancellationToken);
        return Ok(result);
    }

    [HttpGet("{id:guid}/timeline")]
    public async Task<ActionResult<List<ActivityDto>>> GetTimeline(Guid id, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetTicketTimelineQuery { TicketId = id }, cancellationToken);
        return Ok(result);
    }

    [HttpPost("{id:guid}/close")]
    [Authorize(Roles = "Admin,Customer")]
    public async Task<ActionResult<TicketDto>> Close(Guid id, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new CloseTicketCommand { TicketId = id }, cancellationToken);
        return Ok(result);
    }
}
