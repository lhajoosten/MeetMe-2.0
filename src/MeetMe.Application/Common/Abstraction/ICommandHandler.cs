using MediatR;
using MeetMe.Application.Common.Models;

namespace MeetMe.Application.Common.Abstraction
{
    /// <summary>
    /// Handler for commands with response
    /// </summary>
    /// <typeparam name="TCommand">The command type</typeparam>
    /// <typeparam name="TResponse">The response type</typeparam>
    public interface ICommandHandler<in TCommand, TResponse> : IRequestHandler<TCommand, Result<TResponse>>
        where TCommand : ICommand<TResponse>
    {
    }

    /// <summary>
    /// Handler for commands without response
    /// </summary>
    /// <typeparam name="TCommand">The command type</typeparam>
    public interface ICommandHandler<in TCommand> : IRequestHandler<TCommand, Result<MediatR.Unit>>
        where TCommand : ICommand
    {
    }
}
