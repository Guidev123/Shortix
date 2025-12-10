using MidR.Interfaces;
using Shortix.Commons.Core.Results;

namespace Shortix.Commons.Core.Messaging
{
    public interface ICommand : IRequest<Result>, IBaseCommand;

    public interface ICommand<TResponse> : IRequest<Result<TResponse>>, IBaseCommand;

    public interface IBaseCommand;
}