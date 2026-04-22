namespace IdeaForge.Agents.Abstractions;

public sealed record AgentExecutionResult
{
    public required AgentProviderKind Provider { get; init; }

    public string? Model { get; init; }

    public string? SessionId { get; init; }

    public required int ExitCode { get; init; }

    public required string Output { get; init; }

    public string? ErrorOutput { get; init; }

    public string? OutputPath { get; init; }
}
