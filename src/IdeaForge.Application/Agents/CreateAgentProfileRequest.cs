using IdeaForge.Agents.Abstractions;

namespace IdeaForge.Application.Agents;

/// <summary>
/// Request used to create an employee-backed agent profile.
/// </summary>
public sealed class CreateAgentProfileRequest
{
    /// <summary>
    /// Stable employee key used by scripts and APIs, for example <c>reviewer</c>.
    /// </summary>
    public required string Key { get; init; }

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
    /// Preferred model. Defaults to <c>gpt-5.4</c> when omitted.
    /// </summary>
    public string? Model { get; init; }

    /// <summary>
    /// Optional system prompt to steer the agent when runtime execution begins.
    /// </summary>
    public string? SystemPrompt { get; init; }
}
