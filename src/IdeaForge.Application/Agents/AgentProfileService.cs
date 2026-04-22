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

        var existing = await _repository.GetByKeyAsync(request.Key, cancellationToken);
        if (existing is not null)
        {
            throw new InvalidOperationException($"An agent with key '{request.Key}' already exists.");
        }

        var profile = new AgentProfile
        {
            Employee = new Employee
            {
                Key = request.Key.Trim(),
                Name = request.Name.Trim(),
                Role = request.Role?.Trim(),
                Description = request.Description?.Trim(),
                Mission = request.Mission?.Trim(),
                Specialties = request.Specialties.Where(static item => !string.IsNullOrWhiteSpace(item)).Select(static item => item.Trim()).Distinct(StringComparer.OrdinalIgnoreCase).ToList(),
                Metadata = new Dictionary<string, string>(request.Metadata, StringComparer.OrdinalIgnoreCase),
                Status = AgentStatus.Active
            },
            AgentData = new AgentData
            {
                EmployeeId = Guid.Empty,
                Provider = request.Provider,
                Model = string.IsNullOrWhiteSpace(request.Model) ? "gpt-5.4" : request.Model.Trim(),
                SystemPrompt = request.SystemPrompt
            }
        };

        profile.AgentData.EmployeeId = profile.Employee.Id;

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

        existing.Employee.Name = request.Name.Trim();
        existing.Employee.Role = request.Role?.Trim();
        existing.Employee.Description = request.Description?.Trim();
        existing.Employee.Mission = request.Mission?.Trim();
        existing.Employee.Status = request.Status;
        existing.Employee.Specialties = request.Specialties.Where(static item => !string.IsNullOrWhiteSpace(item)).Select(static item => item.Trim()).Distinct(StringComparer.OrdinalIgnoreCase).ToList();
        existing.Employee.Metadata = new Dictionary<string, string>(request.Metadata, StringComparer.OrdinalIgnoreCase);
        existing.Employee.UpdatedAtUtc = DateTimeOffset.UtcNow;

        existing.AgentData.Provider = request.Provider;
        existing.AgentData.Model = string.IsNullOrWhiteSpace(request.Model) ? existing.AgentData.Model : request.Model.Trim();
        existing.AgentData.SystemPrompt = request.SystemPrompt;

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
            Provider = request.Provider,
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
                Key = profile.Employee.Key,
                Name = profile.Employee.Name,
                Role = profile.Employee.Role,
                Description = profile.Employee.Description,
                Mission = profile.Employee.Mission,
                Status = profile.Employee.Status,
                Specialties = profile.Employee.Specialties.ToArray(),
                Metadata = new Dictionary<string, string>(profile.Employee.Metadata, StringComparer.OrdinalIgnoreCase),
                CreatedAtUtc = profile.Employee.CreatedAtUtc,
                UpdatedAtUtc = profile.Employee.UpdatedAtUtc
            },
            AgentData = new AgentDataDto
            {
                Provider = profile.AgentData.Provider,
                Model = profile.AgentData.Model,
                SessionId = profile.AgentData.SessionId,
                SystemPrompt = profile.AgentData.SystemPrompt,
                LastUsedAtUtc = profile.AgentData.LastUsedAtUtc,
                SessionUpdatedAtUtc = profile.AgentData.SessionUpdatedAtUtc
            }
        };

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
