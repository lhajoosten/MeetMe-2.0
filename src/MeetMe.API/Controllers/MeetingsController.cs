using MediatR;
using MeetMe.Application.Common.Models;
using MeetMe.Application.Features.Meetings.Commands.CreateMeeting;
using MeetMe.Application.Features.Meetings.Commands.UpdateMeeting;
using MeetMe.Application.Features.Meetings.Commands.DeleteMeeting;
using MeetMe.Application.Features.Meetings.Queries.GetMeeting;
using MeetMe.Application.Features.Meetings.Queries.GetAllMeetings;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MeetMe.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class MeetingsController : ControllerBase
    {
        private readonly IMediator _mediator;

        public MeetingsController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet]
        public async Task<ActionResult<List<MeetingDto>>> GetMeetings([FromQuery] GetMeetingsQuery query)
        {
            var result = await _mediator.Send(query);

            if (!result.IsSuccess)
                return BadRequest(result.Error);

            return Ok(result.Value);
        }

        [HttpPost]
        public async Task<ActionResult<Guid>> CreateMeeting([FromBody] CreateMeetingCommand command)
        {
            var result = await _mediator.Send(command);

            if (!result.IsSuccess)
                return BadRequest(result.Error);

            return CreatedAtAction(nameof(GetMeeting), new { id = result.Value }, result.Value);
        }

        [HttpGet("{id:guid}")]
        public async Task<ActionResult<MeetingDto>> GetMeeting(Guid id)
        {
            var query = new GetMeetingByIdQuery(id);
            var result = await _mediator.Send(query);

            if (!result.IsSuccess)
                return NotFound(result.Error);

            return Ok(result.Value);
        }

        [HttpPut("{id:guid}")]
        public async Task<ActionResult> UpdateMeeting(Guid id, [FromBody] UpdateMeetingCommand command)
        {
            if (id != command.Id)
                return BadRequest("ID in URL does not match ID in request body");

            var result = await _mediator.Send(command);

            if (!result.IsSuccess)
                return BadRequest(result.Error);

            return NoContent();
        }

        [HttpDelete("{id:guid}")]
        public async Task<ActionResult> DeleteMeeting(Guid id, [FromQuery] Guid userId)
        {
            var command = new DeleteMeetingCommand { Id = id, UserId = userId };
            var result = await _mediator.Send(command);

            if (!result.IsSuccess)
                return BadRequest(result.Error);

            return NoContent();
        }
    }
}