using MediatR;
using MeetMe.Application.Common.Interfaces;
using MeetMe.Application.Common.Models;
using MeetMe.Domain.Entities;

namespace MeetMe.Application.Features.Users.Queries.GetUser;

public class GetUserByIdQueryHandler : IRequestHandler<GetUserByIdQuery, Result<UserDto>>
{
    private readonly IQueryRepository<User, Guid> _userRepository;

    public GetUserByIdQueryHandler(IQueryRepository<User, Guid> userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task<Result<UserDto>> Handle(GetUserByIdQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var user = await _userRepository.GetByIdAsync(request.Id, cancellationToken);
            
            if (user == null)
            {
                return Result.Failure<UserDto>("User not found");
            }

            var userDto = new UserDto
            {
                Id = user.Id,
                FirstName = user.FirstName,
                LastName = user.LastName,
                FullName = $"{user.FirstName} {user.LastName}",
                Email = user.Email.Value,
                Bio = user.Bio,
                ProfilePictureUrl = user.ProfilePictureUrl,
                IsActive = user.IsActive,
                CreatedDate = user.CreatedDate
            };

            return Result.Success(userDto);
        }
        catch (Exception ex)
        {
            return Result.Failure<UserDto>($"Failed to get user: {ex.Message}");
        }
    }
}
