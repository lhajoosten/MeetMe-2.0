using MediatR;
using MeetMe.Application.Common.Models;

namespace MeetMe.Application.Common.Abstraction
{
    /// <summary>
    /// Interface for query handlers
    /// </summary>
    /// <typeparam name="TQuery">The type of the query</typeparam>
    /// <typeparam name="TResponse">The type of the response</typeparam>
    public interface IQueryHandler<TQuery, TResponse> : IRequestHandler<TQuery, Result<TResponse>>
        where TQuery : IQuery<TResponse>
    {
    }
}
