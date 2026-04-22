namespace IdeaForge.Infrastructure.Persistence;

public interface IPostgreSqlInitializer
{
    Task InitializeAsync(CancellationToken cancellationToken = default);
}
