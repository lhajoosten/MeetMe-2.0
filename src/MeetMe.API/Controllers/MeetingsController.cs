using MediatR;
using MeetMe.Application.Common.Models;
using MeetMe.Application.Features.Meetings.Commands.CreateMeeting;
using MeetMe.Application.Features.Meetings.Commands.UpdateMeeting;
using MeetMe.Application.Features.Meetings.Commands.DeleteMeeting;
using MeetMe.Application.Features.Meetings.Queries.GetMeeting;
using MeetMe.Application.Features.Meetings.Queries.GetAllMeetings;
using MeetMe.Application.Features.Meetings.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

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
        public async Task<ActionResult<List<MeetingSummaryDto>>> GetMeetings([FromQuery] GetMeetingsQuery query)
        {
            var result = await _mediator.Send(query);

            if (!result.IsSuccess)
                return BadRequest(result.Error);

            return Ok(result.Value);
        }

        [HttpPost]
        public async Task<ActionResult<int>> CreateMeeting([FromBody] CreateMeetingDto request)
        {
            // Get the current user ID from the JWT token
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out var userId))
            {
                return Unauthorized("User ID not found in token");
            }

            // Create the command with proper mapping
            var command = new CreateMeetingCommand
            {
                Title = request.Title,
                Description = request.Description,
                Location = request.Location,
                StartDateTime = request.StartDate,
                EndDateTime = request.EndDate,
                MaxAttendees = request.MaxAttendees,
                IsPublic = request.IsPublic,
                CreatorId = userId
            };

            var result = await _mediator.Send(command);

            if (!result.IsSuccess)
                return BadRequest(result.Error);

            return CreatedAtAction(nameof(GetMeeting), new { id = result.Value }, result.Value);
        }

        [HttpGet("{id:int}")]
        public async Task<ActionResult<MeetingDetailDto>> GetMeeting(int id)
        {
            var query = new GetMeetingByIdQuery(id);
            var result = await _mediator.Send(query);

            if (!result.IsSuccess)
                return NotFound(result.Error);

            return Ok(result.Value);
        }

        [HttpPut("{id:int}")]
        public async Task<ActionResult> UpdateMeeting(int id, [FromBody] UpdateMeetingCommand command)
        {
            if (id != command.Id)
                return BadRequest("ID in URL does not match ID in request body");

            var result = await _mediator.Send(command);

            if (!result.IsSuccess)
                return BadRequest(result.Error);

            return NoContent();
        }

        [HttpDelete("{id:int}")]
        public async Task<ActionResult> DeleteMeeting(int id, [FromQuery] int userId)
        {
            var command = new DeleteMeetingCommand { Id = id, UserId = userId };
            var result = await _mediator.Send(command);

            if (!result.IsSuccess)
                return BadRequest(result.Error);

            return NoContent();
        }
    }
}