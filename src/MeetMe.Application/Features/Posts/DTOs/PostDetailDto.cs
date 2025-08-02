using AutoMapper;
using MeetMe.Application.Common.Mappings;
using MeetMe.Application.Features.Users.DTOs;
using MeetMe.Application.Features.Meetings.DTOs;
using MeetMe.Application.Features.Comments.DTOs;
using MeetMe.Domain.Entities;

namespace MeetMe.Application.Features.Posts.DTOs
{
    /// <summary>
    /// Detailed post DTO with author information and comments
    /// </summary>
    public record PostDetailDto : IMapFrom<Post>
    {
        public int Id { get; init; }
        public string? Title { get; init; }
        public string Content { get; init; } = string.Empty;
        public int AuthorId { get; init; }
        public UserSummaryDto Author { get; init; } = new();
        public int MeetingId { get; init; }
        public MeetingSummaryDto Meeting { get; init; } = new();
        public List<CommentDto> Comments { get; init; } = new();
        public int Likes { get; init; }
        public bool IsLiked { get; init; }
        public bool IsBookmarked { get; init; }
        public List<string> Tags { get; init; } = new();
        public string CreatedAt { get; init; } = string.Empty;
        public string UpdatedAt { get; init; } = string.Empty;

        public void Mapping(Profile profile)
        {
            profile.CreateMap<Post, PostDetailDto>()
                .ForMember(dest => dest.AuthorId, opt => opt.MapFrom(src => src.AuthorId))
                .ForMember(dest => dest.MeetingId, opt => opt.MapFrom(src => src.MeetingId))
                .ForMember(dest => dest.Comments, opt => opt.MapFrom(src => src.Comments.Where(c => c.IsActive).ToList()))
                .ForMember(dest => dest.Likes, opt => opt.MapFrom(src => 0)) // Placeholder for now
                .ForMember(dest => dest.IsLiked, opt => opt.MapFrom(src => false)) // Placeholder for now
                .ForMember(dest => dest.IsBookmarked, opt => opt.MapFrom(src => false)) // Placeholder for now
                .ForMember(dest => dest.Tags, opt => opt.MapFrom(src => new List<string>())) // Placeholder for now
                .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => src.CreatedDate.ToString("yyyy-MM-ddTHH:mm:ss.fffZ")))
                .ForMember(dest => dest.UpdatedAt, opt => opt.MapFrom(src => (src.LastModifiedDate ?? src.CreatedDate).ToString("yyyy-MM-ddTHH:mm:ss.fffZ")));
        }
    }
}
