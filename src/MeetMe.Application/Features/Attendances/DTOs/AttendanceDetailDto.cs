using AutoMapper;
using MeetMe.Application.Common.Mappings;
using MeetMe.Application.Features.Users.DTOs;
using MeetMe.Domain.Entities;

namespace MeetMe.Application.Features.Attendances.DTOs
{
    /// <summary>
    /// Detailed attendance DTO with user information
    /// </summary>
    public record AttendanceDetailDto : IMapFrom<Attendance>
    {
        public int Id { get; init; }
        public int UserId { get; init; }
        public UserSummaryDto User { get; init; } = new();
        public int MeetingId { get; init; }
        public string Status { get; init; } = string.Empty;
        public string JoinedAt { get; init; } = string.Empty;
        public string CreatedAt { get; init; } = string.Empty;
        public string UpdatedAt { get; init; } = string.Empty;

        public void Mapping(Profile profile)
        {
            profile.CreateMap<Attendance, AttendanceDetailDto>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.UserId, opt => opt.MapFrom(src => src.UserId))
                .ForMember(dest => dest.MeetingId, opt => opt.MapFrom(src => src.MeetingId))
                .ForMember(dest => dest.User, opt => opt.MapFrom(src => src.User))
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToString()))
                .ForMember(dest => dest.JoinedAt, opt => opt.MapFrom(src => src.JoinedAt.ToString("yyyy-MM-ddTHH:mm:ss.fffZ")))
                .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => src.CreatedDate.ToString("yyyy-MM-ddTHH:mm:ss.fffZ")))
                .ForMember(dest => dest.UpdatedAt, opt => opt.MapFrom(src => (src.LastModifiedDate ?? src.CreatedDate).ToString("yyyy-MM-ddTHH:mm:ss.fffZ")));
        }
    }
}
