using IdeaForge.Agents.Abstractions;

namespace IdeaForge.Application.Agents;

/// <summary>
/// Request used to report a completed agent execution back into the system.
/// </summary>
public sealed class ReportAgentExecutionRequest
{
    /// <summary>
    /// Provider that executed the task.
    /// </summary>
    public AgentProviderKind Provider { get; init; }

    /// <summary>
    /// Model used for this execution. When omitted, the current agent model is reused.
    /// </summary>
    public string? Model { get; init; }

    /// <summary>
    /// Session or thread identifier returned by the provider.
    /// </summary>
    public string? SessionId { get; init; }

    /// <summary>
    /// Original user prompt sent to the agent.
    /// </summary>
    public required string Prompt { get; init; }

    /// <summary>
    /// Short summary of the output or final answer.
    /// </summary>
    public string? OutputSummary { get; init; }

    /// <summary>
    /// Indicates whether the execution completed successfully.
    /// </summary>
    public bool Success { get; init; }

    /// <summary>
    /// Error details when the execution failed.
    /// </summary>
    public string? ErrorMessage { get; init; }

    /// <summary>
    /// UTC timestamp when execution started.
    /// </summary>
    public DateTimeOffset StartedAtUtc { get; init; }

    /// <summary>
    /// UTC timestamp when execution completed.
    /// </summary>
    public DateTimeOffset CompletedAtUtc { get; init; }
}
