using IdeaForge.Agents.Abstractions;

namespace IdeaForge.Application.Personnel;

public interface IPersonnelRepository
{
    Task<IReadOnlyList<Person>> ListPeopleAsync(CancellationToken cancellationToken = default);

    Task<Person?> GetPersonAsync(Guid id, CancellationToken cancellationToken = default);

    Task<Person> AddPersonAsync(Person person, CancellationToken cancellationToken = default);

    Task<Person?> UpdatePersonAsync(Person person, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<EmployeeDirectoryRecord>> ListEmployeesAsync(CancellationToken cancellationToken = default);

    Task<EmployeeDirectoryRecord?> GetEmployeeAsync(Guid id, CancellationToken cancellationToken = default);

    Task<EmployeeDirectoryRecord?> GetEmployeeByPersonIdAsync(Guid personId, CancellationToken cancellationToken = default);

    Task<EmployeeDirectoryRecord?> GetEmployeeByKeyAsync(string key, CancellationToken cancellationToken = default);

    Task<EmployeeDirectoryRecord> AddEmployeeAsync(Employee employee, AgentData? agentData, CancellationToken cancellationToken = default);

    Task<EmployeeDirectoryRecord?> UpdateEmployeeAsync(Employee employee, AgentData? agentData, CancellationToken cancellationToken = default);

    Task RemoveAgentDataByPersonIdAsync(Guid personId, CancellationToken cancellationToken = default);
}

public sealed class EmployeeDirectoryRecord
{
    public required Person Person { get; init; }

    public required Employee Employee { get; init; }

    public AgentData? AgentData { get; init; }
}
