using MidR.Interfaces;
using Shortix.Commons.Core.Results;

namespace Shortix.Commons.Core.Messaging
{
    public interface ICommandHandler<TCommand> : IRequestHandler<TCommand, Result>
        where TCommand : ICommand;

    public interface ICommandHandler<TCommand, TResponse> : IRequestHandler<TCommand, Result<TResponse>>
        where TCommand : ICommand<TResponse>;
}