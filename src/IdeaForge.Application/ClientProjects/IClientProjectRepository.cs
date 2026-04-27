namespace IdeaForge.Application.ClientProjects;

public interface IClientProjectRepository
{
    Task<IReadOnlyList<ClientCompany>> ListCompaniesAsync(CancellationToken cancellationToken = default);

    Task<ClientCompany?> GetCompanyByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<ClientCompany?> GetCompanyByKeyAsync(string key, CancellationToken cancellationToken = default);

    Task<ClientCompany> AddCompanyAsync(ClientCompany company, CancellationToken cancellationToken = default);

    Task<ClientCompany?> UpdateCompanyAsync(ClientCompany company, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<ClientProject>> ListProjectsAsync(Guid companyId, CancellationToken cancellationToken = default);

    Task<ClientProject?> GetProjectByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<ClientProject?> GetProjectByCompanyAndKeyAsync(Guid companyId, string key, CancellationToken cancellationToken = default);

    Task<ClientProject> AddProjectAsync(ClientProject project, CancellationToken cancellationToken = default);

    Task<ClientProject?> UpdateProjectAsync(ClientProject project, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<ProjectRepository>> ListRepositoriesAsync(Guid projectId, CancellationToken cancellationToken = default);

    Task<ProjectRepository?> GetRepositoryByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<ProjectRepository?> GetRepositoryByProjectAndKeyAsync(Guid projectId, string key, CancellationToken cancellationToken = default);

    Task<ProjectRepository?> GetRepositoryByLocalPathAsync(string localPath, CancellationToken cancellationToken = default);

    Task<ProjectRepository> AddRepositoryAsync(ProjectRepository repository, CancellationToken cancellationToken = default);

    Task<ProjectRepository?> UpdateRepositoryAsync(ProjectRepository repository, CancellationToken cancellationToken = default);
}
