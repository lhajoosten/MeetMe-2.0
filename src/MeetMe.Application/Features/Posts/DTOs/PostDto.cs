using AutoMapper;
using MeetMe.Application.Common.Mappings;
using MeetMe.Domain.Entities;

namespace MeetMe.Application.Features.Posts.DTOs
{
    public record PostDto : IMapFrom<Post>
    {
        public int Id { get; init; }
        public string Title { get; init; } = string.Empty;
        public string Content { get; init; } = string.Empty;
        public int AuthorId { get; init; }
        public string AuthorName { get; init; } = string.Empty;
        public int MeetingId { get; init; }
        public bool IsActive { get; init; }
        public int CommentCount { get; init; }
        public DateTime CreatedDate { get; init; }
        public DateTime? LastModifiedDate { get; init; }

        public void Mapping(Profile profile)
        {
            profile.CreateMap<Post, PostDto>()
                .ForMember(dest => dest.AuthorName, opt => opt.MapFrom(src => src.Author.FullName))
                .ForMember(dest => dest.CommentCount, opt => opt.MapFrom(src => src.Comments.Count(c => c.IsActive)));
        }
    }
}
