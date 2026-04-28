using IdeaForge.Agents.Abstractions;

namespace IdeaForge.Application.Agents;

public sealed class AgentProfileService : IAgentProfileService
{
    private readonly IAgentProfileRepository _repository;

    public AgentProfileService(IAgentProfileRepository repository)
    {
        _repository = repository;
    }

    public async Task<IReadOnlyList<AgentProfileDto>> ListAsync(CancellationToken cancellationToken = default)
    {
        var items = await _repository.ListAsync(cancellationToken);
        return items.Select(Map).ToArray();
    }

    public async Task<AgentProfileDto?> GetAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var item = await _repository.GetByIdAsync(id, cancellationToken);
        return item is null ? null : Map(item);
    }

    public async Task<AgentProfileDto?> GetByKeyAsync(string key, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(key);

        var item = await _repository.GetByKeyAsync(key.Trim(), cancellationToken);
        return item is null ? null : Map(item);
    }

    public async Task<AgentProfileDto> CreateAsync(CreateAgentProfileRequest request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        var key = RequireText(request.Key, nameof(request.Key));
        var existing = await _repository.GetByKeyAsync(key, cancellationToken);
        if (existing is not null)
        {
            throw new InvalidOperationException($"An agent with key '{key}' already exists.");
        }

        var now = DateTimeOffset.UtcNow;
        var provider = ResolveProvider(request.Provider, existing: null);
        var profile = new AgentProfile
        {
            Person = new Person
            {
                Kind = PersonKind.Ai,
                DisplayName = RequireText(request.DisplayName ?? request.Name, nameof(request.DisplayName)),
                Description = NormalizeOptional(request.PersonDescription ?? request.Description),
                Metadata = new Dictionary<string, string>(request.PersonMetadata, StringComparer.OrdinalIgnoreCase),
                CreatedAtUtc = now,
                UpdatedAtUtc = now
            },
            Employee = new Employee
            {
                Key = key,
                Role = request.Role?.Trim(),
                Mission = request.Mission?.Trim(),
                Specialties = request.Specialties.Where(static item => !string.IsNullOrWhiteSpace(item)).Select(static item => item.Trim()).Distinct(StringComparer.OrdinalIgnoreCase).ToList(),
                Metadata = new Dictionary<string, string>(request.Metadata, StringComparer.OrdinalIgnoreCase),
                Status = AgentStatus.Active,
                CreatedAtUtc = now,
                UpdatedAtUtc = now
            },
            AgentData = new AgentData
            {
                PersonId = Guid.Empty,
                Provider = provider,
                Model = string.IsNullOrWhiteSpace(request.Model) ? "gpt-5.4" : request.Model.Trim(),
                SystemPrompt = NormalizeOptional(request.SystemPrompt),
                ProviderSettings = BuildProviderSettings(provider, request)
            }
        };

        profile.Employee.PersonId = profile.Person.Id;
        profile.AgentData.PersonId = profile.Person.Id;

        var created = await _repository.AddAsync(profile, cancellationToken);
        return Map(created);
    }

    public async Task<AgentProfileDto?> UpdateAsync(Guid id, UpdateAgentProfileRequest request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        var existing = await _repository.GetByIdAsync(id, cancellationToken);
        if (existing is null)
        {
            return null;
        }

        existing.Person.DisplayName = RequireText(request.DisplayName ?? request.Name, nameof(request.DisplayName));
        existing.Person.Description = NormalizeOptional(request.PersonDescription ?? request.Description);
        existing.Person.Metadata = new Dictionary<string, string>(request.PersonMetadata, StringComparer.OrdinalIgnoreCase);
        existing.Person.UpdatedAtUtc = DateTimeOffset.UtcNow;

        existing.Employee.Role = request.Role?.Trim();
        existing.Employee.Mission = request.Mission?.Trim();
        existing.Employee.Status = request.Status;
        existing.Employee.Specialties = request.Specialties.Where(static item => !string.IsNullOrWhiteSpace(item)).Select(static item => item.Trim()).Distinct(StringComparer.OrdinalIgnoreCase).ToList();
        existing.Employee.Metadata = new Dictionary<string, string>(request.Metadata, StringComparer.OrdinalIgnoreCase);
        existing.Employee.UpdatedAtUtc = DateTimeOffset.UtcNow;

        var provider = ResolveProvider(request.Provider, existing.AgentData.Provider);
        existing.AgentData.Provider = provider;
        existing.AgentData.Model = string.IsNullOrWhiteSpace(request.Model) ? existing.AgentData.Model : request.Model.Trim();
        existing.AgentData.SystemPrompt = NormalizeOptional(request.SystemPrompt);
        existing.AgentData.ProviderSettings = BuildProviderSettings(provider, request);

        var updated = await _repository.UpdateAsync(existing, cancellationToken);
        return Map(updated);
    }

    public async Task<AgentExecutionDto> ReportExecutionAsync(Guid employeeId, ReportAgentExecutionRequest request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        var profile = await _repository.GetByIdAsync(employeeId, cancellationToken)
            ?? throw new KeyNotFoundException($"Employee '{employeeId}' was not found.");

        var execution = new AgentExecutionRecord
        {
            EmployeeId = employeeId,
            Provider = ResolveProvider(request.Provider, profile.AgentData.Provider),
            Model = string.IsNullOrWhiteSpace(request.Model) ? profile.AgentData.Model : request.Model.Trim(),
            SessionId = string.IsNullOrWhiteSpace(request.SessionId) ? null : request.SessionId.Trim(),
            Prompt = request.Prompt.Trim(),
            OutputSummary = request.OutputSummary?.Trim(),
            Success = request.Success,
            ErrorMessage = request.ErrorMessage?.Trim(),
            StartedAtUtc = request.StartedAtUtc,
            CompletedAtUtc = request.CompletedAtUtc
        };

        if (!string.IsNullOrWhiteSpace(execution.SessionId) && string.IsNullOrWhiteSpace(profile.AgentData.SessionId))
        {
            profile.AgentData.SessionId = execution.SessionId;
            profile.AgentData.SessionUpdatedAtUtc = DateTimeOffset.UtcNow;
        }

        profile.AgentData.Provider = execution.Provider;
        profile.AgentData.Model = execution.Model;
        profile.AgentData.LastUsedAtUtc = execution.CompletedAtUtc;

        await _repository.UpdateAsync(profile, cancellationToken);
        var saved = await _repository.AddExecutionAsync(execution, cancellationToken);

        return Map(saved);
    }

    public async Task<AgentExecutionDto> ReportExecutionByKeyAsync(string key, ReportAgentExecutionRequest request, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(key);
        ArgumentNullException.ThrowIfNull(request);

        var profile = await _repository.GetByKeyAsync(key.Trim(), cancellationToken)
            ?? throw new KeyNotFoundException($"Employee key '{key}' was not found.");

        return await ReportExecutionAsync(profile.Employee.Id, request, cancellationToken);
    }

    public async Task<IReadOnlyList<AgentExecutionDto>> ListExecutionsAsync(Guid employeeId, CancellationToken cancellationToken = default)
    {
        var items = await _repository.ListExecutionsAsync(employeeId, cancellationToken);
        return items.Select(Map).ToArray();
    }

    public async Task<IReadOnlyList<AgentExecutionDto>> ListExecutionsByKeyAsync(string key, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(key);

        var profile = await _repository.GetByKeyAsync(key.Trim(), cancellationToken)
            ?? throw new KeyNotFoundException($"Employee key '{key}' was not found.");

        return await ListExecutionsAsync(profile.Employee.Id, cancellationToken);
    }

    private static AgentProfileDto Map(AgentProfile profile) =>
        new()
        {
            Employee = new EmployeeDto
            {
                Id = profile.Employee.Id,
                PersonId = profile.Employee.PersonId,
                Key = profile.Employee.Key,
                Role = profile.Employee.Role,
                Mission = profile.Employee.Mission,
                Status = profile.Employee.Status,
                Specialties = profile.Employee.Specialties.ToArray(),
                Metadata = new Dictionary<string, string>(profile.Employee.Metadata, StringComparer.OrdinalIgnoreCase),
                CreatedAtUtc = profile.Employee.CreatedAtUtc,
                UpdatedAtUtc = profile.Employee.UpdatedAtUtc
            },
            Person = new PersonDto
            {
                Id = profile.Person.Id,
                Kind = profile.Person.Kind,
                DisplayName = profile.Person.DisplayName,
                Description = profile.Person.Description,
                Metadata = new Dictionary<string, string>(profile.Person.Metadata, StringComparer.OrdinalIgnoreCase),
                CreatedAtUtc = profile.Person.CreatedAtUtc,
                UpdatedAtUtc = profile.Person.UpdatedAtUtc
            },
            AgentData = new AgentDataDto
            {
                Provider = profile.AgentData.Provider,
                Model = profile.AgentData.Model,
                SessionId = profile.AgentData.SessionId,
                SystemPrompt = profile.AgentData.SystemPrompt,
                CodexReasoning = AgentProviderSettings.Get(profile.AgentData.ProviderSettings, AgentProviderSettings.CodexReasoning),
                CodexEffort = AgentProviderSettings.Get(profile.AgentData.ProviderSettings, AgentProviderSettings.CodexEffort),
                CodexCompute = AgentProviderSettings.Get(profile.AgentData.ProviderSettings, AgentProviderSettings.CodexCompute),
                LastUsedAtUtc = profile.AgentData.LastUsedAtUtc,
                SessionUpdatedAtUtc = profile.AgentData.SessionUpdatedAtUtc
            }
        };

    private static string RequireText(string? value, string parameterName)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException($"{parameterName} is required.", parameterName);
        }

        return value.Trim();
    }

    private static string? NormalizeOptional(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    private static string? NormalizeCodexIntensity(string? value, string parameterName)
    {
        var normalized = NormalizeOptional(value)?.ToLowerInvariant();
        if (normalized is null)
        {
            return null;
        }

        return normalized is "low" or "medium" or "high"
            ? normalized
            : throw new ArgumentException($"{parameterName} must be low, medium, or high.", parameterName);
    }

    private static Dictionary<string, string> BuildProviderSettings(AgentProviderKind provider, CreateAgentProfileRequest request)
    {
        if (provider != AgentProviderKind.Codex)
        {
            return new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        }

        var settings = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        AddOptional(settings, AgentProviderSettings.CodexReasoning, NormalizeCodexIntensity(request.CodexReasoning, nameof(request.CodexReasoning)));
        AddOptional(settings, AgentProviderSettings.CodexEffort, NormalizeCodexIntensity(request.CodexEffort, nameof(request.CodexEffort)));
        AddOptional(settings, AgentProviderSettings.CodexCompute, NormalizeCodexIntensity(request.CodexCompute, nameof(request.CodexCompute)));
        return settings;
    }

    private static Dictionary<string, string> BuildProviderSettings(AgentProviderKind provider, UpdateAgentProfileRequest request)
    {
        if (provider != AgentProviderKind.Codex)
        {
            return new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        }

        var settings = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        AddOptional(settings, AgentProviderSettings.CodexReasoning, NormalizeCodexIntensity(request.CodexReasoning, nameof(request.CodexReasoning)));
        AddOptional(settings, AgentProviderSettings.CodexEffort, NormalizeCodexIntensity(request.CodexEffort, nameof(request.CodexEffort)));
        AddOptional(settings, AgentProviderSettings.CodexCompute, NormalizeCodexIntensity(request.CodexCompute, nameof(request.CodexCompute)));
        return settings;
    }

    private static void AddOptional(Dictionary<string, string> settings, string key, string? value)
    {
        if (!string.IsNullOrWhiteSpace(value))
        {
            settings[key] = value;
        }
    }

    private static AgentProviderKind ResolveProvider(AgentProviderKind requested, AgentProviderKind? existing) =>
        requested is AgentProviderKind.Copilot or AgentProviderKind.Codex
            ? requested
            : existing is AgentProviderKind.Copilot or AgentProviderKind.Codex
                ? existing.Value
                : AgentProviderKind.Codex;

    private static AgentExecutionDto Map(AgentExecutionRecord execution) =>
        new()
        {
            Id = execution.Id,
            EmployeeId = execution.EmployeeId,
            Provider = execution.Provider,
            Model = execution.Model,
            SessionId = execution.SessionId,
            Prompt = execution.Prompt,
            OutputSummary = execution.OutputSummary,
            Success = execution.Success,
            ErrorMessage = execution.ErrorMessage,
            StartedAtUtc = execution.StartedAtUtc,
            CompletedAtUtc = execution.CompletedAtUtc
        };
}
