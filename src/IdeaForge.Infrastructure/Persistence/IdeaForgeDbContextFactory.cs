using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace IdeaForge.Infrastructure.Persistence;

public sealed class IdeaForgeDbContextFactory : IDesignTimeDbContextFactory<IdeaForgeDbContext>
{
    public IdeaForgeDbContext CreateDbContext(string[] args)
    {
        var builder = new DbContextOptionsBuilder<IdeaForgeDbContext>();
        builder.UseNpgsql(ResolveConnectionString());
        return new IdeaForgeDbContext(builder.Options);
    }

    private static string ResolveConnectionString()
    {
        foreach (var variableName in new[] { "IDEA_FORGE_DB_CONNECTION_STRING", "ConnectionStrings__IdeaForgeDb" })
        {
            var value = Environment.GetEnvironmentVariable(variableName);
            if (!string.IsNullOrWhiteSpace(value))
            {
                return value;
            }
        }

        foreach (var candidate in EnumerateDevelopmentSettingsCandidates())
        {
            if (!File.Exists(candidate))
            {
                continue;
            }

            using var document = JsonDocument.Parse(File.ReadAllText(candidate));
            if (document.RootElement.TryGetProperty("ConnectionStrings", out var connectionStrings) &&
                connectionStrings.TryGetProperty("IdeaForgeDb", out var ideaForgeDb) &&
                ideaForgeDb.ValueKind == JsonValueKind.String)
            {
                var value = ideaForgeDb.GetString();
                if (!string.IsNullOrWhiteSpace(value))
                {
                    return value;
                }
            }
        }

        throw new InvalidOperationException("IdeaForgeDb connection string was not found for EF Core design-time operations.");
    }

    private static IEnumerable<string> EnumerateDevelopmentSettingsCandidates()
    {
        var currentDirectory = new DirectoryInfo(Environment.CurrentDirectory);
        while (currentDirectory is not null)
        {
            yield return Path.Combine(currentDirectory.FullName, "src", "IdeaForge.WebApi", "appsettings.Development.json");
            yield return Path.Combine(currentDirectory.FullName, "appsettings.Development.json");
            currentDirectory = currentDirectory.Parent;
        }
    }
}
