namespace IdeaForge.Infrastructure.Persistence;

public sealed class PersonEntity
{
    public Guid Id { get; set; }

    public short Kind { get; set; } = 1;

    public string DisplayName { get; set; } = string.Empty;

    public string? Description { get; set; }

    public string MetadataJson { get; set; } = "{}";

    public DateTimeOffset CreatedAtUtc { get; set; }

    public DateTimeOffset UpdatedAtUtc { get; set; }
}

public sealed class EmployeeEntity
{
    public Guid Id { get; set; }

    public Guid PersonId { get; set; }

    public string Key { get; set; } = string.Empty;

    public string? Role { get; set; }

    public string? Mission { get; set; }

    public short Status { get; set; } = 1;

    public string SpecialtiesJson { get; set; } = "[]";

    public string MetadataJson { get; set; } = "{}";

    public DateTimeOffset CreatedAtUtc { get; set; }

    public DateTimeOffset UpdatedAtUtc { get; set; }
}

public sealed class AgentDataEntity
{
    public Guid EmployeeId { get; set; }

    public short Provider { get; set; }

    public string Model { get; set; } = "gpt-5.4";

    public string? SessionId { get; set; }

    public string? SystemPrompt { get; set; }

    public DateTimeOffset? LastUsedAtUtc { get; set; }

    public DateTimeOffset? SessionUpdatedAtUtc { get; set; }
}

public sealed class AgentExecutionEntity
{
    public Guid Id { get; set; }

    public Guid EmployeeId { get; set; }

    public short Provider { get; set; }

    public string Model { get; set; } = "gpt-5.4";

    public string? SessionId { get; set; }

    public string Prompt { get; set; } = string.Empty;

    public string? OutputSummary { get; set; }

    public bool Success { get; set; }

    public string? ErrorMessage { get; set; }

    public DateTimeOffset StartedAtUtc { get; set; }

    public DateTimeOffset CompletedAtUtc { get; set; }
}

public sealed class ClientCompanyEntity
{
    public Guid Id { get; set; }

    public string Key { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }

    public short Status { get; set; } = 1;

    public DateTimeOffset CreatedAtUtc { get; set; }

    public DateTimeOffset UpdatedAtUtc { get; set; }
}

public sealed class ClientProjectEntity
{
    public Guid Id { get; set; }

    public Guid ClientCompanyId { get; set; }

    public string Key { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }

    public short Status { get; set; } = 1;

    public DateTimeOffset CreatedAtUtc { get; set; }

    public DateTimeOffset UpdatedAtUtc { get; set; }
}

public sealed class ProjectRepositoryEntity
{
    public Guid Id { get; set; }

    public Guid ClientProjectId { get; set; }

    public string Key { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    public string RemoteRepositoryUrl { get; set; } = string.Empty;

    public string LocalPath { get; set; } = string.Empty;

    public string GitStrategySkillPath { get; set; } = string.Empty;

    public string? Description { get; set; }

    public short Status { get; set; } = 1;

    public DateTimeOffset CreatedAtUtc { get; set; }

    public DateTimeOffset UpdatedAtUtc { get; set; }
}
