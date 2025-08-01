using AutoMapper;
using MeetMe.Application.Common.Models;
using MeetMe.Domain.Entities;

namespace MeetMe.Application.Common.Mappings
{
    public class UserMappingProfile : Profile
    {
        public UserMappingProfile()
        {
            CreateMap<User, UserDto>()
                .ForMember(dest => dest.FullName, opt => opt.MapFrom(src => src.FullName))
                .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.Email.Value));
        }
    }
}
