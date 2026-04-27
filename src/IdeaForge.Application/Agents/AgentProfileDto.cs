using IdeaForge.Agents.Abstractions;

namespace IdeaForge.Application.Agents;

/// <summary>
/// Full agent profile composed of person identity, employee data, and runtime agent data.
/// </summary>
public sealed class AgentProfileDto
{
    /// <summary>
    /// Person identity information.
    /// </summary>
    public required PersonDto Person { get; init; }

    /// <summary>
    /// Employee information.
    /// </summary>
    public required EmployeeDto Employee { get; init; }

    /// <summary>
    /// Runtime agent information such as provider, model, and session.
    /// </summary>
    public required AgentDataDto AgentData { get; init; }
}

/// <summary>
/// Person identity stored for an employee or AI agent.
/// </summary>
public sealed class PersonDto
{
    /// <summary>
    /// Unique person identifier.
    /// </summary>
    public required Guid Id { get; init; }

    /// <summary>
    /// Type of person. Values: 0 = Unknown, 1 = Human, 2 = AI.
    /// </summary>
    public PersonKind Kind { get; init; }

    /// <summary>
    /// Display name used in user interfaces.
    /// </summary>
    public required string DisplayName { get; init; }

    /// <summary>
    /// Optional person profile description.
    /// </summary>
    public string? Description { get; init; }

    /// <summary>
    /// Free-form person metadata such as avatar, locale, or contact tags.
    /// </summary>
    public IReadOnlyDictionary<string, string> Metadata { get; init; } = new Dictionary<string, string>();

    /// <summary>
    /// UTC timestamp when this person was created.
    /// </summary>
    public DateTimeOffset CreatedAtUtc { get; init; }

    /// <summary>
    /// UTC timestamp when this person was last updated.
    /// </summary>
    public DateTimeOffset UpdatedAtUtc { get; init; }
}

/// <summary>
/// Employee work assignment information stored for an agent.
/// </summary>
public sealed class EmployeeDto
{
    /// <summary>
    /// Unique employee identifier.
    /// </summary>
    public required Guid Id { get; init; }

    /// <summary>
    /// Person identity linked to this employee record.
    /// </summary>
    public required Guid PersonId { get; init; }

    /// <summary>
    /// Stable employee key used by scripts and APIs.
    /// </summary>
    public required string Key { get; init; }

    /// <summary>
    /// Functional role of the employee.
    /// </summary>
    public string? Role { get; init; }

    /// <summary>
    /// Long-term mission or responsibility of the employee.
    /// </summary>
    public string? Mission { get; init; }

    /// <summary>
    /// Current employee lifecycle status.
    /// </summary>
    public AgentStatus Status { get; init; }

    /// <summary>
    /// Main areas of expertise for the employee.
    /// </summary>
    public IReadOnlyList<string> Specialties { get; init; } = Array.Empty<string>();

    /// <summary>
    /// Free-form metadata such as team, owner, or environment tags.
    /// </summary>
    public IReadOnlyDictionary<string, string> Metadata { get; init; } = new Dictionary<string, string>();

    /// <summary>
    /// UTC timestamp when this employee was created.
    /// </summary>
    public DateTimeOffset CreatedAtUtc { get; init; }

    /// <summary>
    /// UTC timestamp when this employee was last updated.
    /// </summary>
    public DateTimeOffset UpdatedAtUtc { get; init; }
}

/// <summary>
/// Runtime agent information stored separately from employee identity.
/// </summary>
public sealed class AgentDataDto
{
    /// <summary>
    /// LLM provider currently assigned to the agent.
    /// </summary>
    public AgentProviderKind Provider { get; init; }

    /// <summary>
    /// Model currently assigned to the agent.
    /// </summary>
    public required string Model { get; init; }

    /// <summary>
    /// Provider session or thread identifier, when available.
    /// </summary>
    public string? SessionId { get; init; }

    /// <summary>
    /// Optional system prompt used to steer runtime behavior.
    /// </summary>
    public string? SystemPrompt { get; init; }

    /// <summary>
    /// UTC timestamp of the last successful runtime usage.
    /// </summary>
    public DateTimeOffset? LastUsedAtUtc { get; init; }

    /// <summary>
    /// UTC timestamp when the provider session identifier was last updated.
    /// </summary>
    public DateTimeOffset? SessionUpdatedAtUtc { get; init; }
}
