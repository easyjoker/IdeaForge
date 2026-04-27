using IdeaForge.Application.Agents;
using IdeaForge.Application.ClientProjects;
using IdeaForge.Application.Personnel;
using Microsoft.Extensions.DependencyInjection;

namespace IdeaForge.Application;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddIdeaForgeApplication(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.AddScoped<IAgentProfileService, AgentProfileService>();
        services.AddScoped<IAgentConversationService, AgentConversationService>();
        services.AddScoped<IClientProjectService, ClientProjectService>();
        services.AddScoped<IPersonnelService, PersonnelService>();
        return services;
    }
}
