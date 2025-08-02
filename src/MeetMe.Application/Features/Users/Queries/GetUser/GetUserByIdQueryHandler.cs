using MeetMe.Application.Common.Abstraction;
using MeetMe.Application.Common.Interfaces;
using MeetMe.Application.Common.Models;
using MeetMe.Application.Features.Users.DTOs;
using MeetMe.Domain.Entities;
using AutoMapper;

namespace MeetMe.Application.Features.Users.Queries.GetUser;

public class GetUserByIdQueryHandler : IQueryHandler<GetUserByIdQuery, UserDetailDto>
{
    private readonly IQueryRepository<User, int> _userRepository;
    private readonly IMapper _mapper;

    public GetUserByIdQueryHandler(IQueryRepository<User, int> userRepository, IMapper mapper)
    {
        _userRepository = userRepository;
        _mapper = mapper;
    }

    public async Task<Result<UserDetailDto>> Handle(GetUserByIdQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var user = await _userRepository.GetByIdAsync(request.Id, cancellationToken);
            
            if (user == null)
            {
                return Result.Failure<UserDetailDto>("User not found");
            }

            var userDetailDto = _mapper.Map<UserDetailDto>(user);

            return Result.Success(userDetailDto);
        }
        catch (Exception ex)
        {
            return Result.Failure<UserDetailDto>($"Failed to get user: {ex.Message}");
        }
    }
}
