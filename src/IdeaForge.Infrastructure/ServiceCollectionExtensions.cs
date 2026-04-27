using IdeaForge.Application.Agents;
using IdeaForge.Infrastructure.Agents;
using IdeaForge.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace IdeaForge.Infrastructure;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddIdeaForgeInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);

        var options = new PostgreSqlOptions
        {
            ConnectionString = configuration.GetConnectionString("IdeaForgeDb") ?? string.Empty
        };

        services.AddSingleton(options);
        services.AddDbContextFactory<IdeaForgeDbContext>(builder => builder.UseNpgsql(options.ConnectionString));
        services.AddSingleton<IPostgreSqlInitializer, PostgreSqlInitializer>();
        services.AddScoped<IAgentProfileRepository, PostgreSqlAgentProfileRepository>();
        return services;
    }
}
