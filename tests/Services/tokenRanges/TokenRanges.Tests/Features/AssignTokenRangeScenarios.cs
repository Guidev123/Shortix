using FluentAssertions;
using System.Net.Http.Json;
using TokenRanges.Tests.Abstractions;

namespace TokenRanges.Tests.Features
{
    public class AssignTokenRangeScenarios : IClassFixture<Fixture>
    {
        private const string AssignRangeEndpoint = "api/v1/token-ranges/assign";
        private readonly HttpClient _client;

        public AssignTokenRangeScenarios(Fixture fixture)
        {
            _client = fixture.CreateClient();
        }

        [Fact]
        public async Task ShouldReturnRange_WhenRequested()
        {
            var result = await _client.PostAsJsonAsync(AssignRangeEndpoint, new { Key = "Tests" });

            var response = await result.Content.ReadFromJsonAsync<TokenRangeResponse>();

            response.Should().NotBeNull();
            response.Start.Should().BeGreaterThan(0);
            response.End.Should().BeGreaterThan(response.Start);
        }


        [Fact]
        public async Task ShouldNotRepeatRange_WhenRequested()
        {
            var resultOne = await _client.PostAsJsonAsync(AssignRangeEndpoint, new { Key = "Tests" });
            var responseOne = await resultOne.Content.ReadFromJsonAsync<TokenRangeResponse>();

            var resultTwo = await _client.PostAsJsonAsync(AssignRangeEndpoint, new { Key = "Tests" });
            var responseTwo = await resultTwo.Content.ReadFromJsonAsync<TokenRangeResponse>();

            responseOne.Should().NotBeNull();
            responseTwo.Should().NotBeNull();
            responseTwo.Start.Should().BeGreaterThan(responseOne.End);
        }
    }
    internal sealed record TokenRangeResponse(long Start, long End);
}
