namespace IdeaForge.Application.Agents;

public interface IAgentConversationService
{
    Task<AgentConversationResponse> ChatAsync(Guid employeeId, AgentConversationRequest request, CancellationToken cancellationToken = default);

    Task<AgentConversationResponse> ChatByKeyAsync(string key, AgentConversationRequest request, CancellationToken cancellationToken = default);
}
