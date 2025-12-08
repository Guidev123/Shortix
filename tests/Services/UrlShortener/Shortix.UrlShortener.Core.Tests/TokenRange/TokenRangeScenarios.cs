using FluentAssertions;
using TokenRangeDto = Shortix.UrlShortener.Core.TokenRange;

namespace Shortix.UrlShortener.Core.Tests.TokenRange
{
    public class TokenRangeScenarios
    {
        [Fact]
        public void When_Start_Token_Is_Greater_Than_End_Token_Then_Throws_Exception()
        {
            var act = () => new TokenRangeDto(200, 100);

            act.Should()
                .Throw<ArgumentException>()
                .WithMessage("Start token cannot be greater than end token. (Parameter 'Start')");
        }
    }
}