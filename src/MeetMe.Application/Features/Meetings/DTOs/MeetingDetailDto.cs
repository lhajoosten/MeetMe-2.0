using AutoMapper;
using MeetMe.Application.Common.Mappings;
using MeetMe.Application.Features.Users.DTOs;
using MeetMe.Application.Features.Attendances.DTOs;
using MeetMe.Application.Features.Posts.DTOs;
using MeetMe.Domain.Entities;

namespace MeetMe.Application.Features.Meetings.DTOs
{
    /// <summary>
    /// Detailed meeting DTO with full related entities for detail views
    /// </summary>
    public record MeetingDetailDto : IMapFrom<Meeting>
    {
        public int Id { get; init; }
        public string Title { get; init; } = string.Empty;
        public string Description { get; init; } = string.Empty;
        public string Location { get; init; } = string.Empty;
        public string StartDateTime { get; init; } = string.Empty;
        public string EndDateTime { get; init; } = string.Empty;
        public int? MaxAttendees { get; init; }
        public bool IsPublic { get; init; }
        public int CreatorId { get; init; }
        public UserSummaryDto Creator { get; init; } = new();
        public List<AttendanceDetailDto> Attendees { get; init; } = new();
        public List<PostSummaryDto> Posts { get; init; } = new();
        public bool IsUpcoming { get; init; }
        public string CreatedAt { get; init; } = string.Empty;
        public string UpdatedAt { get; init; } = string.Empty;

        public void Mapping(Profile profile)
        {
            profile.CreateMap<Meeting, MeetingDetailDto>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.CreatorId, opt => opt.MapFrom(src => src.CreatorId))
                .ForMember(dest => dest.Location, opt => opt.MapFrom(src => src.Location.Value))
                .ForMember(dest => dest.StartDateTime, opt => opt.MapFrom(src => src.MeetingDateTime.StartDateTime.ToString("yyyy-MM-ddTHH:mm:ss.fffZ")))
                .ForMember(dest => dest.EndDateTime, opt => opt.MapFrom(src => src.MeetingDateTime.EndDateTime.ToString("yyyy-MM-ddTHH:mm:ss.fffZ")))
                .ForMember(dest => dest.Creator, opt => opt.MapFrom(src => src.Creator))
                .ForMember(dest => dest.Attendees, opt => opt.MapFrom(src => src.Attendees.Where(a => a.IsActive)))
                .ForMember(dest => dest.Posts, opt => opt.MapFrom(src => src.Posts.Where(p => p.IsActive)))
                .ForMember(dest => dest.IsUpcoming, opt => opt.MapFrom(src => src.MeetingDateTime.StartDateTime > DateTime.UtcNow))
                .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => src.CreatedDate.ToString("yyyy-MM-ddTHH:mm:ss.fffZ")))
                .ForMember(dest => dest.UpdatedAt, opt => opt.MapFrom(src => (src.LastModifiedDate ?? src.CreatedDate).ToString("yyyy-MM-ddTHH:mm:ss.fffZ")));
        }
    }
}
