using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace IdeaForge.Infrastructure.Persistence;

public sealed class PostgreSqlInitializer : IPostgreSqlInitializer
{
    private readonly PostgreSqlOptions _options;
    private readonly IDbContextFactory<IdeaForgeDbContext> _dbContextFactory;

    public PostgreSqlInitializer(
        PostgreSqlOptions options,
        IDbContextFactory<IdeaForgeDbContext> dbContextFactory)
    {
        _options = options;
        _dbContextFactory = dbContextFactory;
    }

    public async Task InitializeAsync(CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(_options.ConnectionString))
        {
            throw new InvalidOperationException("PostgreSQL connection string is not configured.");
        }

        await EnsureDatabaseExistsAsync(cancellationToken);

        await using var dbContext = await _dbContextFactory.CreateDbContextAsync(cancellationToken);
        await dbContext.Database.MigrateAsync(cancellationToken);
    }

    private async Task EnsureDatabaseExistsAsync(CancellationToken cancellationToken)
    {
        var targetBuilder = new NpgsqlConnectionStringBuilder(_options.ConnectionString);
        var databaseName = targetBuilder.Database;
        if (string.IsNullOrWhiteSpace(databaseName))
        {
            throw new InvalidOperationException("PostgreSQL database name is missing from the connection string.");
        }

        var adminBuilder = new NpgsqlConnectionStringBuilder(_options.ConnectionString)
        {
            Database = "postgres"
        };

        await using var adminConnection = new NpgsqlConnection(adminBuilder.ConnectionString);
        await adminConnection.OpenAsync(cancellationToken);

        const string existsSql = "select 1 from pg_database where datname = @databaseName;";
        await using var existsCommand = new NpgsqlCommand(existsSql, adminConnection);
        existsCommand.Parameters.AddWithValue("databaseName", databaseName);

        var exists = await existsCommand.ExecuteScalarAsync(cancellationToken);
        if (exists is not null)
        {
            return;
        }

        var escapedDatabaseName = databaseName.Replace("\"", "\"\"", StringComparison.Ordinal);
        var createSql = $"create database \"{escapedDatabaseName}\";";
        await using var createCommand = new NpgsqlCommand(createSql, adminConnection);
        await createCommand.ExecuteNonQueryAsync(cancellationToken);
    }
}
