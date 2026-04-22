namespace IdeaForge.Infrastructure.Persistence;

public sealed class PostgreSqlOptions
{
    public const string SectionName = "ConnectionStrings";

    public string ConnectionString { get; set; } = string.Empty;

    public string SchemaScriptPath { get; set; } = Path.Combine("..", "..", "db", "postgresql", "001_agents.sql");
}
