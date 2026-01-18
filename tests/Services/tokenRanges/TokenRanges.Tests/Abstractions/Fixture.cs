using Microsoft.AspNetCore.Mvc.Testing;
using Npgsql;
using Testcontainers.PostgreSql;

namespace TokenRanges.Tests.Abstractions
{
    public class Fixture : WebApplicationFactory<Program>, IAsyncLifetime
    {
        private readonly PostgreSqlContainer _postgreSqlContainer;
        public string ConnectionString => _postgreSqlContainer.GetConnectionString();

        public Fixture()
        {
            _postgreSqlContainer = new PostgreSqlBuilder("postgres:15.1").Build();
        }

        public async Task InitializeAsync()
        {
            await _postgreSqlContainer.StartAsync();

            Environment.SetEnvironmentVariable("Postgres__ConnectionString", ConnectionString);

            var tableSql = await File.ReadAllTextAsync("Abstractions/Files/TokenRangesTable.sql");

            await using var connection = new NpgsqlConnection(ConnectionString);
            await connection.OpenAsync();

            await using var command = new NpgsqlCommand(tableSql, connection);
            await command.ExecuteNonQueryAsync();
        }

        public new async Task DisposeAsync()
        {
            await _postgreSqlContainer.DisposeAsync();
        }
    }
}
