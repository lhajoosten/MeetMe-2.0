using MediatR;
using MeetMe.Application.Features.Posts.Commands.CreatePost;
using MeetMe.Application.Features.Posts.Commands.UpdatePost;
using MeetMe.Application.Features.Posts.Commands.DeletePost;
using MeetMe.Application.Features.Posts.Queries.GetPost;
using MeetMe.Application.Features.Posts.Queries.GetPostsByMeeting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace MeetMe.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class PostsController : ControllerBase
{
    private readonly IMediator _mediator;

    public PostsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Create a new post for a meeting
    /// </summary>
    /// <param name="request">Post creation data</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Created post</returns>
    [HttpPost]
    public async Task<IActionResult> CreatePost([FromBody] CreatePostRequest request, CancellationToken cancellationToken)
    {
        var userId = GetCurrentUserId();
        var command = new CreatePostCommand(request.Title, request.Content, request.MeetingId, userId);
        var result = await _mediator.Send(command, cancellationToken);

        if (!result.IsSuccess)
        {
            return BadRequest(new { message = result.Error });
        }

        return CreatedAtAction(nameof(GetPost), new { id = result.Value.Id }, result.Value);
    }

    /// <summary>
    /// Get a specific post by ID
    /// </summary>
    /// <param name="id">Post ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Post details</returns>
    [HttpGet("{id}")]
    public async Task<IActionResult> GetPost(int id, CancellationToken cancellationToken)
    {
        var query = new GetPostByIdQuery(id);
        var result = await _mediator.Send(query, cancellationToken);

        if (!result.IsSuccess)
        {
            return NotFound(new { message = result.Error });
        }

        return Ok(result.Value);
    }

    /// <summary>
    /// Get all posts for a specific meeting
    /// </summary>
    /// <param name="meetingId">Meeting ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of posts</returns>
    [HttpGet("meeting/{meetingId}")]
    public async Task<IActionResult> GetPostsByMeeting(Guid meetingId, CancellationToken cancellationToken)
    {
        var query = new GetPostsByMeetingQuery(meetingId);
        var result = await _mediator.Send(query, cancellationToken);

        if (!result.IsSuccess)
        {
            return BadRequest(new { message = result.Error });
        }

        return Ok(result.Value);
    }

    /// <summary>
    /// Update an existing post
    /// </summary>
    /// <param name="id">Post ID</param>
    /// <param name="request">Updated post data</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Updated post</returns>
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdatePost(int id, [FromBody] UpdatePostRequest request, CancellationToken cancellationToken)
    {
        var userId = GetCurrentUserId();
        var command = new UpdatePostCommand(id, request.Title, request.Content, userId);
        var result = await _mediator.Send(command, cancellationToken);

        if (!result.IsSuccess)
        {
            return BadRequest(new { message = result.Error });
        }

        return Ok(result.Value);
    }

    /// <summary>
    /// Delete a post
    /// </summary>
    /// <param name="id">Post ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Success status</returns>
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeletePost(int id, CancellationToken cancellationToken)
    {
        var userId = GetCurrentUserId();
        var command = new DeletePostCommand(id, userId);
        var result = await _mediator.Send(command, cancellationToken);

        if (!result.IsSuccess)
        {
            return BadRequest(new { message = result.Error });
        }

        return NoContent();
    }

    private Guid GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return Guid.TryParse(userIdClaim, out var userId) ? userId : Guid.Empty;
    }
}

// Request DTOs
public class CreatePostRequest
{
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public Guid MeetingId { get; set; }
}

public class UpdatePostRequest
{
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
}
