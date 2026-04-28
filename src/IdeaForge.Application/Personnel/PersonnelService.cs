using IdeaForge.Agents.Abstractions;

namespace IdeaForge.Application.Personnel;

public sealed class PersonnelService : IPersonnelService
{
    private readonly IPersonnelRepository _repository;

    public PersonnelService(IPersonnelRepository repository)
    {
        _repository = repository;
    }

    public async Task<IReadOnlyList<PersonDirectoryDto>> ListPeopleAsync(CancellationToken cancellationToken = default)
    {
        var people = await _repository.ListPeopleAsync(cancellationToken);
        return people.Select(Map).ToArray();
    }

    public async Task<PersonDirectoryDto?> GetPersonAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var record = await _repository.GetPersonAsync(id, cancellationToken);
        return record is null ? null : Map(record);
    }

    public async Task<PersonDirectoryDto> CreatePersonAsync(CreatePersonRequest request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        EnsureSupportedKind(request.Kind);

        var displayName = RequireText(request.DisplayName, nameof(request.DisplayName));
        var existingPerson = await _repository.GetPersonByKindAndDisplayNameAsync(request.Kind, displayName, cancellationToken);
        if (existingPerson is not null)
        {
            throw new InvalidOperationException($"Person '{displayName}' already exists for kind '{request.Kind}'.");
        }

        var now = DateTimeOffset.UtcNow;
        var person = new Person
        {
            Kind = request.Kind,
            DisplayName = displayName,
            Description = NormalizeOptional(request.Description),
            Metadata = NormalizeMetadata(request.Metadata),
            CreatedAtUtc = now,
            UpdatedAtUtc = now
        };

        var agentData = person.Kind == PersonKind.Ai
            ? CreateAgentData(person.Id, request.AgentSettings, existing: null)
            : null;

        return Map(await _repository.AddPersonAsync(person, agentData, cancellationToken));
    }

    public async Task<PersonDirectoryDto?> UpdatePersonAsync(Guid id, UpdatePersonRequest request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        EnsureSupportedKind(request.Kind);

        var existing = await _repository.GetPersonAsync(id, cancellationToken);
        if (existing is null)
        {
            return null;
        }

        var person = existing.Person;
        person.Kind = request.Kind;
        person.DisplayName = RequireText(request.DisplayName, nameof(request.DisplayName));
        person.Description = NormalizeOptional(request.Description);
        person.Metadata = NormalizeMetadata(request.Metadata);
        person.UpdatedAtUtc = DateTimeOffset.UtcNow;

        var agentData = person.Kind == PersonKind.Ai
            ? CreateAgentData(person.Id, request.AgentSettings, existing.AgentData)
            : null;

        var updated = await _repository.UpdatePersonAsync(person, agentData, cancellationToken);
        return updated is null ? null : Map(updated);
    }

    public async Task<bool> DeletePersonAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var personRecord = await _repository.GetPersonAsync(id, cancellationToken);
        if (personRecord is null)
        {
            return false;
        }

        var employee = await _repository.GetEmployeeByPersonIdAsync(id, cancellationToken);
        if (employee is not null)
        {
            throw new InvalidOperationException($"Person '{personRecord.Person.DisplayName}' has an employee assignment. Delete the employee assignment first.");
        }

        return await _repository.DeletePersonAsync(id, cancellationToken);
    }

    public async Task<IReadOnlyList<EmployeeDirectoryDto>> ListEmployeesAsync(CancellationToken cancellationToken = default)
    {
        var employees = await _repository.ListEmployeesAsync(cancellationToken);
        return employees.Select(Map).ToArray();
    }

    public async Task<EmployeeDirectoryDto?> GetEmployeeAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var employee = await _repository.GetEmployeeAsync(id, cancellationToken);
        return employee is null ? null : Map(employee);
    }

    public async Task<EmployeeDirectoryDto> CreateEmployeeAsync(CreateEmployeeRequest request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        var personRecord = await _repository.GetPersonAsync(request.PersonId, cancellationToken)
            ?? throw new KeyNotFoundException($"Person '{request.PersonId}' was not found.");

        var existingForPerson = await _repository.GetEmployeeByPersonIdAsync(request.PersonId, cancellationToken);
        if (existingForPerson is not null)
        {
            throw new InvalidOperationException($"Person '{request.PersonId}' already has an employee record.");
        }

        var key = RequireText(request.Key, nameof(request.Key));
        var existingKey = await _repository.GetEmployeeByKeyAsync(key, cancellationToken);
        if (existingKey is not null)
        {
            throw new InvalidOperationException($"Employee key '{key}' already exists.");
        }

        var person = personRecord.Person;
        var now = DateTimeOffset.UtcNow;
        var employee = new Employee
        {
            PersonId = person.Id,
            Key = key,
            Role = NormalizeOptional(request.Role),
            Mission = NormalizeOptional(request.Mission),
            Status = AgentStatus.Active,
            Specialties = NormalizeList(request.Specialties),
            Metadata = NormalizeMetadata(request.Metadata),
            CreatedAtUtc = now,
            UpdatedAtUtc = now
        };

        return Map(await _repository.AddEmployeeAsync(employee, cancellationToken));
    }

    public async Task<EmployeeDirectoryDto?> UpdateEmployeeAsync(Guid id, UpdateEmployeeRequest request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        var existing = await _repository.GetEmployeeAsync(id, cancellationToken);
        if (existing is null)
        {
            return null;
        }

        var key = RequireText(request.Key, nameof(request.Key));
        var keyOwner = await _repository.GetEmployeeByKeyAsync(key, cancellationToken);
        if (keyOwner is not null && keyOwner.Employee.Id != id)
        {
            throw new InvalidOperationException($"Employee key '{key}' already exists.");
        }

        existing.Employee.Key = key;
        existing.Employee.Role = NormalizeOptional(request.Role);
        existing.Employee.Mission = NormalizeOptional(request.Mission);
        existing.Employee.Status = request.Status;
        existing.Employee.Specialties = NormalizeList(request.Specialties);
        existing.Employee.Metadata = NormalizeMetadata(request.Metadata);
        existing.Employee.UpdatedAtUtc = DateTimeOffset.UtcNow;

        return Map(await _repository.UpdateEmployeeAsync(existing.Employee, cancellationToken) ?? existing);
    }

    public Task<bool> DeleteEmployeeAsync(Guid id, CancellationToken cancellationToken = default) =>
        _repository.DeleteEmployeeAsync(id, cancellationToken);

    private static AgentData CreateAgentData(Guid personId, UpsertAgentSettingsRequest? request, AgentData? existing)
    {
        var provider = ResolveProvider(request?.Provider, existing?.Provider);
        return new AgentData
        {
            PersonId = personId,
            Provider = provider,
            Model = string.IsNullOrWhiteSpace(request?.Model) ? existing?.Model ?? "gpt-5.4" : request.Model.Trim(),
            SessionId = existing?.SessionId,
            SystemPrompt = request is null ? existing?.SystemPrompt : NormalizeOptional(request.SystemPrompt),
            CodexReasoning = provider == AgentProviderKind.Codex ? NormalizeCodexIntensity(request is null ? existing?.CodexReasoning : request.CodexReasoning, nameof(request.CodexReasoning)) : null,
            CodexEffort = provider == AgentProviderKind.Codex ? NormalizeCodexIntensity(request is null ? existing?.CodexEffort : request.CodexEffort, nameof(request.CodexEffort)) : null,
            CodexCompute = provider == AgentProviderKind.Codex ? NormalizeCodexIntensity(request is null ? existing?.CodexCompute : request.CodexCompute, nameof(request.CodexCompute)) : null,
            LastUsedAtUtc = existing?.LastUsedAtUtc,
            SessionUpdatedAtUtc = existing?.SessionUpdatedAtUtc
        };
    }

    private static AgentProviderKind ResolveProvider(AgentProviderKind? requested, AgentProviderKind? existing) =>
        requested is AgentProviderKind.Copilot or AgentProviderKind.Codex
            ? requested.Value
            : existing is AgentProviderKind.Copilot or AgentProviderKind.Codex
                ? existing.Value
                : AgentProviderKind.Codex;

    private static void EnsureSupportedKind(PersonKind kind)
    {
        if (kind is not (PersonKind.Human or PersonKind.Ai))
        {
            throw new ArgumentException("Person kind must be Human (1) or AI (2).", nameof(kind));
        }
    }

    private static PersonDirectoryDto Map(PersonDirectoryRecord record) =>
        new()
        {
            Id = record.Person.Id,
            Kind = record.Person.Kind,
            DisplayName = record.Person.DisplayName,
            Description = record.Person.Description,
            Metadata = new Dictionary<string, string>(record.Person.Metadata, StringComparer.OrdinalIgnoreCase),
            AgentSettings = record.AgentData is null ? null : Map(record.AgentData),
            CreatedAtUtc = record.Person.CreatedAtUtc,
            UpdatedAtUtc = record.Person.UpdatedAtUtc
        };

    private static EmployeeDirectoryDto Map(EmployeeDirectoryRecord record) =>
        new()
        {
            Person = Map(new PersonDirectoryRecord { Person = record.Person, AgentData = record.AgentData }),
            Id = record.Employee.Id,
            Key = record.Employee.Key,
            Role = record.Employee.Role,
            Mission = record.Employee.Mission,
            Status = record.Employee.Status,
            Specialties = record.Employee.Specialties.ToArray(),
            Metadata = new Dictionary<string, string>(record.Employee.Metadata, StringComparer.OrdinalIgnoreCase),
            CreatedAtUtc = record.Employee.CreatedAtUtc,
            UpdatedAtUtc = record.Employee.UpdatedAtUtc
        };

    private static AgentSettingsDto Map(AgentData agentData) =>
        new()
        {
            Provider = agentData.Provider,
            Model = agentData.Model,
            SessionId = agentData.SessionId,
            SystemPrompt = agentData.SystemPrompt,
            CodexReasoning = agentData.CodexReasoning,
            CodexEffort = agentData.CodexEffort,
            CodexCompute = agentData.CodexCompute,
            LastUsedAtUtc = agentData.LastUsedAtUtc,
            SessionUpdatedAtUtc = agentData.SessionUpdatedAtUtc
        };

    private static string RequireText(string value, string parameterName)
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

    private static List<string> NormalizeList(IEnumerable<string> values) =>
        values
            .Where(static value => !string.IsNullOrWhiteSpace(value))
            .Select(static value => value.Trim())
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

    private static Dictionary<string, string> NormalizeMetadata(IReadOnlyDictionary<string, string> metadata) =>
        metadata
            .Where(static pair => !string.IsNullOrWhiteSpace(pair.Key) && !string.IsNullOrWhiteSpace(pair.Value))
            .ToDictionary(static pair => pair.Key.Trim(), static pair => pair.Value.Trim(), StringComparer.OrdinalIgnoreCase);
}
