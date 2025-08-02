using AutoMapper;
using MeetMe.Application.Common.Mappings;
using MeetMe.Domain.Entities;

namespace MeetMe.Application.Features.Meetings.DTOs
{
    /// <summary>
    /// Standard meeting DTO for list views and basic operations
    /// </summary>
    public record MeetingDto : IMapFrom<Meeting>
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
        public string CreatorName { get; init; } = string.Empty;
        public int AttendeeCount { get; init; }
        public int PostCount { get; init; }
        public bool IsUpcoming { get; init; }
        public string CreatedAt { get; init; } = string.Empty;

        public void Mapping(Profile profile)
        {
            profile.CreateMap<Meeting, MeetingDto>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.CreatorId, opt => opt.MapFrom(src => src.CreatorId))
                .ForMember(dest => dest.CreatorName, opt => opt.MapFrom(src => src.Creator != null ? src.Creator.FullName : string.Empty))
                .ForMember(dest => dest.Location, opt => opt.MapFrom(src => src.Location.Value))
                .ForMember(dest => dest.AttendeeCount, opt => opt.MapFrom(src => src.Attendees.Count(a => a.IsActive)))
                .ForMember(dest => dest.PostCount, opt => opt.MapFrom(src => src.Posts.Count(p => p.IsActive)))
                .ForMember(dest => dest.IsUpcoming, opt => opt.MapFrom(src => src.MeetingDateTime.StartDateTime > DateTime.UtcNow))
                .ForMember(dest => dest.StartDateTime, opt => opt.MapFrom(src => src.MeetingDateTime.StartDateTime.ToString("yyyy-MM-ddTHH:mm:ss.fffZ")))
                .ForMember(dest => dest.EndDateTime, opt => opt.MapFrom(src => src.MeetingDateTime.EndDateTime.ToString("yyyy-MM-ddTHH:mm:ss.fffZ")))
                .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => src.CreatedDate.ToString("yyyy-MM-ddTHH:mm:ss.fffZ")));
        }
    }
}
