using Microsoft.Extensions.Hosting;
using Npgsql;

namespace IdeaForge.Infrastructure.Persistence;

public sealed class PostgreSqlInitializer : IPostgreSqlInitializer
{
    private readonly PostgreSqlOptions _options;
    private readonly IHostEnvironment _hostEnvironment;

    public PostgreSqlInitializer(PostgreSqlOptions options, IHostEnvironment hostEnvironment)
    {
        _options = options;
        _hostEnvironment = hostEnvironment;
    }

    public async Task InitializeAsync(CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(_options.ConnectionString))
        {
            throw new InvalidOperationException("PostgreSQL connection string is not configured.");
        }

        await EnsureDatabaseExistsAsync(cancellationToken);

        var schemaPath = Path.GetFullPath(Path.Combine(_hostEnvironment.ContentRootPath, _options.SchemaScriptPath));
        if (!File.Exists(schemaPath))
        {
            throw new FileNotFoundException($"Schema script was not found: {schemaPath}", schemaPath);
        }

        var sql = await File.ReadAllTextAsync(schemaPath, cancellationToken);

        await using var connection = new NpgsqlConnection(_options.ConnectionString);
        await connection.OpenAsync(cancellationToken);

        await using var command = new NpgsqlCommand(sql, connection);
        await command.ExecuteNonQueryAsync(cancellationToken);
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
