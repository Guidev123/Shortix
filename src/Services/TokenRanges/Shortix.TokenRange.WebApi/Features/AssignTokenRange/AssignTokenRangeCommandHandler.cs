using Shortix.Commons.Core.Messaging;
using Shortix.Commons.Core.Results;

namespace Shortix.TokenRange.WebApi.Features.AssignTokenRange
{
    internal sealed class AssignTokenRangeCommandHandler(AssignTokenRangeService assignTokenRangeService) : ICommandHandler<AssignTokenRangeCommand, AssignTokenRangeResponse>
    {
        public async Task<Result<AssignTokenRangeResponse>> ExecuteAsync(AssignTokenRangeCommand request, CancellationToken cancellationToken = default)
        {
            return await assignTokenRangeService.AssignRangeAsync(request.Key, cancellationToken);

        }
    }
}
