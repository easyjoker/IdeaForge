using IdeaForge.Agents.Abstractions;

namespace IdeaForge.Application.Agents;

public sealed class AgentExecutionDto
{
    public required Guid Id { get; init; }

    public required Guid EmployeeId { get; init; }

    public AgentProviderKind Provider { get; init; }

    public required string Model { get; init; }

    public string? SessionId { get; init; }

    public required string Prompt { get; init; }

    public string? OutputSummary { get; init; }

    public bool Success { get; init; }

    public string? ErrorMessage { get; init; }

    public DateTimeOffset StartedAtUtc { get; init; }

    public DateTimeOffset CompletedAtUtc { get; init; }
}
