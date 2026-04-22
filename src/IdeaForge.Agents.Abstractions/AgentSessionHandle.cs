namespace IdeaForge.Agents.Abstractions;

public sealed record AgentSessionHandle
{
    public required AgentProviderKind Provider { get; init; }

    public required string SessionId { get; init; }

    public string? DisplayName { get; init; }
}
