using AutoMapper;
using MeetMe.Application.Common.Mappings;
using MeetMe.Domain.Entities;

namespace MeetMe.Application.Features.Comments.DTOs
{
    /// <summary>
    /// Standard comment DTO for list views and nested comments
    /// </summary>
    public record CommentDto : IMapFrom<Comment>
    {
        public int Id { get; init; }
        public string Content { get; init; } = string.Empty;
        public int AuthorId { get; init; }
        public string AuthorName { get; init; } = string.Empty;
        public int PostId { get; init; }
        public int? ParentCommentId { get; init; }
        public List<CommentDto> Replies { get; init; } = new();
        public string CreatedAt { get; init; } = string.Empty;
        public string UpdatedAt { get; init; } = string.Empty;

        public void Mapping(Profile profile)
        {
            profile.CreateMap<Comment, CommentDto>()
                .ForMember(dest => dest.AuthorId, opt => opt.MapFrom(src => src.AuthorId))
                .ForMember(dest => dest.AuthorName, 
                    opt => opt.MapFrom(src => src.Author != null ? src.Author.FirstName + " " + src.Author.LastName : string.Empty))
                .ForMember(dest => dest.Replies, 
                    opt => opt.MapFrom(src => src.Replies.Where(c => c.IsActive).ToList()))
                .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => src.CreatedDate.ToString("yyyy-MM-ddTHH:mm:ss.fffZ")))
                .ForMember(dest => dest.UpdatedAt, opt => opt.MapFrom(src => (src.LastModifiedDate ?? src.CreatedDate).ToString("yyyy-MM-ddTHH:mm:ss.fffZ")));
        }
    }
}
