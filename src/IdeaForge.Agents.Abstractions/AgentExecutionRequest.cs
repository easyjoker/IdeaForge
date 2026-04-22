namespace IdeaForge.Agents.Abstractions;

public sealed record AgentExecutionRequest
{
    public required string Prompt { get; init; }

    public string? Model { get; init; }

    public string? SessionId { get; init; }

    public string? WorkingDirectory { get; init; }

    public IReadOnlyList<string> AdditionalDirectories { get; init; } = Array.Empty<string>();

    public string? OutputPath { get; init; }

    public string? SchemaPath { get; init; }

    public IReadOnlyDictionary<string, string> Metadata { get; init; } = new Dictionary<string, string>();
}
