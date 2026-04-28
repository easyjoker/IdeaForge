using IdeaForge.Agents.Abstractions;

namespace IdeaForge.Application.Agents;

/// <summary>
/// Request used to update an existing AI person, employee record, and agent settings.
/// </summary>
public sealed class UpdateAgentProfileRequest
{
    /// <summary>
    /// Display name of the AI person.
    /// </summary>
    public string? DisplayName { get; init; }

    /// <summary>
    /// Backward-compatible alias for <see cref="DisplayName"/>.
    /// </summary>
    public string? Name { get; init; }

    /// <summary>
    /// Optional person profile description.
    /// </summary>
    public string? PersonDescription { get; init; }

    /// <summary>
    /// Functional role of the employee, for example <c>Code Reviewer</c>.
    /// </summary>
    public string? Role { get; init; }

    /// <summary>
    /// Backward-compatible alias for <see cref="PersonDescription"/>.
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
    /// Free-form person metadata such as avatar, locale, or contact tags.
    /// </summary>
    public IReadOnlyDictionary<string, string> PersonMetadata { get; init; } = new Dictionary<string, string>();

    /// <summary>
    /// Free-form employee metadata such as team, owner, or environment tags.
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

    /// <summary>
    /// Codex CLI reasoning depth passed as --reasoning. Values: low, medium, high. Applies only when provider is Codex.
    /// </summary>
    public string? CodexReasoning { get; init; }

    /// <summary>
    /// Codex CLI effort level passed as --effort. Values: low, medium, high. Applies only when provider is Codex.
    /// </summary>
    public string? CodexEffort { get; init; }

    /// <summary>
    /// Codex CLI compute level passed as --compute. Values: low, medium, high. Applies only when provider is Codex.
    /// </summary>
    public string? CodexCompute { get; init; }
}
