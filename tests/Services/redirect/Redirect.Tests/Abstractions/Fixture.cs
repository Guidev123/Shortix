using Microsoft.AspNetCore.Mvc.Testing;

namespace Redirect.Tests.Abstractions
{
    public class Fixture : WebApplicationFactory<Program>, IAsyncLifetime
    {
        public Fixture()
        {
        }

        public async Task InitializeAsync()
        {
        }

        public new async Task DisposeAsync()
        {
        }
    }
}