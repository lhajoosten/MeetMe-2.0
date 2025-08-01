using MeetMe.Application.Common.Models;
using MediatR;

namespace MeetMe.Application.Features.Search.Queries.GetPopularSearchTerms;

public record GetPopularSearchTermsQuery(
    int Count = 10
) : IRequest<Result<List<string>>>;
