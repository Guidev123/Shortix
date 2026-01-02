using MidR.Interfaces;
using Shortix.Commons.Core.Results;

namespace Shortix.Commons.Core.Messaging
{
    public interface IQuery<TResponse> : IRequest<Result<TResponse>>;
}