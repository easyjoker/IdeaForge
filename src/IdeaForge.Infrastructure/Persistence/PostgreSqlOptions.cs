namespace IdeaForge.Infrastructure.Persistence;

public sealed class PostgreSqlOptions
{
    public const string SectionName = "ConnectionStrings";

    public string ConnectionString { get; set; } = string.Empty;
}
