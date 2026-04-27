namespace IdeaForge.Application.Personnel;

public interface IPersonnelService
{
    Task<IReadOnlyList<PersonDirectoryDto>> ListPeopleAsync(CancellationToken cancellationToken = default);

    Task<PersonDirectoryDto?> GetPersonAsync(Guid id, CancellationToken cancellationToken = default);

    Task<PersonDirectoryDto> CreatePersonAsync(CreatePersonRequest request, CancellationToken cancellationToken = default);

    Task<PersonDirectoryDto?> UpdatePersonAsync(Guid id, UpdatePersonRequest request, CancellationToken cancellationToken = default);

    Task<bool> DeletePersonAsync(Guid id, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<EmployeeDirectoryDto>> ListEmployeesAsync(CancellationToken cancellationToken = default);

    Task<EmployeeDirectoryDto?> GetEmployeeAsync(Guid id, CancellationToken cancellationToken = default);

    Task<EmployeeDirectoryDto> CreateEmployeeAsync(CreateEmployeeRequest request, CancellationToken cancellationToken = default);

    Task<EmployeeDirectoryDto?> UpdateEmployeeAsync(Guid id, UpdateEmployeeRequest request, CancellationToken cancellationToken = default);

    Task<bool> DeleteEmployeeAsync(Guid id, CancellationToken cancellationToken = default);
}
