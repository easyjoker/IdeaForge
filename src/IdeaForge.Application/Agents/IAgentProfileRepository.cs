using IdeaForge.Agents.Abstractions;

namespace IdeaForge.Application.Agents;

public interface IAgentProfileRepository
{
    Task<IReadOnlyList<AgentProfile>> ListAsync(CancellationToken cancellationToken = default);

    Task<AgentProfile?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<AgentProfile?> GetByKeyAsync(string key, CancellationToken cancellationToken = default);

    Task<AgentProfile> AddAsync(AgentProfile profile, CancellationToken cancellationToken = default);

    Task<AgentProfile> UpdateAsync(AgentProfile profile, CancellationToken cancellationToken = default);

    Task<AgentExecutionRecord> AddExecutionAsync(AgentExecutionRecord execution, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<AgentExecutionRecord>> ListExecutionsAsync(Guid employeeId, CancellationToken cancellationToken = default);
}
