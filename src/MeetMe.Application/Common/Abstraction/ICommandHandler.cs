using MediatR;
using MeetMe.Application.Common.Models;

namespace MeetMe.Application.Common.Abstraction
{
    /// <summary>
    /// Interface for command handlers that don't return a value
    /// </summary>
    /// <typeparam name="TCommand">The type of the command</typeparam>
    public interface ICommandHandler<TCommand> : IRequestHandler<TCommand, Result>
        where TCommand : ICommand
    {
    }

    /// <summary>
    /// Interface for command handlers that return a value
    /// </summary>
    /// <typeparam name="TCommand">The type of the command</typeparam>
    /// <typeparam name="TResponse">The type of the response</typeparam>
    public interface ICommandHandler<TCommand, TResponse> : IRequestHandler<TCommand, Result<TResponse>>
        where TCommand : ICommand<TResponse>
    {
    }
}
