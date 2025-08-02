using MediatR;
using MeetMe.Application.Common.Models;

namespace MeetMe.Application.Common.Abstraction
{
    /// <summary>
    /// Marker interface for queries
    /// </summary>
    /// <typeparam name="TResponse">The response type</typeparam>
    public interface IQuery<TResponse> : IRequest<Result<TResponse>>
    {
    }
}
