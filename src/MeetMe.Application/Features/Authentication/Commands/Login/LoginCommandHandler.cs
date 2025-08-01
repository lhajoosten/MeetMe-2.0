using AutoMapper;
using MediatR;
using MeetMe.Application.Common.Interfaces;
using MeetMe.Application.Common.Models;
using MeetMe.Application.Features.Authentication.DTOs;
using MeetMe.Application.Services;
using MeetMe.Domain.Entities;
using MeetMe.Domain.ValueObjects;

namespace MeetMe.Application.Features.Authentication.Commands.Login;

public class LoginCommandHandler : IRequestHandler<LoginCommand, Result<AuthenticationResult>>
{
    private readonly IQueryRepository<User, Guid> _userQueryRepository;
    private readonly IJwtTokenService _jwtTokenService;
    private readonly IPasswordService _passwordService;
    private readonly IMapper _mapper;

    public LoginCommandHandler(
        IQueryRepository<User, Guid> userQueryRepository,
        IJwtTokenService jwtTokenService,
        IPasswordService passwordService,
        IMapper mapper)
    {
        _userQueryRepository = userQueryRepository;
        _jwtTokenService = jwtTokenService;
        _passwordService = passwordService;
        _mapper = mapper;
    }

    public async Task<Result<AuthenticationResult>> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        try
        {
            // Validate email format
            if (!Email.IsValidEmail(request.Email))
            {
                return Result.Failure<AuthenticationResult>("Invalid email format");
            }

            // Find user by email
            var email = Email.Create(request.Email);
            var user = await _userQueryRepository.FirstOrDefaultAsync(u => u.Email == email, cancellationToken);

            if (user == null)
            {
                return Result.Failure<AuthenticationResult>("Invalid email or password");
            }

            // Verify password
            if (!_passwordService.VerifyPassword(request.Password, user.PasswordHash))
            {
                return Result.Failure<AuthenticationResult>("Invalid email or password");
            }

            // Check if user is active
            if (!user.IsActive)
            {
                return Result.Failure<AuthenticationResult>("Account is inactive");
            }

            // Generate JWT token
            var token = _jwtTokenService.GenerateToken(user);
            var refreshToken = _jwtTokenService.GenerateRefreshToken();

            var userDto = _mapper.Map<UserDto>(user);

            var authResult = new AuthenticationResult
            {
                Success = true,
                Token = token,
                RefreshToken = refreshToken,
                ExpiresAt = DateTime.UtcNow.AddHours(24), // 24 hour expiry
                User = userDto
            };

            return Result.Success(authResult);
        }
        catch (Exception ex)
        {
            return Result.Failure<AuthenticationResult>($"Login failed: {ex.Message}");
        }
    }
}
