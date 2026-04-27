using IdeaForge.Agents.Abstractions;

namespace IdeaForge.Application.Personnel;

/// <summary>
/// Person identity returned by the personnel API.
/// </summary>
public sealed class PersonDirectoryDto
{
    /// <summary>
    /// Unique person identifier.
    /// </summary>
    public required Guid Id { get; init; }

    /// <summary>
    /// Person kind. Values: 0 = Unknown, 1 = Human, 2 = AI.
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
    /// Free-form person metadata.
    /// </summary>
    public IReadOnlyDictionary<string, string> Metadata { get; init; } = new Dictionary<string, string>();

    /// <summary>
    /// AI agent settings. Present only when this person is AI and settings exist.
    /// </summary>
    public AgentSettingsDto? AgentSettings { get; init; }

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
/// Employee work assignment returned by the personnel API.
/// </summary>
public sealed class EmployeeDirectoryDto
{
    /// <summary>
    /// Person identity attached to this employee.
    /// </summary>
    public required PersonDirectoryDto Person { get; init; }

    /// <summary>
    /// Employee identifier.
    /// </summary>
    public required Guid Id { get; init; }

    /// <summary>
    /// Stable employee key used by scripts and APIs.
    /// </summary>
    public required string Key { get; init; }

    /// <summary>
    /// Functional role or job title.
    /// </summary>
    public string? Role { get; init; }

    /// <summary>
    /// Long-term mission or responsibility.
    /// </summary>
    public string? Mission { get; init; }

    /// <summary>
    /// Employee lifecycle status. Values: 0 = Draft, 1 = Active, 2 = Disabled, 3 = Archived.
    /// </summary>
    public AgentStatus Status { get; init; }

    /// <summary>
    /// Main areas of expertise.
    /// </summary>
    public IReadOnlyList<string> Specialties { get; init; } = Array.Empty<string>();

    /// <summary>
    /// Free-form employee metadata.
    /// </summary>
    public IReadOnlyDictionary<string, string> Metadata { get; init; } = new Dictionary<string, string>();

    /// <summary>
    /// UTC timestamp when this employee record was created.
    /// </summary>
    public DateTimeOffset CreatedAtUtc { get; init; }

    /// <summary>
    /// UTC timestamp when this employee record was last updated.
    /// </summary>
    public DateTimeOffset UpdatedAtUtc { get; init; }
}

/// <summary>
/// AI-only agent settings for a person.
/// </summary>
public sealed class AgentSettingsDto
{
    /// <summary>
    /// LLM provider. Values: 0 = Unknown, 1 = Copilot, 2 = Codex.
    /// </summary>
    public AgentProviderKind Provider { get; init; }

    /// <summary>
    /// Model currently assigned to the AI person.
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

/// <summary>
/// Request used to create a person.
/// </summary>
public sealed class CreatePersonRequest
{
    /// <summary>
    /// Person kind. Use 1 for Human and 2 for AI.
    /// </summary>
    public PersonKind Kind { get; init; } = PersonKind.Human;

    /// <summary>
    /// Display name used in user interfaces.
    /// </summary>
    public required string DisplayName { get; init; }

    /// <summary>
    /// Optional person profile description.
    /// </summary>
    public string? Description { get; init; }

    /// <summary>
    /// Free-form person metadata.
    /// </summary>
    public IReadOnlyDictionary<string, string> Metadata { get; init; } = new Dictionary<string, string>();

    /// <summary>
    /// AI-only settings. Used only when kind is AI; omitted Human people do not get agent settings.
    /// </summary>
    public UpsertAgentSettingsRequest? AgentSettings { get; init; }
}

/// <summary>
/// Request used to update a person.
/// </summary>
public sealed class UpdatePersonRequest
{
    /// <summary>
    /// Person kind. Use 1 for Human and 2 for AI.
    /// </summary>
    public PersonKind Kind { get; init; } = PersonKind.Human;

    /// <summary>
    /// Display name used in user interfaces.
    /// </summary>
    public required string DisplayName { get; init; }

    /// <summary>
    /// Optional person profile description.
    /// </summary>
    public string? Description { get; init; }

    /// <summary>
    /// Free-form person metadata.
    /// </summary>
    public IReadOnlyDictionary<string, string> Metadata { get; init; } = new Dictionary<string, string>();

    /// <summary>
    /// AI-only settings. Used only when kind is AI; changing a person to Human removes existing settings.
    /// </summary>
    public UpsertAgentSettingsRequest? AgentSettings { get; init; }
}

/// <summary>
/// Request used to create an employee record for a person.
/// </summary>
public sealed class CreateEmployeeRequest
{
    /// <summary>
    /// Person that will become an employee.
    /// </summary>
    public required Guid PersonId { get; init; }

    /// <summary>
    /// Stable employee key used by scripts and APIs.
    /// </summary>
    public required string Key { get; init; }

    /// <summary>
    /// Functional role or job title.
    /// </summary>
    public string? Role { get; init; }

    /// <summary>
    /// Long-term mission or responsibility.
    /// </summary>
    public string? Mission { get; init; }

    /// <summary>
    /// Main areas of expertise.
    /// </summary>
    public IReadOnlyList<string> Specialties { get; init; } = Array.Empty<string>();

    /// <summary>
    /// Free-form employee metadata.
    /// </summary>
    public IReadOnlyDictionary<string, string> Metadata { get; init; } = new Dictionary<string, string>();

}

/// <summary>
/// Request used to update an employee work assignment.
/// </summary>
public sealed class UpdateEmployeeRequest
{
    /// <summary>
    /// Stable employee key used by scripts and APIs.
    /// </summary>
    public required string Key { get; init; }

    /// <summary>
    /// Functional role or job title.
    /// </summary>
    public string? Role { get; init; }

    /// <summary>
    /// Long-term mission or responsibility.
    /// </summary>
    public string? Mission { get; init; }

    /// <summary>
    /// Employee lifecycle status. Values: 0 = Draft, 1 = Active, 2 = Disabled, 3 = Archived.
    /// </summary>
    public AgentStatus Status { get; init; } = AgentStatus.Active;

    /// <summary>
    /// Main areas of expertise.
    /// </summary>
    public IReadOnlyList<string> Specialties { get; init; } = Array.Empty<string>();

    /// <summary>
    /// Free-form employee metadata.
    /// </summary>
    public IReadOnlyDictionary<string, string> Metadata { get; init; } = new Dictionary<string, string>();

}

/// <summary>
/// Request used to create or update AI-only agent settings.
/// </summary>
public sealed class UpsertAgentSettingsRequest
{
    /// <summary>
    /// LLM provider. Values: 1 = Copilot, 2 = Codex.
    /// </summary>
    public AgentProviderKind Provider { get; init; }

    /// <summary>
    /// Model currently assigned to the AI person.
    /// </summary>
    public string? Model { get; init; }

    /// <summary>
    /// Optional system prompt used to steer runtime behavior.
    /// </summary>
    public string? SystemPrompt { get; init; }
}
