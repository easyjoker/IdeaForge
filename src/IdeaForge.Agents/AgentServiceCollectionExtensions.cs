using IdeaForge.Agents.Abstractions;
using IdeaForge.Agents.Codex;
using IdeaForge.Agents.Copilot;
using Microsoft.Extensions.DependencyInjection;

namespace IdeaForge.Agents;

public static class AgentServiceCollectionExtensions
{
    public static IServiceCollection AddIdeaForgeAgents(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.AddSingleton<IAgentProvider, CopilotAgentProvider>();
        services.AddSingleton<IAgentProvider, CodexAgentProvider>();
        services.AddSingleton<AgentProviderRegistry>();

        return services;
    }

    public static IServiceCollection AddIdeaForgeCodex(
        this IServiceCollection services,
        Action<CodexAgentOptions>? configure = null)
    {
        ArgumentNullException.ThrowIfNull(services);

        var options = new CodexAgentOptions();
        configure?.Invoke(options);

        services.AddSingleton(options);
        services.AddSingleton<CodexAgentProvider>();
        services.AddSingleton<IAgentProvider>(serviceProvider => serviceProvider.GetRequiredService<CodexAgentProvider>());

        return services;
    }

    public static IServiceCollection AddIdeaForgeCopilot(
        this IServiceCollection services,
        Action<CopilotAgentOptions>? configure = null)
    {
        ArgumentNullException.ThrowIfNull(services);

        var options = new CopilotAgentOptions();
        configure?.Invoke(options);

        services.AddSingleton(options);
        services.AddSingleton<CopilotAgentProvider>();
        services.AddSingleton<IAgentProvider>(serviceProvider => serviceProvider.GetRequiredService<CopilotAgentProvider>());

        return services;
    }
}
