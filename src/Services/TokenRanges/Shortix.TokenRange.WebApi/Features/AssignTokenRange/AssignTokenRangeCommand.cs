using Shortix.Commons.Core.Messaging;

namespace Shortix.TokenRange.WebApi.Features.AssignTokenRange
{
    internal sealed record AssignTokenRangeCommand(string Key) : ICommand<AssignTokenRangeResponse>;
}
