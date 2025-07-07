using MediatR;
using MeetMe.Application.Common.Models;

namespace MeetMe.Application.Common.Abstraction
{
    /// <summary>
    /// Marker interface for queries
    /// </summary>
    /// <typeparam name="TResponse">The type of the response</typeparam>
    public interface IQuery<TResponse> : IRequest<Result<TResponse>>
    {
    }
}
