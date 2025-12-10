using MidR.Interfaces;
using Shortix.Commons.Core.Results;

namespace Shortix.Commons.Core.Messaging
{
    public interface IQueryHandler<TQuery, TResponse> : IRequestHandler<TQuery, Result<TResponse>>
    where TQuery : IQuery<TResponse>;
}