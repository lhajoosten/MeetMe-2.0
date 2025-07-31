using AutoMapper;
using MeetMe.Application.Common.Models;
using MeetMe.Domain.Entities;

namespace MeetMe.Application.Common.Mappings
{
    public class MeetingMappingProfile : Profile
    {
        public MeetingMappingProfile()
        {
            CreateMap<Meeting, MeetingDto>()
                .ForMember(dest => dest.Location, opt => opt.MapFrom(src => src.Location.Value))
                .ForMember(dest => dest.StartDateTime, opt => opt.MapFrom(src => src.MeetingDateTime.StartDateTime))
                .ForMember(dest => dest.EndDateTime, opt => opt.MapFrom(src => src.MeetingDateTime.EndDateTime))
                .ForMember(dest => dest.CreatorName, opt => opt.MapFrom(src => src.Creator.FullName))
                .ForMember(dest => dest.AttendeeCount, opt => opt.MapFrom(src => src.Attendees.Count(a => a.IsActive)))
                .ForMember(dest => dest.PostCount, opt => opt.MapFrom(src => src.Posts.Count(p => p.IsActive)))
                .ForMember(dest => dest.IsUpcoming, opt => opt.MapFrom(src => src.MeetingDateTime.StartDateTime > DateTime.UtcNow));
        }
    }
}
