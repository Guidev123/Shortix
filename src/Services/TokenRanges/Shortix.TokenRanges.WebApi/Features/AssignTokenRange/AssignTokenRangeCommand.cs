using Shortix.Commons.Core.Messaging;

namespace Shortix.TokenRanges.WebApi.Features.AssignTokenRange
{
    internal sealed record AssignTokenRangeCommand(string Key) : ICommand<AssignTokenRangeResponse>;
}
