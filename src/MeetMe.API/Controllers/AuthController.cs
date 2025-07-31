using MediatR;
using MeetMe.Application.Features.Authentication.Commands;
using MeetMe.Application.Features.Authentication.DTOs;
using Microsoft.AspNetCore.Mvc;

namespace MeetMe.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IMediator _mediator;

    public AuthController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Login with email and password
    /// </summary>
    /// <param name="request">Login credentials</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Authentication result with JWT token</returns>
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request, CancellationToken cancellationToken)
    {
        var command = new LoginCommand(request.Email, request.Password);
        var result = await _mediator.Send(command, cancellationToken);

        if (!result.IsSuccess)
        {
            return BadRequest(new { message = result.Error });
        }

        return Ok(result.Value);
    }

    /// <summary>
    /// Register a new user account
    /// </summary>
    /// <param name="request">Registration details</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Authentication result with JWT token</returns>
    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request, CancellationToken cancellationToken)
    {
        var command = new RegisterCommand(
            request.Email,
            request.Password,
            request.ConfirmPassword,
            request.FirstName,
            request.LastName);

        var result = await _mediator.Send(command, cancellationToken);

        if (!result.IsSuccess)
        {
            return BadRequest(new { message = result.Error });
        }

        return Ok(result.Value);
    }
}
