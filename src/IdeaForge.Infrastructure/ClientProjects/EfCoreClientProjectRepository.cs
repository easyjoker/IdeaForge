using IdeaForge.Application.ClientProjects;
using IdeaForge.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace IdeaForge.Infrastructure.ClientProjects;

public sealed class EfCoreClientProjectRepository : IClientProjectRepository
{
    private readonly IDbContextFactory<IdeaForgeDbContext> _dbContextFactory;

    public EfCoreClientProjectRepository(IDbContextFactory<IdeaForgeDbContext> dbContextFactory)
    {
        _dbContextFactory = dbContextFactory;
    }

    public async Task<IReadOnlyList<ClientCompany>> ListCompaniesAsync(CancellationToken cancellationToken = default)
    {
        await using var dbContext = await _dbContextFactory.CreateDbContextAsync(cancellationToken);
        var items = await dbContext.ClientCompanies
            .AsNoTracking()
            .OrderBy(static company => company.Name)
            .ToListAsync(cancellationToken);

        return items.Select(Map).ToArray();
    }

    public async Task<ClientCompany?> GetCompanyByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        await using var dbContext = await _dbContextFactory.CreateDbContextAsync(cancellationToken);
        var item = await dbContext.ClientCompanies
            .AsNoTracking()
            .FirstOrDefaultAsync(company => company.Id == id, cancellationToken);

        return item is null ? null : Map(item);
    }

    public async Task<ClientCompany?> GetCompanyByKeyAsync(string key, CancellationToken cancellationToken = default)
    {
        await using var dbContext = await _dbContextFactory.CreateDbContextAsync(cancellationToken);
        var item = await dbContext.ClientCompanies
            .AsNoTracking()
            .FirstOrDefaultAsync(company => company.Key == key, cancellationToken);

        return item is null ? null : Map(item);
    }

    public async Task<ClientCompany> AddCompanyAsync(ClientCompany company, CancellationToken cancellationToken = default)
    {
        await using var dbContext = await _dbContextFactory.CreateDbContextAsync(cancellationToken);
        var entity = ToEntity(company);
        dbContext.ClientCompanies.Add(entity);
        await dbContext.SaveChangesAsync(cancellationToken);
        return Map(entity);
    }

    public async Task<ClientCompany?> UpdateCompanyAsync(ClientCompany company, CancellationToken cancellationToken = default)
    {
        await using var dbContext = await _dbContextFactory.CreateDbContextAsync(cancellationToken);
        var entity = await dbContext.ClientCompanies.FirstOrDefaultAsync(item => item.Id == company.Id, cancellationToken);
        if (entity is null)
        {
            return null;
        }

        entity.Name = company.Name;
        entity.Description = company.Description;
        entity.Status = (short)company.Status;
        entity.UpdatedAtUtc = company.UpdatedAtUtc;

        await dbContext.SaveChangesAsync(cancellationToken);
        return Map(entity);
    }

    public async Task<IReadOnlyList<ClientProject>> ListProjectsAsync(Guid companyId, CancellationToken cancellationToken = default)
    {
        await using var dbContext = await _dbContextFactory.CreateDbContextAsync(cancellationToken);
        var items = await dbContext.ClientProjects
            .AsNoTracking()
            .Where(project => project.ClientCompanyId == companyId)
            .OrderBy(static project => project.Name)
            .ToListAsync(cancellationToken);

        return items.Select(Map).ToArray();
    }

    public async Task<ClientProject?> GetProjectByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        await using var dbContext = await _dbContextFactory.CreateDbContextAsync(cancellationToken);
        var item = await dbContext.ClientProjects
            .AsNoTracking()
            .FirstOrDefaultAsync(project => project.Id == id, cancellationToken);

        return item is null ? null : Map(item);
    }

    public async Task<ClientProject?> GetProjectByCompanyAndKeyAsync(Guid companyId, string key, CancellationToken cancellationToken = default)
    {
        await using var dbContext = await _dbContextFactory.CreateDbContextAsync(cancellationToken);
        var item = await dbContext.ClientProjects
            .AsNoTracking()
            .FirstOrDefaultAsync(project => project.ClientCompanyId == companyId && project.Key == key, cancellationToken);

        return item is null ? null : Map(item);
    }

    public async Task<ClientProject> AddProjectAsync(ClientProject project, CancellationToken cancellationToken = default)
    {
        await using var dbContext = await _dbContextFactory.CreateDbContextAsync(cancellationToken);
        var entity = ToEntity(project);
        dbContext.ClientProjects.Add(entity);
        await dbContext.SaveChangesAsync(cancellationToken);
        return Map(entity);
    }

    public async Task<ClientProject?> UpdateProjectAsync(ClientProject project, CancellationToken cancellationToken = default)
    {
        await using var dbContext = await _dbContextFactory.CreateDbContextAsync(cancellationToken);
        var entity = await dbContext.ClientProjects.FirstOrDefaultAsync(item => item.Id == project.Id, cancellationToken);
        if (entity is null)
        {
            return null;
        }

        entity.Name = project.Name;
        entity.Description = project.Description;
        entity.Status = (short)project.Status;
        entity.UpdatedAtUtc = project.UpdatedAtUtc;

        await dbContext.SaveChangesAsync(cancellationToken);
        return Map(entity);
    }

    public async Task<IReadOnlyList<ProjectRepository>> ListRepositoriesAsync(Guid projectId, CancellationToken cancellationToken = default)
    {
        await using var dbContext = await _dbContextFactory.CreateDbContextAsync(cancellationToken);
        var items = await dbContext.ProjectRepositories
            .AsNoTracking()
            .Where(repository => repository.ClientProjectId == projectId)
            .OrderBy(static repository => repository.Name)
            .ToListAsync(cancellationToken);

        return items.Select(Map).ToArray();
    }

    public async Task<ProjectRepository?> GetRepositoryByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        await using var dbContext = await _dbContextFactory.CreateDbContextAsync(cancellationToken);
        var item = await dbContext.ProjectRepositories
            .AsNoTracking()
            .FirstOrDefaultAsync(repository => repository.Id == id, cancellationToken);

        return item is null ? null : Map(item);
    }

    public async Task<ProjectRepository?> GetRepositoryByProjectAndKeyAsync(Guid projectId, string key, CancellationToken cancellationToken = default)
    {
        await using var dbContext = await _dbContextFactory.CreateDbContextAsync(cancellationToken);
        var item = await dbContext.ProjectRepositories
            .AsNoTracking()
            .FirstOrDefaultAsync(repository => repository.ClientProjectId == projectId && repository.Key == key, cancellationToken);

        return item is null ? null : Map(item);
    }

    public async Task<ProjectRepository?> GetRepositoryByLocalPathAsync(string localPath, CancellationToken cancellationToken = default)
    {
        await using var dbContext = await _dbContextFactory.CreateDbContextAsync(cancellationToken);
        var item = await dbContext.ProjectRepositories
            .AsNoTracking()
            .FirstOrDefaultAsync(repository => repository.LocalPath == localPath, cancellationToken);

        return item is null ? null : Map(item);
    }

    public async Task<ProjectRepository> AddRepositoryAsync(ProjectRepository repository, CancellationToken cancellationToken = default)
    {
        await using var dbContext = await _dbContextFactory.CreateDbContextAsync(cancellationToken);
        var entity = ToEntity(repository);
        dbContext.ProjectRepositories.Add(entity);
        await dbContext.SaveChangesAsync(cancellationToken);
        return Map(entity);
    }

    public async Task<ProjectRepository?> UpdateRepositoryAsync(ProjectRepository repository, CancellationToken cancellationToken = default)
    {
        await using var dbContext = await _dbContextFactory.CreateDbContextAsync(cancellationToken);
        var entity = await dbContext.ProjectRepositories.FirstOrDefaultAsync(item => item.Id == repository.Id, cancellationToken);
        if (entity is null)
        {
            return null;
        }

        entity.Name = repository.Name;
        entity.RemoteRepositoryUrl = repository.RemoteRepositoryUrl;
        entity.LocalPath = repository.LocalPath;
        entity.GitStrategySkillPath = repository.GitStrategySkillPath;
        entity.Description = repository.Description;
        entity.Status = (short)repository.Status;
        entity.UpdatedAtUtc = repository.UpdatedAtUtc;

        await dbContext.SaveChangesAsync(cancellationToken);
        return Map(entity);
    }

    private static ClientCompany Map(ClientCompanyEntity entity) =>
        new()
        {
            Id = entity.Id,
            Key = entity.Key,
            Name = entity.Name,
            Description = entity.Description,
            Status = (ClientResourceStatus)entity.Status,
            CreatedAtUtc = entity.CreatedAtUtc,
            UpdatedAtUtc = entity.UpdatedAtUtc
        };

    private static ClientProject Map(ClientProjectEntity entity) =>
        new()
        {
            Id = entity.Id,
            ClientCompanyId = entity.ClientCompanyId,
            Key = entity.Key,
            Name = entity.Name,
            Description = entity.Description,
            Status = (ClientResourceStatus)entity.Status,
            CreatedAtUtc = entity.CreatedAtUtc,
            UpdatedAtUtc = entity.UpdatedAtUtc
        };

    private static ProjectRepository Map(ProjectRepositoryEntity entity) =>
        new()
        {
            Id = entity.Id,
            ClientProjectId = entity.ClientProjectId,
            Key = entity.Key,
            Name = entity.Name,
            RemoteRepositoryUrl = entity.RemoteRepositoryUrl,
            LocalPath = entity.LocalPath,
            GitStrategySkillPath = entity.GitStrategySkillPath,
            Description = entity.Description,
            Status = (ClientResourceStatus)entity.Status,
            CreatedAtUtc = entity.CreatedAtUtc,
            UpdatedAtUtc = entity.UpdatedAtUtc
        };

    private static ClientCompanyEntity ToEntity(ClientCompany company) =>
        new()
        {
            Id = company.Id,
            Key = company.Key,
            Name = company.Name,
            Description = company.Description,
            Status = (short)company.Status,
            CreatedAtUtc = company.CreatedAtUtc,
            UpdatedAtUtc = company.UpdatedAtUtc
        };

    private static ClientProjectEntity ToEntity(ClientProject project) =>
        new()
        {
            Id = project.Id,
            ClientCompanyId = project.ClientCompanyId,
            Key = project.Key,
            Name = project.Name,
            Description = project.Description,
            Status = (short)project.Status,
            CreatedAtUtc = project.CreatedAtUtc,
            UpdatedAtUtc = project.UpdatedAtUtc
        };

    private static ProjectRepositoryEntity ToEntity(ProjectRepository repository) =>
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
            Status = (short)repository.Status,
            CreatedAtUtc = repository.CreatedAtUtc,
            UpdatedAtUtc = repository.UpdatedAtUtc
        };
}
