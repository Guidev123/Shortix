using Shortix.Commons.Core.Messaging;

namespace Shortix.UrlShortener.Core.UseCases.Urls.Add
{
    public sealed record AddUrlCommand(string LongUrl) : ICommand<AddUrlResponse>
    {
        public string CreatedBy { get; private set; } = null!;

        public AddUrlCommand SetCreatedBy(string createdBy)
        {
            return this with { CreatedBy = createdBy };
        }
    }
}