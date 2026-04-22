using IdeaForge.Agents.Abstractions;

namespace IdeaForge.Application.Agents;

/// <summary>
/// Request used to update an existing employee-style agent.
/// </summary>
public sealed class UpdateAgentProfileRequest
{
    /// <summary>
    /// Display name of the employee-style agent.
    /// </summary>
    public required string Name { get; init; }

    /// <summary>
    /// Functional role of the employee, for example <c>Code Reviewer</c>.
    /// </summary>
    public string? Role { get; init; }

    /// <summary>
    /// Short description of what this employee does.
    /// </summary>
    public string? Description { get; init; }

    /// <summary>
    /// Long-term mission or responsibility of the employee.
    /// </summary>
    public string? Mission { get; init; }

    /// <summary>
    /// Current employee lifecycle status.
    /// </summary>
    public AgentStatus Status { get; init; } = AgentStatus.Active;

    /// <summary>
    /// Main areas of expertise for the employee.
    /// </summary>
    public IReadOnlyList<string> Specialties { get; init; } = Array.Empty<string>();

    /// <summary>
    /// Free-form metadata such as team, owner, or environment tags.
    /// </summary>
    public IReadOnlyDictionary<string, string> Metadata { get; init; } = new Dictionary<string, string>();

    /// <summary>
    /// LLM provider used by this agent, for example Copilot or Codex.
    /// </summary>
    public AgentProviderKind Provider { get; init; }

    /// <summary>
    /// Preferred model. When omitted, the existing model is kept.
    /// </summary>
    public string? Model { get; init; }

    /// <summary>
    /// Optional system prompt to steer the agent when runtime execution begins.
    /// </summary>
    public string? SystemPrompt { get; init; }
}
