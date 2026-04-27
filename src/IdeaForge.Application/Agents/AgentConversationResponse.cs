using IdeaForge.Agents.Abstractions;

namespace IdeaForge.Application.Agents;

/// <summary>
/// Result returned after sending a prompt to a specific AI employee agent.
/// </summary>
public sealed class AgentConversationResponse
{
    public required Guid EmployeeId { get; init; }

    public required string EmployeeKey { get; init; }

    public required Guid PersonId { get; init; }

    public required string PersonDisplayName { get; init; }

    /// <summary>
    /// Backward-compatible alias for <see cref="PersonDisplayName"/>.
    /// </summary>
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
