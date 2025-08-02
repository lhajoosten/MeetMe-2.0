using AutoMapper;
using MeetMe.Application.Common.Mappings;
using MeetMe.Application.Features.Users.DTOs;
using MeetMe.Domain.Entities;

namespace MeetMe.Application.Features.Comments.DTOs
{
    /// <summary>
    /// Detailed comment DTO with full author information for detailed views
    /// </summary>
    public record CommentDetailDto : IMapFrom<Comment>
    {
        public int Id { get; init; }
        public string Content { get; init; } = string.Empty;
        public int AuthorId { get; init; }
        public UserSummaryDto Author { get; init; } = new();
        public int PostId { get; init; }
        public int? ParentCommentId { get; init; }
        public List<CommentDetailDto> Replies { get; init; } = new();
        public string CreatedAt { get; init; } = string.Empty;
        public string UpdatedAt { get; init; } = string.Empty;

        public void Mapping(Profile profile)
        {
            profile.CreateMap<Comment, CommentDetailDto>()
                .ForMember(dest => dest.AuthorId, opt => opt.MapFrom(src => src.AuthorId))
                .ForMember(dest => dest.Author, opt => opt.MapFrom(src => src.Author))
                .ForMember(dest => dest.Replies, 
                    opt => opt.MapFrom(src => src.Replies.Where(c => c.IsActive).ToList()))
                .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => src.CreatedDate.ToString("yyyy-MM-ddTHH:mm:ss.fffZ")))
                .ForMember(dest => dest.UpdatedAt, opt => opt.MapFrom(src => (src.LastModifiedDate ?? src.CreatedDate).ToString("yyyy-MM-ddTHH:mm:ss.fffZ")));
        }
    }
}
