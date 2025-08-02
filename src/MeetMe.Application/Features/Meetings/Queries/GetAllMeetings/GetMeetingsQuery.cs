using MeetMe.Application.Common.Abstraction;
using MeetMe.Application.Features.Meetings.DTOs;
using System.Text.Json.Serialization;

namespace MeetMe.Application.Features.Meetings.Queries.GetAllMeetings
{
    public record GetMeetingsQuery : IQuery<List<MeetingSummaryDto>>
    {
        public bool? IsUpcoming { get; init; }
        public bool? IsActive { get; init; } = true;
        
        [JsonPropertyName("isPublic")]
        public bool? IsPublic { get; init; }
        
        public int? CreatorId { get; init; }
        
        [JsonPropertyName("query")]
        public string? SearchTerm { get; init; }
        
        public int Page { get; init; } = 1;
        public int PageSize { get; init; } = 10;
    }
}
