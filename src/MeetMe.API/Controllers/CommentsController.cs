using MediatR;
using MeetMe.Application.Features.Comments.Commands.CreateComment;
using MeetMe.Application.Features.Comments.Commands.DeleteComment;
using MeetMe.Application.Features.Comments.Commands.UpdateComment;
using MeetMe.Application.Features.Comments.Queries.GetComment;
using MeetMe.Application.Features.Comments.Queries.GetCommentsByPost;
using MeetMe.Application.Features.Comments.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace MeetMe.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class CommentsController : ControllerBase
{
    private readonly IMediator _mediator;

    public CommentsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost]
    public async Task<ActionResult<CommentDto>> CreateComment([FromBody] CreateCommentDto request)
    {
        var userId = GetCurrentUserId();
        var command = new CreateCommentCommand(
            request.Content,
            request.PostId,
            userId,
            request.ParentCommentId);

        var result = await _mediator.Send(command);

        if (result.IsFailure)
        {
            return BadRequest(result.Error);
        }

        return CreatedAtAction(nameof(GetComment), new { id = result.Value.Id }, result.Value);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<CommentDto>> GetComment(int id)
    {
        var query = new GetCommentQuery(id);
        var result = await _mediator.Send(query);

        if (result.IsFailure)
        {
            return NotFound(result.Error);
        }

        return Ok(result.Value);
    }

    [HttpGet("post/{postId}")]
    public async Task<ActionResult<List<CommentDto>>> GetCommentsByPost(
        int postId,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        var query = new GetCommentsByPostQuery(postId, page, pageSize);
        var result = await _mediator.Send(query);

        if (result.IsFailure)
        {
            return BadRequest(result.Error);
        }

        return Ok(result.Value);
    }

    [HttpPut("{id}")]
    public async Task<ActionResult> UpdateComment(int id, [FromBody] UpdateCommentDto request)
    {
        var userId = GetCurrentUserId();
        var command = new UpdateCommentCommand(id, request.Content, userId);

        var result = await _mediator.Send(command);

        if (result.IsFailure)
        {
            return BadRequest(result.Error);
        }

        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult> DeleteComment(int id)
    {
        var userId = GetCurrentUserId();
        var command = new DeleteCommentCommand(id, userId);

        var result = await _mediator.Send(command);

        if (result.IsFailure)
        {
            return BadRequest(result.Error);
        }

        return NoContent();
    }

    private int GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out var userId))
        {
            throw new UnauthorizedAccessException("Invalid user ID in token");
        }
        return userId;
    }
}
