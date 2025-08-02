using MediatR;
using MeetMe.Application.Common.Models;

namespace MeetMe.Application.Common.Abstraction
{
    /// <summary>
    /// Marker interface for commands
    /// </summary>
    /// <typeparam name="TResponse">The response type</typeparam>
    public interface ICommand<TResponse> : IRequest<Result<TResponse>>
    {
    }

    /// <summary>
    /// Marker interface for commands without response
    /// </summary>
    public interface ICommand : IRequest<Result<MediatR.Unit>>
    {
    }
}
