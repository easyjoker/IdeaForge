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
        var person = await _repository.GetPersonAsync(id, cancellationToken);
        return person is null ? null : Map(person);
    }

    public async Task<PersonDirectoryDto> CreatePersonAsync(CreatePersonRequest request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        EnsureSupportedKind(request.Kind);

        var now = DateTimeOffset.UtcNow;
        var person = new Person
        {
            Kind = request.Kind,
            DisplayName = RequireText(request.DisplayName, nameof(request.DisplayName)),
            Description = NormalizeOptional(request.Description),
            Metadata = NormalizeMetadata(request.Metadata),
            CreatedAtUtc = now,
            UpdatedAtUtc = now
        };

        return Map(await _repository.AddPersonAsync(person, cancellationToken));
    }

    public async Task<PersonDirectoryDto?> UpdatePersonAsync(Guid id, UpdatePersonRequest request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        EnsureSupportedKind(request.Kind);

        var person = await _repository.GetPersonAsync(id, cancellationToken);
        if (person is null)
        {
            return null;
        }

        var wasAi = person.Kind == PersonKind.Ai;
        person.Kind = request.Kind;
        person.DisplayName = RequireText(request.DisplayName, nameof(request.DisplayName));
        person.Description = NormalizeOptional(request.Description);
        person.Metadata = NormalizeMetadata(request.Metadata);
        person.UpdatedAtUtc = DateTimeOffset.UtcNow;

        var updated = await _repository.UpdatePersonAsync(person, cancellationToken);
        if (wasAi && request.Kind != PersonKind.Ai)
        {
            await _repository.RemoveAgentDataByPersonIdAsync(id, cancellationToken);
        }

        return updated is null ? null : Map(updated);
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

        var person = await _repository.GetPersonAsync(request.PersonId, cancellationToken)
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

        var agentData = person.Kind == PersonKind.Ai
            ? CreateAgentData(employee.Id, request.AgentSettings, requireSettings: true)
            : null;

        return Map(await _repository.AddEmployeeAsync(employee, agentData, cancellationToken));
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

        var agentData = existing.Person.Kind == PersonKind.Ai
            ? CreateAgentData(existing.Employee.Id, request.AgentSettings, existing.AgentData, requireSettings: existing.AgentData is null)
            : null;

        return Map(await _repository.UpdateEmployeeAsync(existing.Employee, agentData, cancellationToken) ?? existing);
    }

    private static AgentData CreateAgentData(Guid employeeId, UpsertAgentSettingsRequest? request, bool requireSettings) =>
        CreateAgentData(employeeId, request, null, requireSettings);

    private static AgentData CreateAgentData(Guid employeeId, UpsertAgentSettingsRequest? request, AgentData? existing, bool requireSettings)
    {
        if (request is null && requireSettings)
        {
            throw new ArgumentException("Agent settings are required when the person is AI.", nameof(request));
        }

        return new AgentData
        {
            EmployeeId = employeeId,
            Provider = request?.Provider ?? existing?.Provider ?? AgentProviderKind.Codex,
            Model = string.IsNullOrWhiteSpace(request?.Model) ? existing?.Model ?? "gpt-5.4" : request.Model.Trim(),
            SessionId = existing?.SessionId,
            SystemPrompt = request is null ? existing?.SystemPrompt : NormalizeOptional(request.SystemPrompt),
            LastUsedAtUtc = existing?.LastUsedAtUtc,
            SessionUpdatedAtUtc = existing?.SessionUpdatedAtUtc
        };
    }

    private static void EnsureSupportedKind(PersonKind kind)
    {
        if (kind is not (PersonKind.Human or PersonKind.Ai))
        {
            throw new ArgumentException("Person kind must be Human (1) or AI (2).", nameof(kind));
        }
    }

    private static PersonDirectoryDto Map(Person person) =>
        new()
        {
            Id = person.Id,
            Kind = person.Kind,
            DisplayName = person.DisplayName,
            Description = person.Description,
            Metadata = new Dictionary<string, string>(person.Metadata, StringComparer.OrdinalIgnoreCase),
            CreatedAtUtc = person.CreatedAtUtc,
            UpdatedAtUtc = person.UpdatedAtUtc
        };

    private static EmployeeDirectoryDto Map(EmployeeDirectoryRecord record) =>
        new()
        {
            Person = Map(record.Person),
            Id = record.Employee.Id,
            Key = record.Employee.Key,
            Role = record.Employee.Role,
            Mission = record.Employee.Mission,
            Status = record.Employee.Status,
            Specialties = record.Employee.Specialties.ToArray(),
            Metadata = new Dictionary<string, string>(record.Employee.Metadata, StringComparer.OrdinalIgnoreCase),
            AgentSettings = record.AgentData is null ? null : Map(record.AgentData),
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
