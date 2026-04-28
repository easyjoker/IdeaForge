namespace IdeaForge.Agents.Abstractions;

public sealed class AgentData
{
    public Guid PersonId { get; set; }

    public AgentProviderKind Provider { get; set; }

    public string Model { get; set; } = "gpt-5.4";

    public string? SessionId { get; set; }

    public string? SystemPrompt { get; set; }

    public Dictionary<string, string> ProviderSettings { get; set; } = new(StringComparer.OrdinalIgnoreCase);

    public DateTimeOffset? LastUsedAtUtc { get; set; }

    public DateTimeOffset? SessionUpdatedAtUtc { get; set; }
}
