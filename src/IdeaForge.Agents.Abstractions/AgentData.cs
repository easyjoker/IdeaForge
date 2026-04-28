namespace IdeaForge.Agents.Abstractions;

public sealed class AgentData
{
    public Guid PersonId { get; set; }

    public AgentProviderKind Provider { get; set; }

    public string Model { get; set; } = "gpt-5.4";

    public string? SessionId { get; set; }

    public string? SystemPrompt { get; set; }

    public string? CodexReasoning { get; set; }

    public string? CodexEffort { get; set; }

    public string? CodexCompute { get; set; }

    public DateTimeOffset? LastUsedAtUtc { get; set; }

    public DateTimeOffset? SessionUpdatedAtUtc { get; set; }
}
