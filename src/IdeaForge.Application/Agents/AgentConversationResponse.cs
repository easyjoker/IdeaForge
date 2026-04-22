using IdeaForge.Agents.Abstractions;

namespace IdeaForge.Application.Agents;

/// <summary>
/// Result returned after sending a prompt to a specific employee-style agent.
/// </summary>
public sealed class AgentConversationResponse
{
    public required Guid EmployeeId { get; init; }

    public required string EmployeeKey { get; init; }

    public required string EmployeeName { get; init; }

    public AgentProviderKind Provider { get; init; }

    public required string Model { get; init; }

    public string? SessionId { get; init; }

    public required int ExitCode { get; init; }

    public required string Output { get; init; }

    public string? ErrorOutput { get; init; }

    public required DateTimeOffset StartedAtUtc { get; init; }

    public required DateTimeOffset CompletedAtUtc { get; init; }
}
