using AutoMapper;
using MeetMe.Application.Common.Mappings;
using MeetMe.Domain.Entities;

namespace MeetMe.Application.Features.Users.DTOs
{
    /// <summary>
    /// Lightweight user DTO for references in other features (comments, posts, etc.)
    /// </summary>
    public record UserSummaryDto : IMapFrom<User>
    {
        public string Id { get; init; } = string.Empty;
        public string FirstName { get; init; } = string.Empty;
        public string LastName { get; init; } = string.Empty;
        public string FullName { get; init; } = string.Empty;
        public string Email { get; init; } = string.Empty;
        public string? ProfilePictureUrl { get; init; }

        public void Mapping(Profile profile)
        {
            profile.CreateMap<User, UserSummaryDto>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id.ToString()))
                .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.Email.Value))
                .ForMember(dest => dest.FullName, opt => opt.MapFrom(src => src.FullName));
        }
    }
}
