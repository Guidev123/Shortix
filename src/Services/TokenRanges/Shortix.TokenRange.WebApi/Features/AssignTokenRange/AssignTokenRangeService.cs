using Npgsql;
using Shortix.Commons.Core.Results;

namespace Shortix.TokenRange.WebApi.Features.AssignTokenRange
{
    internal sealed class AssignTokenRangeService(string connectionString)
    {
        private const int DefaultRangeSize = 1000;

        private static readonly string SqlQuery = $$"""
             INSERT INTO "TokenRanges" ("MachineIdentifier", "Start", "End")
             VALUES (
                 @MachineIdentifier,
                 COALESCE((SELECT MAX("End") FROM "TokenRanges") + 1, {{DefaultRangeSize}}),
                 COALESCE((SELECT MAX("End") FROM "TokenRanges") + {{DefaultRangeSize}}, 2000)
             )
             RETURNING "Id", "MachineIdentifier", "Start", "End";
        """;

        public async Task<Result<AssignTokenRangeResponse>> AssignRangeAsync(string machineIdentifier, CancellationToken cancellationToken = default)
        {
            using var connection = new NpgsqlConnection(connectionString);

            await connection.OpenAsync(cancellationToken);

            await using var command = new NpgsqlCommand(SqlQuery, connection);
            command.Parameters.AddWithValue("@MachineIdentifier", machineIdentifier);

            await using var reader = await command.ExecuteReaderAsync(cancellationToken);

            if (await reader.ReadAsync(cancellationToken))
            {
                return new AssignTokenRangeResponse(
                    reader.GetInt64(2),
                    reader.GetInt64(3)
                );
            }

            return Result.Failure<AssignTokenRangeResponse>(AssignTokenRangeErrors.FailedToAssignRange);
        }
    }
}
