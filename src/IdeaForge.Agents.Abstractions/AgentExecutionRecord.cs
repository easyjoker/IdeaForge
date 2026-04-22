namespace IdeaForge.Agents.Abstractions;

public sealed class AgentExecutionRecord
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid EmployeeId { get; set; }

    public AgentProviderKind Provider { get; set; }

    public string Model { get; set; } = "gpt-5.4";

    public string? SessionId { get; set; }

    public string Prompt { get; set; } = string.Empty;

    public string? OutputSummary { get; set; }

    public bool Success { get; set; }

    public string? ErrorMessage { get; set; }

    public DateTimeOffset StartedAtUtc { get; set; }

    public DateTimeOffset CompletedAtUtc { get; set; }
}
