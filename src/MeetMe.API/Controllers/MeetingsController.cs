using MediatR;
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
            // Implementation for getting single meeting
            return Ok();
        }
    }
}