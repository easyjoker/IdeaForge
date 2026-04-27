namespace IdeaForge.Application.ClientProjects;

public interface IClientProjectService
{
    Task<IReadOnlyList<ClientCompanyDto>> ListCompaniesAsync(CancellationToken cancellationToken = default);

    Task<ClientCompanyDto?> GetCompanyAsync(Guid id, CancellationToken cancellationToken = default);

    Task<ClientCompanyDto?> GetCompanyByKeyAsync(string key, CancellationToken cancellationToken = default);

    Task<ClientCompanyDto> CreateCompanyAsync(CreateClientCompanyRequest request, CancellationToken cancellationToken = default);

    Task<ClientCompanyDto?> UpdateCompanyAsync(Guid id, UpdateClientCompanyRequest request, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<ClientProjectDto>> ListProjectsAsync(Guid companyId, CancellationToken cancellationToken = default);

    Task<ClientProjectDto?> GetProjectAsync(Guid id, CancellationToken cancellationToken = default);

    Task<ClientProjectDto> CreateProjectAsync(Guid companyId, CreateClientProjectRequest request, CancellationToken cancellationToken = default);

    Task<ClientProjectDto?> UpdateProjectAsync(Guid id, UpdateClientProjectRequest request, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<ProjectRepositoryDto>> ListRepositoriesAsync(Guid projectId, CancellationToken cancellationToken = default);

    Task<ProjectRepositoryDto?> GetRepositoryAsync(Guid id, CancellationToken cancellationToken = default);

    Task<ProjectRepositoryDto> CreateRepositoryAsync(Guid projectId, CreateProjectRepositoryRequest request, CancellationToken cancellationToken = default);

    Task<ProjectRepositoryDto?> UpdateRepositoryAsync(Guid id, UpdateProjectRepositoryRequest request, CancellationToken cancellationToken = default);
}
