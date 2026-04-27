namespace IdeaForge.Application.ClientProjects;

/// <summary>
/// Lifecycle status for client companies, projects, and repositories.
/// </summary>
public enum ClientResourceStatus
{
    /// <summary>
    /// Draft configuration that is not ready for normal use.
    /// </summary>
    Draft = 0,

    /// <summary>
    /// Active resource that agents can use.
    /// </summary>
    Active = 1,

    /// <summary>
    /// Temporarily disabled resource that agents should not use.
    /// </summary>
    Disabled = 2,

    /// <summary>
    /// Archived resource kept for history only.
    /// </summary>
    Archived = 3
}

public sealed class ClientCompany
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public string Key { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }

    public ClientResourceStatus Status { get; set; } = ClientResourceStatus.Active;

    public DateTimeOffset CreatedAtUtc { get; set; } = DateTimeOffset.UtcNow;

    public DateTimeOffset UpdatedAtUtc { get; set; } = DateTimeOffset.UtcNow;
}

public sealed class ClientProject
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid ClientCompanyId { get; set; }

    public string Key { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }

    public ClientResourceStatus Status { get; set; } = ClientResourceStatus.Active;

    public DateTimeOffset CreatedAtUtc { get; set; } = DateTimeOffset.UtcNow;

    public DateTimeOffset UpdatedAtUtc { get; set; } = DateTimeOffset.UtcNow;
}

public sealed class ProjectRepository
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid ClientProjectId { get; set; }

    public string Key { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    public string RemoteRepositoryUrl { get; set; } = string.Empty;

    public string LocalPath { get; set; } = string.Empty;

    public string GitStrategySkillPath { get; set; } = string.Empty;

    public string? Description { get; set; }

    public ClientResourceStatus Status { get; set; } = ClientResourceStatus.Active;

    public DateTimeOffset CreatedAtUtc { get; set; } = DateTimeOffset.UtcNow;

    public DateTimeOffset UpdatedAtUtc { get; set; } = DateTimeOffset.UtcNow;
}

/// <summary>
/// Client company returned by the project settings API.
/// </summary>
public sealed class ClientCompanyDto
{
    /// <summary>
    /// Unique client company identifier.
    /// </summary>
    public required Guid Id { get; init; }

    /// <summary>
    /// Stable company key used by APIs and scripts.
    /// </summary>
    public required string Key { get; init; }

    /// <summary>
    /// Display name of the client company.
    /// </summary>
    public required string Name { get; init; }

    /// <summary>
    /// Optional short description of the company relationship or scope.
    /// </summary>
    public string? Description { get; init; }

    /// <summary>
    /// Current lifecycle status.
    /// </summary>
    public ClientResourceStatus Status { get; init; }

    /// <summary>
    /// UTC timestamp when this company was created.
    /// </summary>
    public DateTimeOffset CreatedAtUtc { get; init; }

    /// <summary>
    /// UTC timestamp when this company was last updated.
    /// </summary>
    public DateTimeOffset UpdatedAtUtc { get; init; }
}

/// <summary>
/// Client project returned by the project settings API.
/// </summary>
public sealed class ClientProjectDto
{
    /// <summary>
    /// Unique client project identifier.
    /// </summary>
    public required Guid Id { get; init; }

    /// <summary>
    /// Client company that owns this project.
    /// </summary>
    public required Guid ClientCompanyId { get; init; }

    /// <summary>
    /// Stable project key scoped to the owning company.
    /// </summary>
    public required string Key { get; init; }

    /// <summary>
    /// Display name of the client project.
    /// </summary>
    public required string Name { get; init; }

    /// <summary>
    /// Optional short description of the project goal or scope.
    /// </summary>
    public string? Description { get; init; }

    /// <summary>
    /// Current lifecycle status.
    /// </summary>
    public ClientResourceStatus Status { get; init; }

    /// <summary>
    /// UTC timestamp when this project was created.
    /// </summary>
    public DateTimeOffset CreatedAtUtc { get; init; }

    /// <summary>
    /// UTC timestamp when this project was last updated.
    /// </summary>
    public DateTimeOffset UpdatedAtUtc { get; init; }
}

/// <summary>
/// Repository or service configuration returned by the project settings API.
/// </summary>
public sealed class ProjectRepositoryDto
{
    /// <summary>
    /// Unique repository configuration identifier.
    /// </summary>
    public required Guid Id { get; init; }

    /// <summary>
    /// Client project that owns this repository.
    /// </summary>
    public required Guid ClientProjectId { get; init; }

    /// <summary>
    /// Stable repository key scoped to the owning project.
    /// </summary>
    public required string Key { get; init; }

    /// <summary>
    /// Display name of the repository or service.
    /// </summary>
    public required string Name { get; init; }

    /// <summary>
    /// Client remote repository URL.
    /// </summary>
    public required string RemoteRepositoryUrl { get; init; }

    /// <summary>
    /// Local workspace path used by ideaForge agents.
    /// </summary>
    public required string LocalPath { get; init; }

    /// <summary>
    /// Path to the git strategy skill that agents should read before working in this repository.
    /// </summary>
    public required string GitStrategySkillPath { get; init; }

    /// <summary>
    /// Optional short description of the service or repository.
    /// </summary>
    public string? Description { get; init; }

    /// <summary>
    /// Current lifecycle status.
    /// </summary>
    public ClientResourceStatus Status { get; init; }

    /// <summary>
    /// UTC timestamp when this repository configuration was created.
    /// </summary>
    public DateTimeOffset CreatedAtUtc { get; init; }

    /// <summary>
    /// UTC timestamp when this repository configuration was last updated.
    /// </summary>
    public DateTimeOffset UpdatedAtUtc { get; init; }
}

/// <summary>
/// Request used to create a client company.
/// </summary>
public sealed class CreateClientCompanyRequest
{
    /// <summary>
    /// Stable company key used by APIs and scripts, for example <c>contoso</c>.
    /// </summary>
    public required string Key { get; init; }

    /// <summary>
    /// Display name of the client company.
    /// </summary>
    public required string Name { get; init; }

    /// <summary>
    /// Optional short description of the company relationship or scope.
    /// </summary>
    public string? Description { get; init; }
}

/// <summary>
/// Request used to update a client company.
/// </summary>
public sealed class UpdateClientCompanyRequest
{
    /// <summary>
    /// Display name of the client company.
    /// </summary>
    public required string Name { get; init; }

    /// <summary>
    /// Optional short description of the company relationship or scope.
    /// </summary>
    public string? Description { get; init; }

    /// <summary>
    /// Current lifecycle status.
    /// </summary>
    public ClientResourceStatus Status { get; init; } = ClientResourceStatus.Active;
}

/// <summary>
/// Request used to create a client project under a company.
/// </summary>
public sealed class CreateClientProjectRequest
{
    /// <summary>
    /// Stable project key scoped to the owning company, for example <c>commerce-platform</c>.
    /// </summary>
    public required string Key { get; init; }

    /// <summary>
    /// Display name of the client project.
    /// </summary>
    public required string Name { get; init; }

    /// <summary>
    /// Optional short description of the project goal or scope.
    /// </summary>
    public string? Description { get; init; }
}

/// <summary>
/// Request used to update a client project.
/// </summary>
public sealed class UpdateClientProjectRequest
{
    /// <summary>
    /// Display name of the client project.
    /// </summary>
    public required string Name { get; init; }

    /// <summary>
    /// Optional short description of the project goal or scope.
    /// </summary>
    public string? Description { get; init; }

    /// <summary>
    /// Current lifecycle status.
    /// </summary>
    public ClientResourceStatus Status { get; init; } = ClientResourceStatus.Active;
}

/// <summary>
/// Request used to create a repository or service under a client project.
/// </summary>
public sealed class CreateProjectRepositoryRequest
{
    /// <summary>
    /// Stable repository key scoped to the owning project, for example <c>order-service</c>.
    /// </summary>
    public required string Key { get; init; }

    /// <summary>
    /// Display name of the repository or service.
    /// </summary>
    public required string Name { get; init; }

    /// <summary>
    /// Client remote repository URL.
    /// </summary>
    public required string RemoteRepositoryUrl { get; init; }

    /// <summary>
    /// Local workspace path used by ideaForge agents.
    /// </summary>
    public required string LocalPath { get; init; }

    /// <summary>
    /// Path to the git strategy skill that agents should read before working in this repository.
    /// </summary>
    public required string GitStrategySkillPath { get; init; }

    /// <summary>
    /// Optional short description of the service or repository.
    /// </summary>
    public string? Description { get; init; }
}

/// <summary>
/// Request used to update a repository or service.
/// </summary>
public sealed class UpdateProjectRepositoryRequest
{
    /// <summary>
    /// Display name of the repository or service.
    /// </summary>
    public required string Name { get; init; }

    /// <summary>
    /// Client remote repository URL.
    /// </summary>
    public required string RemoteRepositoryUrl { get; init; }

    /// <summary>
    /// Local workspace path used by ideaForge agents.
    /// </summary>
    public required string LocalPath { get; init; }

    /// <summary>
    /// Path to the git strategy skill that agents should read before working in this repository.
    /// </summary>
    public required string GitStrategySkillPath { get; init; }

    /// <summary>
    /// Optional short description of the service or repository.
    /// </summary>
    public string? Description { get; init; }

    /// <summary>
    /// Current lifecycle status.
    /// </summary>
    public ClientResourceStatus Status { get; init; } = ClientResourceStatus.Active;
}
