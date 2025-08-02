using AutoMapper;
using MeetMe.Application.Common.Mappings;
using MeetMe.Domain.Entities;

namespace MeetMe.Application.Features.Users.DTOs
{
    /// <summary>
    /// Detailed user DTO for profile views
    /// </summary>
    public record UserDetailDto : IMapFrom<User>
    {
        public int Id { get; init; }
        public string FirstName { get; init; } = string.Empty;
        public string LastName { get; init; } = string.Empty;
        public string FullName { get; init; } = string.Empty;
        public string Email { get; init; } = string.Empty;
        public string? Bio { get; init; }
        public string? ProfilePictureUrl { get; init; }
        public string CreatedAt { get; init; } = string.Empty;
        public string UpdatedAt { get; init; } = string.Empty;
        public int MeetingsCreatedCount { get; init; }
        public int MeetingsAttendedCount { get; init; }
        public int PostsCount { get; init; }

        public void Mapping(Profile profile)
        {
            profile.CreateMap<User, UserDetailDto>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.FullName, opt => opt.MapFrom(src => src.FullName))
                .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.Email.Value))
                .ForMember(dest => dest.MeetingsCreatedCount, opt => opt.MapFrom(src => src.CreatedMeetings.Count(m => m.IsActive)))
                .ForMember(dest => dest.MeetingsAttendedCount, opt => opt.MapFrom(src => src.Attendances.Count(a => a.IsActive)))
                .ForMember(dest => dest.PostsCount, opt => opt.MapFrom(src => src.Posts.Count(p => p.IsActive)))
                .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => src.CreatedDate.ToString("yyyy-MM-ddTHH:mm:ss.fffZ")))
                .ForMember(dest => dest.UpdatedAt, opt => opt.MapFrom(src => (src.LastModifiedDate ?? src.CreatedDate).ToString("yyyy-MM-ddTHH:mm:ss.fffZ")));
        }
    }
}
