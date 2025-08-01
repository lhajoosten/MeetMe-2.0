using AutoMapper;
using MediatR;
using MeetMe.Application.Common.Interfaces;
using MeetMe.Application.Common.Models;
using MeetMe.Application.Features.Authentication.DTOs;
using MeetMe.Application.Services;
using MeetMe.Domain.Entities;
using MeetMe.Domain.ValueObjects;
using Microsoft.AspNetCore.Identity;

namespace MeetMe.Application.Features.Authentication.Commands.Register;

public class RegisterCommandHandler : IRequestHandler<RegisterCommand, Result<AuthenticationResult>>
{
    private readonly ICommandRepository<User, Guid> _userCommandRepository;
    private readonly IQueryRepository<User, Guid> _userQueryRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IJwtTokenService _jwtTokenService;
    private readonly IPasswordService _passwordService;
    private readonly IMapper _mapper;
    private readonly RoleManager<Role> _roleManager;

    public RegisterCommandHandler(
        ICommandRepository<User, Guid> userCommandRepository,
        IQueryRepository<User, Guid> userQueryRepository,
        IUnitOfWork unitOfWork,
        IJwtTokenService jwtTokenService,
        IPasswordService passwordService,
        IMapper mapper,
        RoleManager<Role> roleManager)
    {
        _userCommandRepository = userCommandRepository;
        _userQueryRepository = userQueryRepository;
        _unitOfWork = unitOfWork;
        _jwtTokenService = jwtTokenService;
        _passwordService = passwordService;
        _mapper = mapper;
        _roleManager = roleManager;
    }

    public async Task<Result<AuthenticationResult>> Handle(RegisterCommand request, CancellationToken cancellationToken)
    {
        try
        {
            // Validate email format
            if (!Email.IsValidEmail(request.Email))
            {
                return Result.Failure<AuthenticationResult>("Invalid email format");
            }

            // Validate password match
            if (request.Password != request.ConfirmPassword)
            {
                return Result.Failure<AuthenticationResult>("Passwords do not match");
            }

            // Validate password strength (basic validation)
            if (request.Password.Length < 6)
            {
                return Result.Failure<AuthenticationResult>("Password must be at least 6 characters long");
            }

            // Check if user already exists
            var email = Email.Create(request.Email);
            var existingUser = await _userQueryRepository.FirstOrDefaultAsync(u => u.Email == email, cancellationToken);

            if (existingUser != null)
            {
                return Result.Failure<AuthenticationResult>("User with this email already exists");
            }

            // Hash password
            var hashedPassword = _passwordService.HashPassword(request.Password);

            // Get the default Member role
            var memberRole = await _roleManager.FindByNameAsync("Member");
            
            if (memberRole == null)
            {
                return Result.Failure<AuthenticationResult>("Default Member role not found. Please contact system administrator.");
            }

            // Create new user
            var user = User.Create(
                request.FirstName,
                request.LastName,
                email,
                hashedPassword);

            // Set the default role
            user.SetPrimaryRole(memberRole);

            // Use a temporary user ID for audit (the user doesn't exist yet)
            var tempUserId = "SYSTEM_REGISTRATION";
            await _userCommandRepository.AddAsync(user, tempUserId, cancellationToken);
            await _unitOfWork.SaveChangesAsync(tempUserId, cancellationToken);

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
            return Result.Failure<AuthenticationResult>($"Registration failed: {ex.Message}");
        }
    }
}
