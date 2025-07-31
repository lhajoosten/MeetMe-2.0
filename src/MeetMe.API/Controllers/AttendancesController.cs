using MediatR;
using MeetMe.Application.Features.Attendances.Commands.JoinMeeting;
using MeetMe.Application.Features.Attendances.Commands.LeaveMeeting;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MeetMe.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class AttendancesController : ControllerBase
{
    private readonly IMediator _mediator;

    public AttendancesController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost("join")]
    public async Task<ActionResult<Guid>> JoinMeeting([FromBody] JoinMeetingCommand command)
    {
        var result = await _mediator.Send(command);

        if (!result.IsSuccess)
            return BadRequest(result.Error);

        return Ok(result.Value);
    }

    [HttpPost("leave")]
    public async Task<ActionResult> LeaveMeeting([FromBody] LeaveMeetingCommand command)
    {
        var result = await _mediator.Send(command);

        if (!result.IsSuccess)
            return BadRequest(result.Error);

        return NoContent();
    }
}
