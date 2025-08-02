using AutoMapper;
using MeetMe.Application.Common.Mappings;
using MeetMe.Domain.Entities;

namespace MeetMe.Application.Features.Attendances.DTOs
{
    /// <summary>
    /// Lightweight attendance DTO for summary views and cross-feature references
    /// </summary>
    public record AttendanceSummaryDto : IMapFrom<Attendance>
    {
        public int Id { get; init; }
        public int UserId { get; init; }
        public int MeetingId { get; init; }
        public string Status { get; init; } = string.Empty;
        public string JoinedAt { get; init; } = string.Empty;

        public void Mapping(Profile profile)
        {
            profile.CreateMap<Attendance, AttendanceSummaryDto>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.UserId, opt => opt.MapFrom(src => src.UserId))
                .ForMember(dest => dest.MeetingId, opt => opt.MapFrom(src => src.MeetingId))
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToString()))
                .ForMember(dest => dest.JoinedAt, opt => opt.MapFrom(src => src.JoinedAt.ToString("yyyy-MM-ddTHH:mm:ss.fffZ")));
        }
    }
}
