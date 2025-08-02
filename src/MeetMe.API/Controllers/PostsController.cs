using MediatR;
using MeetMe.Application.Features.Posts.Commands.CreatePost;
using MeetMe.Application.Features.Posts.Commands.UpdatePost;
using MeetMe.Application.Features.Posts.Commands.DeletePost;
using MeetMe.Application.Features.Posts.Queries.GetPost;
using MeetMe.Application.Features.Posts.Queries.GetPostsByMeeting;
using MeetMe.Application.Features.Comments.Commands.CreateComment;
using MeetMe.Application.Features.Comments.Commands.UpdateComment;
using MeetMe.Application.Features.Comments.Commands.DeleteComment;
using MeetMe.Application.Features.Posts.DTOs;
using MeetMe.Application.Features.Comments.DTOs;
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
    public async Task<IActionResult> CreatePost([FromBody] CreatePostDto request, CancellationToken cancellationToken)
    {
        var userId = GetCurrentUserId();
        var command = new CreatePostCommand(request.Title ?? string.Empty, request.Content, request.MeetingId, userId);
        var result = await _mediator.Send(command, cancellationToken);

        if (!result.IsSuccess)
        {
            return BadRequest(new { message = result.Error });
        }

        return CreatedAtAction(nameof(GetPost), new { id = result.Value.Id }, result.Value);
    }

    /// <summary>
    /// Get all posts for a specific meeting
    /// </summary>
    /// <param name="meetingId">Meeting ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of posts for the meeting</returns>
    [HttpGet("meeting/{meetingId}")]
    public async Task<IActionResult> GetPostsByMeeting(int meetingId, CancellationToken cancellationToken)
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
    /// Update an existing post
    /// </summary>
    /// <param name="id">Post ID</param>
    /// <param name="request">Updated post data</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Updated post</returns>
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdatePost(int id, [FromBody] UpdatePostDto request, CancellationToken cancellationToken)
    {
        var userId = GetCurrentUserId();
        var command = new UpdatePostCommand(id, request.Title ?? string.Empty, request.Content, userId);
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

    /// <summary>
    /// Create a comment on a post
    /// </summary>
    /// <param name="postId">Post ID</param>
    /// <param name="request">Comment creation data</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Created comment</returns>
    [HttpPost("{postId}/comments")]
    public async Task<IActionResult> CreateComment(int postId, [FromBody] CreateCommentDto request, CancellationToken cancellationToken)
    {
        var userId = GetCurrentUserId();
        var command = new CreateCommentCommand(request.Content, postId, userId, request.ParentCommentId);
        var result = await _mediator.Send(command, cancellationToken);

        if (!result.IsSuccess)
        {
            return BadRequest(new { message = result.Error });
        }

        return Ok(result.Value);
    }

    /// <summary>
    /// Update a comment on a post
    /// </summary>
    /// <param name="postId">Post ID</param>
    /// <param name="commentId">Comment ID</param>
    /// <param name="request">Updated comment data</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Updated comment</returns>
    [HttpPut("{postId}/comments/{commentId}")]
    public async Task<IActionResult> UpdateComment(int postId, int commentId, [FromBody] UpdateCommentDto request, CancellationToken cancellationToken)
    {
        var userId = GetCurrentUserId();
        var command = new UpdateCommentCommand(commentId, request.Content, userId);
        var result = await _mediator.Send(command, cancellationToken);

        if (!result.IsSuccess)
        {
            return BadRequest(new { message = result.Error });
        }

        return Ok(result.Value);
    }

    /// <summary>
    /// Delete a comment on a post
    /// </summary>
    /// <param name="postId">Post ID</param>
    /// <param name="commentId">Comment ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Success status</returns>
    [HttpDelete("{postId}/comments/{commentId}")]
    public async Task<IActionResult> DeleteComment(int postId, int commentId, CancellationToken cancellationToken)
    {
        var userId = GetCurrentUserId();
        var command = new DeleteCommentCommand(commentId, userId);
        var result = await _mediator.Send(command, cancellationToken);

        if (!result.IsSuccess)
        {
            return BadRequest(new { message = result.Error });
        }

        return NoContent();
    }

    private int GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return int.TryParse(userIdClaim, out var userId) ? userId : 0;
    }
}
