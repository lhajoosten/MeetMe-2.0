using MediatR;
using MeetMe.Application.Common.Interfaces;
using MeetMe.Application.Common.Models;
using MeetMe.Domain.Entities;
using MeetMe.Domain.ValueObjects;

namespace MeetMe.Application.Features.Users.Commands.CreateUser;

public class CreateUserCommandHandler : IRequestHandler<CreateUserCommand, Result<Guid>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IQueryRepository<User, Guid> _userQueryRepository;

    public CreateUserCommandHandler(
        IUnitOfWork unitOfWork,
        IQueryRepository<User, Guid> userQueryRepository)
    {
        _unitOfWork = unitOfWork;
        _userQueryRepository = userQueryRepository;
    }

    public async Task<Result<Guid>> Handle(CreateUserCommand request, CancellationToken cancellationToken)
    {
        try
        {
            // Check if user with this email already exists
            var existingUser = await _userQueryRepository
                .FirstOrDefaultAsync(u => u.Email.Value == request.Email, cancellationToken);

            if (existingUser != null)
            {
                return Result.Failure<Guid>("A user with this email already exists");
            }

            // Create the user using domain factory method
            var user = User.Create(
                request.FirstName,
                request.LastName,
                request.Email);

            // Add to repository
            var userRepository = _unitOfWork.CommandRepository<User, Guid>();
            await userRepository.AddAsync(user, "System", cancellationToken);

            // Save changes
            await _unitOfWork.SaveChangesAsync("System", cancellationToken);

            return Result.Success(user.Id);
        }
        catch (Exception ex)
        {
            return Result.Failure<Guid>($"Failed to create user: {ex.Message}");
        }
    }
}
