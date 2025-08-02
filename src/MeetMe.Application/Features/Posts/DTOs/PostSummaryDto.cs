using AutoMapper;
using MeetMe.Application.Common.Mappings;
using MeetMe.Application.Features.Users.DTOs;
using MeetMe.Domain.Entities;

namespace MeetMe.Application.Features.Posts.DTOs
{
    /// <summary>
    /// Lightweight post DTO for cross-feature references and list views
    /// </summary>
    public record PostSummaryDto : IMapFrom<Post>
    {
        public int Id { get; init; }
        public string? Title { get; init; }
        public string Content { get; init; } = string.Empty;
        public int AuthorId { get; init; }
        public string AuthorName { get; init; } = string.Empty;
        public int MeetingId { get; init; }
        public string CreatedAt { get; init; } = string.Empty;
        public int CommentsCount { get; init; }

        public void Mapping(Profile profile)
        {
            profile.CreateMap<Post, PostSummaryDto>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.AuthorId, opt => opt.MapFrom(src => src.AuthorId))
                .ForMember(dest => dest.MeetingId, opt => opt.MapFrom(src => src.MeetingId))
                .ForMember(dest => dest.AuthorName, opt => opt.MapFrom(src => src.Author.FullName))
                .ForMember(dest => dest.CommentsCount, opt => opt.MapFrom(src => src.Comments.Count(c => c.IsActive)))
                .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => src.CreatedDate.ToString("yyyy-MM-ddTHH:mm:ss.fffZ")));
        }
    }
}
