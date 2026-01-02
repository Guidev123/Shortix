namespace Shortix.Commons.Core.Results
{
    public sealed class ValidationError : Error
    {
        public ValidationError(Error[] errors)
            : base(
                "General.Validation",
                "One or more validation errors occurred",
                ErrorTypeEnum.Validation)
        {
            Errors = errors;
        }

        public Error[] Errors { get; }

        public static ValidationError FromResults(IEnumerable<Result> results) =>
            new([.. results.Where(r => r.IsFailure).Select(r => r.Error)!]);
    }
}