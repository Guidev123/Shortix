using FluentAssertions;
using Shortix.UrlShortener.Core.Extensions;

namespace Shortix.UrlShortener.Core.Tests.Base62Encoding
{
    public class Base62EncodingScenarios
    {
        [Theory]
        [InlineData(1, "1")]
        [InlineData(20, "K")]
        [InlineData(1000, "G8")]
        [InlineData(61, "z")]
        [InlineData(987654321, "14q60P")]
        public void Should_Encode_Number_To_Base62(long number, string expected)
        {
            number
                .EncodeToBase62()
                .Should()
                .Be(expected);
        }
    }
}