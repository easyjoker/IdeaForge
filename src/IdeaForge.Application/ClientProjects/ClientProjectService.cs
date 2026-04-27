namespace IdeaForge.Application.ClientProjects;

public sealed class ClientProjectService : IClientProjectService
{
    private readonly IClientProjectRepository _repository;

    public ClientProjectService(IClientProjectRepository repository)
    {
        _repository = repository;
    }

    public async Task<IReadOnlyList<ClientCompanyDto>> ListCompaniesAsync(CancellationToken cancellationToken = default)
    {
        var companies = await _repository.ListCompaniesAsync(cancellationToken);
        return companies.Select(Map).ToArray();
    }

    public async Task<ClientCompanyDto?> GetCompanyAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var company = await _repository.GetCompanyByIdAsync(id, cancellationToken);
        return company is null ? null : Map(company);
    }

    public async Task<ClientCompanyDto?> GetCompanyByKeyAsync(string key, CancellationToken cancellationToken = default)
    {
        var company = await _repository.GetCompanyByKeyAsync(RequireText(key, nameof(key)), cancellationToken);
        return company is null ? null : Map(company);
    }

    public async Task<ClientCompanyDto> CreateCompanyAsync(CreateClientCompanyRequest request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        var key = RequireText(request.Key, nameof(request.Key));
        var existing = await _repository.GetCompanyByKeyAsync(key, cancellationToken);
        if (existing is not null)
        {
            throw new InvalidOperationException($"Client company key '{key}' already exists.");
        }

        var now = DateTimeOffset.UtcNow;
        var company = new ClientCompany
        {
            Key = key,
            Name = RequireText(request.Name, nameof(request.Name)),
            Description = NormalizeOptional(request.Description),
            Status = ClientResourceStatus.Active,
            CreatedAtUtc = now,
            UpdatedAtUtc = now
        };

        return Map(await _repository.AddCompanyAsync(company, cancellationToken));
    }

    public async Task<ClientCompanyDto?> UpdateCompanyAsync(Guid id, UpdateClientCompanyRequest request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        var existing = await _repository.GetCompanyByIdAsync(id, cancellationToken);
        if (existing is null)
        {
            return null;
        }

        existing.Name = RequireText(request.Name, nameof(request.Name));
        existing.Description = NormalizeOptional(request.Description);
        existing.Status = request.Status;
        existing.UpdatedAtUtc = DateTimeOffset.UtcNow;

        var updated = await _repository.UpdateCompanyAsync(existing, cancellationToken);
        return updated is null ? null : Map(updated);
    }

    public async Task<IReadOnlyList<ClientProjectDto>> ListProjectsAsync(Guid companyId, CancellationToken cancellationToken = default)
    {
        var company = await _repository.GetCompanyByIdAsync(companyId, cancellationToken);
        if (company is null)
        {
            throw new KeyNotFoundException($"Client company '{companyId}' was not found.");
        }

        var projects = await _repository.ListProjectsAsync(companyId, cancellationToken);
        return projects.Select(Map).ToArray();
    }

    public async Task<ClientProjectDto?> GetProjectAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var project = await _repository.GetProjectByIdAsync(id, cancellationToken);
        return project is null ? null : Map(project);
    }

    public async Task<ClientProjectDto> CreateProjectAsync(Guid companyId, CreateClientProjectRequest request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        var company = await _repository.GetCompanyByIdAsync(companyId, cancellationToken);
        if (company is null)
        {
            throw new KeyNotFoundException($"Client company '{companyId}' was not found.");
        }

        var key = RequireText(request.Key, nameof(request.Key));
        var existing = await _repository.GetProjectByCompanyAndKeyAsync(companyId, key, cancellationToken);
        if (existing is not null)
        {
            throw new InvalidOperationException($"Client project key '{key}' already exists for company '{companyId}'.");
        }

        var now = DateTimeOffset.UtcNow;
        var project = new ClientProject
        {
            ClientCompanyId = companyId,
            Key = key,
            Name = RequireText(request.Name, nameof(request.Name)),
            Description = NormalizeOptional(request.Description),
            Status = ClientResourceStatus.Active,
            CreatedAtUtc = now,
            UpdatedAtUtc = now
        };

        return Map(await _repository.AddProjectAsync(project, cancellationToken));
    }

    public async Task<ClientProjectDto?> UpdateProjectAsync(Guid id, UpdateClientProjectRequest request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        var existing = await _repository.GetProjectByIdAsync(id, cancellationToken);
        if (existing is null)
        {
            return null;
        }

        existing.Name = RequireText(request.Name, nameof(request.Name));
        existing.Description = NormalizeOptional(request.Description);
        existing.Status = request.Status;
        existing.UpdatedAtUtc = DateTimeOffset.UtcNow;

        var updated = await _repository.UpdateProjectAsync(existing, cancellationToken);
        return updated is null ? null : Map(updated);
    }

    public async Task<IReadOnlyList<ProjectRepositoryDto>> ListRepositoriesAsync(Guid projectId, CancellationToken cancellationToken = default)
    {
        var project = await _repository.GetProjectByIdAsync(projectId, cancellationToken);
        if (project is null)
        {
            throw new KeyNotFoundException($"Client project '{projectId}' was not found.");
        }

        var repositories = await _repository.ListRepositoriesAsync(projectId, cancellationToken);
        return repositories.Select(Map).ToArray();
    }

    public async Task<ProjectRepositoryDto?> GetRepositoryAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var repository = await _repository.GetRepositoryByIdAsync(id, cancellationToken);
        return repository is null ? null : Map(repository);
    }

    public async Task<ProjectRepositoryDto> CreateRepositoryAsync(Guid projectId, CreateProjectRepositoryRequest request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        var project = await _repository.GetProjectByIdAsync(projectId, cancellationToken);
        if (project is null)
        {
            throw new KeyNotFoundException($"Client project '{projectId}' was not found.");
        }

        var key = RequireText(request.Key, nameof(request.Key));
        var localPath = RequireText(request.LocalPath, nameof(request.LocalPath));
        await EnsureRepositoryKeyIsAvailableAsync(projectId, key, null, cancellationToken);
        await EnsureLocalPathIsAvailableAsync(localPath, null, cancellationToken);

        var now = DateTimeOffset.UtcNow;
        var repository = new ProjectRepository
        {
            ClientProjectId = projectId,
            Key = key,
            Name = RequireText(request.Name, nameof(request.Name)),
            RemoteRepositoryUrl = RequireText(request.RemoteRepositoryUrl, nameof(request.RemoteRepositoryUrl)),
            LocalPath = localPath,
            GitStrategySkillPath = RequireText(request.GitStrategySkillPath, nameof(request.GitStrategySkillPath)),
            Description = NormalizeOptional(request.Description),
            Status = ClientResourceStatus.Active,
            CreatedAtUtc = now,
            UpdatedAtUtc = now
        };

        return Map(await _repository.AddRepositoryAsync(repository, cancellationToken));
    }

    public async Task<ProjectRepositoryDto?> UpdateRepositoryAsync(Guid id, UpdateProjectRepositoryRequest request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        var existing = await _repository.GetRepositoryByIdAsync(id, cancellationToken);
        if (existing is null)
        {
            return null;
        }

        var localPath = RequireText(request.LocalPath, nameof(request.LocalPath));
        await EnsureLocalPathIsAvailableAsync(localPath, id, cancellationToken);

        existing.Name = RequireText(request.Name, nameof(request.Name));
        existing.RemoteRepositoryUrl = RequireText(request.RemoteRepositoryUrl, nameof(request.RemoteRepositoryUrl));
        existing.LocalPath = localPath;
        existing.GitStrategySkillPath = RequireText(request.GitStrategySkillPath, nameof(request.GitStrategySkillPath));
        existing.Description = NormalizeOptional(request.Description);
        existing.Status = request.Status;
        existing.UpdatedAtUtc = DateTimeOffset.UtcNow;

        var updated = await _repository.UpdateRepositoryAsync(existing, cancellationToken);
        return updated is null ? null : Map(updated);
    }

    private async Task EnsureRepositoryKeyIsAvailableAsync(Guid projectId, string key, Guid? currentRepositoryId, CancellationToken cancellationToken)
    {
        var existing = await _repository.GetRepositoryByProjectAndKeyAsync(projectId, key, cancellationToken);
        if (existing is not null && existing.Id != currentRepositoryId)
        {
            throw new InvalidOperationException($"Project repository key '{key}' already exists for project '{projectId}'.");
        }
    }

    private async Task EnsureLocalPathIsAvailableAsync(string localPath, Guid? currentRepositoryId, CancellationToken cancellationToken)
    {
        var existing = await _repository.GetRepositoryByLocalPathAsync(localPath, cancellationToken);
        if (existing is not null && existing.Id != currentRepositoryId)
        {
            throw new InvalidOperationException($"Project repository local path '{localPath}' already exists.");
        }
    }

    private static string RequireText(string value, string parameterName)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException($"{parameterName} is required.", parameterName);
        }

        return value.Trim();
    }

    private static string? NormalizeOptional(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    private static ClientCompanyDto Map(ClientCompany company) =>
        new()
        {
            Id = company.Id,
            Key = company.Key,
            Name = company.Name,
            Description = company.Description,
            Status = company.Status,
            CreatedAtUtc = company.CreatedAtUtc,
            UpdatedAtUtc = company.UpdatedAtUtc
        };

    private static ClientProjectDto Map(ClientProject project) =>
        new()
        {
            Id = project.Id,
            ClientCompanyId = project.ClientCompanyId,
            Key = project.Key,
            Name = project.Name,
            Description = project.Description,
            Status = project.Status,
            CreatedAtUtc = project.CreatedAtUtc,
            UpdatedAtUtc = project.UpdatedAtUtc
        };

    private static ProjectRepositoryDto Map(ProjectRepository repository) =>
        new()
        {
            Id = repository.Id,
            ClientProjectId = repository.ClientProjectId,
            Key = repository.Key,
            Name = repository.Name,
            RemoteRepositoryUrl = repository.RemoteRepositoryUrl,
            LocalPath = repository.LocalPath,
            GitStrategySkillPath = repository.GitStrategySkillPath,
            Description = repository.Description,
            Status = repository.Status,
            CreatedAtUtc = repository.CreatedAtUtc,
            UpdatedAtUtc = repository.UpdatedAtUtc
        };
}
