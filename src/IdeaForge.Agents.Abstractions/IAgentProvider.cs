namespace IdeaForge.Agents.Abstractions;

public interface IAgentProvider
{
    string Name { get; }

    AgentProviderKind Kind { get; }

    AgentProviderCapabilities Capabilities { get; }

    Task<AgentExecutionResult> ExecuteAsync(
        AgentExecutionRequest request,
        CancellationToken cancellationToken = default);
}
