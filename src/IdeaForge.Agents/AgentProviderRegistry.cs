using IdeaForge.Agents.Abstractions;

namespace IdeaForge.Agents;

public sealed class AgentProviderRegistry
{
    private readonly Dictionary<AgentProviderKind, IAgentProvider> _providers;

    public AgentProviderRegistry(IEnumerable<IAgentProvider> providers)
    {
        ArgumentNullException.ThrowIfNull(providers);

        _providers = providers.ToDictionary(provider => provider.Kind);
    }

    public IReadOnlyCollection<IAgentProvider> Providers => _providers.Values;

    public IAgentProvider GetRequiredProvider(AgentProviderKind kind)
    {
        if (_providers.TryGetValue(kind, out var provider))
        {
            return provider;
        }

        throw new InvalidOperationException($"No agent provider has been registered for '{kind}'.");
    }
}
