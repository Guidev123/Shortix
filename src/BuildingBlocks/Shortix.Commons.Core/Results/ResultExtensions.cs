namespace Shortix.Commons.Core.Results
{
    public static class ResultExtensions
    {
        extension(Result result)
        {
            public TOut Match<TOut>(
            Func<TOut> onSuccess,
            Func<Result, TOut> onFailure) => result.IsSuccess ? onSuccess() : onFailure(result);
        }

        extension<TIn>(Result<TIn> result)
        {
            public TOut Match<TOut>(
            Func<TIn, TOut> onSuccess,
            Func<Result<TIn>, TOut> onFailure) => result.IsSuccess ? onSuccess(result.Value) : onFailure(result);
        }
    }
}