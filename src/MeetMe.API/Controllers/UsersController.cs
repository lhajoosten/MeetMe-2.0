using MediatR;
using MeetMe.Application.Common.Models;
using MeetMe.Application.Features.Users.Commands.CreateUser;
using MeetMe.Application.Features.Users.Queries.GetUser;
using MeetMe.Application.Features.Users.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace MeetMe.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class UsersController : ControllerBase
{
    private readonly IMediator _mediator;

    public UsersController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost]
    [AllowAnonymous] // Allow anonymous for user registration
    public async Task<ActionResult<int>> CreateUser([FromBody] CreateUserCommand command)
    {
        var result = await _mediator.Send(command);

        if (!result.IsSuccess)
            return BadRequest(result.Error);

        return CreatedAtAction(nameof(GetUser), new { id = result.Value }, result.Value);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<UserDetailDto>> GetUser(int id)
    {
        var query = new GetUserByIdQuery(id);
        var result = await _mediator.Send(query);

        if (!result.IsSuccess)
            return NotFound(result.Error);

        return Ok(result.Value);
    }

    [HttpGet("profile")]
    public async Task<ActionResult<UserDetailDto>> GetCurrentUserProfile()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId) || !int.TryParse(userId, out var userInt))
            return Unauthorized();

        var query = new GetUserByIdQuery(userInt);
        var result = await _mediator.Send(query);

        if (!result.IsSuccess)
            return NotFound(result.Error);

        return Ok(result.Value);
    }

    [HttpPost("profile/picture")]
    public ActionResult<string> UploadProfilePicture(IFormFile file)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId) || !Guid.TryParse(userId, out var userGuid))
            return Unauthorized();

        if (file == null || file.Length == 0)
            return BadRequest("No file uploaded");

        // For now, return a mock response. We'll need to implement the actual file upload service
        return Ok(new { profilePictureUrl = $"/uploads/profiles/{userGuid}/{file.FileName}" });
    }
}
