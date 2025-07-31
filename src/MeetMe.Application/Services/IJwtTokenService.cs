using MeetMe.Domain.Entities;

namespace MeetMe.Application.Services;

public interface IJwtTokenService
{
    string GenerateToken(User user);
    string GenerateRefreshToken();
    bool ValidateToken(string token);
    Guid GetUserIdFromToken(string token);
}
