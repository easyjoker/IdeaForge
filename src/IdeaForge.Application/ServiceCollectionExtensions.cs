using IdeaForge.Application.Agents;
using Microsoft.Extensions.DependencyInjection;

namespace IdeaForge.Application;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddIdeaForgeApplication(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.AddScoped<IAgentProfileService, AgentProfileService>();
        services.AddScoped<IAgentConversationService, AgentConversationService>();
        return services;
    }
}
