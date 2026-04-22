using IdeaForge.Agents.Abstractions;

namespace IdeaForge.Application.Agents;

public sealed class AgentConversationService : IAgentConversationService
{
    private readonly IAgentProfileRepository _repository;
    private readonly IReadOnlyDictionary<AgentProviderKind, IAgentProvider> _providers;

    public AgentConversationService(
        IAgentProfileRepository repository,
        IEnumerable<IAgentProvider> providers)
    {
        ArgumentNullException.ThrowIfNull(repository);
        ArgumentNullException.ThrowIfNull(providers);

        _repository = repository;
        _providers = providers.ToDictionary(provider => provider.Kind);
    }

    public async Task<AgentConversationResponse> ChatAsync(
        Guid employeeId,
        AgentConversationRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        var profile = await _repository.GetByIdAsync(employeeId, cancellationToken)
            ?? throw new KeyNotFoundException($"Employee '{employeeId}' was not found.");

        return await ChatInternalAsync(profile, request, cancellationToken);
    }

    public async Task<AgentConversationResponse> ChatByKeyAsync(
        string key,
        AgentConversationRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(key);
        ArgumentNullException.ThrowIfNull(request);

        var profile = await _repository.GetByKeyAsync(key.Trim(), cancellationToken)
            ?? throw new KeyNotFoundException($"Employee key '{key}' was not found.");

        return await ChatInternalAsync(profile, request, cancellationToken);
    }

    private async Task<AgentConversationResponse> ChatInternalAsync(
        AgentProfile profile,
        AgentConversationRequest request,
        CancellationToken cancellationToken)
    {
        if (profile.Employee.Status != AgentStatus.Active)
        {
            throw new InvalidOperationException($"Employee '{profile.Employee.Key}' is not active.");
        }

        if (!_providers.TryGetValue(profile.AgentData.Provider, out var provider))
        {
            throw new InvalidOperationException($"No provider is registered for '{profile.AgentData.Provider}'.");
        }

        var startedAtUtc = DateTimeOffset.UtcNow;
        var executionRequest = new AgentExecutionRequest
        {
            Prompt = BuildPrompt(profile, request.Prompt),
            Model = profile.AgentData.Model,
            SessionId = provider.Capabilities.SupportsNamedSessions ? profile.AgentData.SessionId : null,
            WorkingDirectory = request.WorkingDirectory,
            AdditionalDirectories = request.AdditionalDirectories
                .Where(static path => !string.IsNullOrWhiteSpace(path))
                .Select(static path => path.Trim())
                .ToArray(),
            Metadata = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                ["employeeId"] = profile.Employee.Id.ToString(),
                ["employeeKey"] = profile.Employee.Key
            }
        };

        var result = await provider.ExecuteAsync(executionRequest, cancellationToken);
        var completedAtUtc = DateTimeOffset.UtcNow;

        profile.AgentData.Provider = result.Provider;
        profile.AgentData.Model = string.IsNullOrWhiteSpace(result.Model) ? profile.AgentData.Model : result.Model;
        profile.AgentData.LastUsedAtUtc = completedAtUtc;

        if (!string.IsNullOrWhiteSpace(result.SessionId) &&
            !string.Equals(profile.AgentData.SessionId, result.SessionId, StringComparison.Ordinal))
        {
            profile.AgentData.SessionId = result.SessionId;
            profile.AgentData.SessionUpdatedAtUtc = completedAtUtc;
        }

        await _repository.UpdateAsync(profile, cancellationToken);

        var execution = new AgentExecutionRecord
        {
            EmployeeId = profile.Employee.Id,
            Provider = result.Provider,
            Model = profile.AgentData.Model,
            SessionId = result.SessionId,
            Prompt = request.Prompt.Trim(),
            OutputSummary = SummarizeOutput(result.Output, result.ErrorOutput),
            Success = result.ExitCode == 0,
            ErrorMessage = result.ErrorOutput,
            StartedAtUtc = startedAtUtc,
            CompletedAtUtc = completedAtUtc
        };

        await _repository.AddExecutionAsync(execution, cancellationToken);

        return new AgentConversationResponse
        {
            EmployeeId = profile.Employee.Id,
            EmployeeKey = profile.Employee.Key,
            EmployeeName = profile.Employee.Name,
            Provider = result.Provider,
            Model = profile.AgentData.Model,
            SessionId = profile.AgentData.SessionId,
            ExitCode = result.ExitCode,
            Output = result.Output,
            ErrorOutput = result.ErrorOutput,
            StartedAtUtc = startedAtUtc,
            CompletedAtUtc = completedAtUtc
        };
    }

    private static string BuildPrompt(AgentProfile profile, string prompt)
    {
        var trimmedPrompt = prompt.Trim();
        if (string.IsNullOrWhiteSpace(profile.AgentData.SystemPrompt))
        {
            return trimmedPrompt;
        }

        return $"""
            System instructions:
            {profile.AgentData.SystemPrompt.Trim()}

            User request:
            {trimmedPrompt}
            """;
    }

    private static string? SummarizeOutput(string output, string? errorOutput)
    {
        var text = string.IsNullOrWhiteSpace(output) ? errorOutput : output;
        if (string.IsNullOrWhiteSpace(text))
        {
            return null;
        }

        var trimmed = text.Trim();
        return trimmed.Length <= 1000 ? trimmed : trimmed[..1000];
    }
}
