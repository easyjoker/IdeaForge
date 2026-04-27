using IdeaForge.Agents.Abstractions;

namespace IdeaForge.Application.Personnel;

public interface IPersonnelRepository
{
    Task<IReadOnlyList<PersonDirectoryRecord>> ListPeopleAsync(CancellationToken cancellationToken = default);

    Task<PersonDirectoryRecord?> GetPersonAsync(Guid id, CancellationToken cancellationToken = default);

    Task<PersonDirectoryRecord?> GetPersonByKindAndDisplayNameAsync(PersonKind kind, string displayName, CancellationToken cancellationToken = default);

    Task<PersonDirectoryRecord> AddPersonAsync(Person person, AgentData? agentData, CancellationToken cancellationToken = default);

    Task<PersonDirectoryRecord?> UpdatePersonAsync(Person person, AgentData? agentData, CancellationToken cancellationToken = default);

    Task<bool> DeletePersonAsync(Guid id, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<EmployeeDirectoryRecord>> ListEmployeesAsync(CancellationToken cancellationToken = default);

    Task<EmployeeDirectoryRecord?> GetEmployeeAsync(Guid id, CancellationToken cancellationToken = default);

    Task<EmployeeDirectoryRecord?> GetEmployeeByPersonIdAsync(Guid personId, CancellationToken cancellationToken = default);

    Task<EmployeeDirectoryRecord?> GetEmployeeByKeyAsync(string key, CancellationToken cancellationToken = default);

    Task<EmployeeDirectoryRecord> AddEmployeeAsync(Employee employee, CancellationToken cancellationToken = default);

    Task<EmployeeDirectoryRecord?> UpdateEmployeeAsync(Employee employee, CancellationToken cancellationToken = default);

    Task<bool> DeleteEmployeeAsync(Guid id, CancellationToken cancellationToken = default);

    Task RemoveAgentDataByPersonIdAsync(Guid personId, CancellationToken cancellationToken = default);
}

public sealed class PersonDirectoryRecord
{
    public required Person Person { get; init; }

    public AgentData? AgentData { get; init; }
}

public sealed class EmployeeDirectoryRecord
{
    public required Person Person { get; init; }

    public required Employee Employee { get; init; }

    public AgentData? AgentData { get; init; }
}
