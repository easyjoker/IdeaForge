namespace IdeaForge.Application.Agents;

public interface IAgentProfileService
{
    Task<IReadOnlyList<AgentProfileDto>> ListAsync(CancellationToken cancellationToken = default);

    Task<AgentProfileDto?> GetAsync(Guid id, CancellationToken cancellationToken = default);

    Task<AgentProfileDto?> GetByKeyAsync(string key, CancellationToken cancellationToken = default);

    Task<AgentProfileDto> CreateAsync(CreateAgentProfileRequest request, CancellationToken cancellationToken = default);

    Task<AgentProfileDto?> UpdateAsync(Guid id, UpdateAgentProfileRequest request, CancellationToken cancellationToken = default);

    Task<AgentExecutionDto> ReportExecutionAsync(Guid employeeId, ReportAgentExecutionRequest request, CancellationToken cancellationToken = default);

    Task<AgentExecutionDto> ReportExecutionByKeyAsync(string key, ReportAgentExecutionRequest request, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<AgentExecutionDto>> ListExecutionsAsync(Guid employeeId, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<AgentExecutionDto>> ListExecutionsByKeyAsync(string key, CancellationToken cancellationToken = default);
}
