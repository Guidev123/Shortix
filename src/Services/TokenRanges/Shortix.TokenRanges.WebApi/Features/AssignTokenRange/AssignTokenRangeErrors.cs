using Shortix.Commons.Core.Results;

namespace Shortix.TokenRange.WebApi.Features.AssignTokenRange
{
    public static class AssignTokenRangeErrors
    {
        public static readonly Error FailedToAssignRange = Error.Problem(
            "AssignTokenRange.FailedToAssignRange",
            "Failed to assign token range."
        );
    }
}
