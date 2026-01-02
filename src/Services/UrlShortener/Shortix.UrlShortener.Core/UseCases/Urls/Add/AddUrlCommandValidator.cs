using FluentValidation;
using Shortix.UrlShortener.Core.Errors;

namespace Shortix.UrlShortener.Core.UseCases.Urls.Add
{
    internal sealed class AddUrlCommandValidator : AbstractValidator<AddUrlCommand>
    {
        private const int MaxUrlLength = 2048;

        public AddUrlCommandValidator()
        {
            RuleFor(c => c.CreatedBy)
                .NotEmpty()
                .WithMessage(ShortenedUrlErrors.CreatedByShouldBeNotEmpty.Description);

            RuleFor(c => c.LongUrl)
                .NotEmpty()
                .WithMessage(ShortenedUrlErrors.UrlShouldNotBeEmpty.Description)
                .Must(c => c.Length <= MaxUrlLength)
                .WithMessage(ShortenedUrlErrors.UrlMaxLengthExceeded(MaxUrlLength).Description)
                .Must(BeAValidUrl)
                .WithMessage(ShortenedUrlErrors.UrlShouldBeAbsolute.Description);
        }

        private static bool BeAValidUrl(string url)
        {
            if (string.IsNullOrWhiteSpace(url))
                return false;

            if (!Uri.TryCreate(url, UriKind.Absolute, out var uri))
                return false;

            return uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps;
        }
    }
}