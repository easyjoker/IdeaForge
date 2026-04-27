using System.Text.Json;
using IdeaForge.Agents.Abstractions;
using IdeaForge.Application.Personnel;
using IdeaForge.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace IdeaForge.Infrastructure.Personnel;

public sealed class EfCorePersonnelRepository : IPersonnelRepository
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);
    private readonly IDbContextFactory<IdeaForgeDbContext> _dbContextFactory;

    public EfCorePersonnelRepository(IDbContextFactory<IdeaForgeDbContext> dbContextFactory)
    {
        _dbContextFactory = dbContextFactory;
    }

    public async Task<IReadOnlyList<Person>> ListPeopleAsync(CancellationToken cancellationToken = default)
    {
        await using var dbContext = await _dbContextFactory.CreateDbContextAsync(cancellationToken);
        var entities = await dbContext.Persons
            .AsNoTracking()
            .OrderBy(static person => person.DisplayName)
            .ToListAsync(cancellationToken);

        return entities.Select(Map).ToArray();
    }

    public async Task<Person?> GetPersonAsync(Guid id, CancellationToken cancellationToken = default)
    {
        await using var dbContext = await _dbContextFactory.CreateDbContextAsync(cancellationToken);
        var entity = await dbContext.Persons
            .AsNoTracking()
            .FirstOrDefaultAsync(person => person.Id == id, cancellationToken);

        return entity is null ? null : Map(entity);
    }

    public async Task<Person?> GetPersonByKindAndDisplayNameAsync(PersonKind kind, string displayName, CancellationToken cancellationToken = default)
    {
        await using var dbContext = await _dbContextFactory.CreateDbContextAsync(cancellationToken);
        var normalizedDisplayName = displayName.Trim().ToLower();
        var entity = await dbContext.Persons
            .AsNoTracking()
            .FirstOrDefaultAsync(
                person => person.Kind == (short)kind && person.DisplayName.ToLower() == normalizedDisplayName,
                cancellationToken);

        return entity is null ? null : Map(entity);
    }

    public async Task<Person> AddPersonAsync(Person person, CancellationToken cancellationToken = default)
    {
        await using var dbContext = await _dbContextFactory.CreateDbContextAsync(cancellationToken);
        var entity = ToEntity(person);
        dbContext.Persons.Add(entity);
        await dbContext.SaveChangesAsync(cancellationToken);
        return Map(entity);
    }

    public async Task<Person?> UpdatePersonAsync(Person person, CancellationToken cancellationToken = default)
    {
        await using var dbContext = await _dbContextFactory.CreateDbContextAsync(cancellationToken);
        var entity = await dbContext.Persons.FirstOrDefaultAsync(item => item.Id == person.Id, cancellationToken);
        if (entity is null)
        {
            return null;
        }

        entity.Kind = (short)person.Kind;
        entity.DisplayName = person.DisplayName;
        entity.Description = person.Description;
        entity.MetadataJson = JsonSerializer.Serialize(person.Metadata, JsonOptions);
        entity.UpdatedAtUtc = person.UpdatedAtUtc;

        await dbContext.SaveChangesAsync(cancellationToken);
        return Map(entity);
    }

    public async Task<bool> DeletePersonAsync(Guid id, CancellationToken cancellationToken = default)
    {
        await using var dbContext = await _dbContextFactory.CreateDbContextAsync(cancellationToken);
        var entity = await dbContext.Persons.FirstOrDefaultAsync(person => person.Id == id, cancellationToken);
        if (entity is null)
        {
            return false;
        }

        dbContext.Persons.Remove(entity);
        await dbContext.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<IReadOnlyList<EmployeeDirectoryRecord>> ListEmployeesAsync(CancellationToken cancellationToken = default)
    {
        await using var dbContext = await _dbContextFactory.CreateDbContextAsync(cancellationToken);
        var employees = await dbContext.Employees.AsNoTracking().OrderBy(static employee => employee.Key).ToListAsync(cancellationToken);
        return await BuildEmployeeRecordsAsync(dbContext, employees, cancellationToken);
    }

    public async Task<EmployeeDirectoryRecord?> GetEmployeeAsync(Guid id, CancellationToken cancellationToken = default)
    {
        await using var dbContext = await _dbContextFactory.CreateDbContextAsync(cancellationToken);
        var entity = await dbContext.Employees.AsNoTracking().FirstOrDefaultAsync(employee => employee.Id == id, cancellationToken);
        if (entity is null)
        {
            return null;
        }

        return (await BuildEmployeeRecordsAsync(dbContext, [entity], cancellationToken)).SingleOrDefault();
    }

    public async Task<EmployeeDirectoryRecord?> GetEmployeeByPersonIdAsync(Guid personId, CancellationToken cancellationToken = default)
    {
        await using var dbContext = await _dbContextFactory.CreateDbContextAsync(cancellationToken);
        var entity = await dbContext.Employees.AsNoTracking().FirstOrDefaultAsync(employee => employee.PersonId == personId, cancellationToken);
        if (entity is null)
        {
            return null;
        }

        return (await BuildEmployeeRecordsAsync(dbContext, [entity], cancellationToken)).SingleOrDefault();
    }

    public async Task<EmployeeDirectoryRecord?> GetEmployeeByKeyAsync(string key, CancellationToken cancellationToken = default)
    {
        await using var dbContext = await _dbContextFactory.CreateDbContextAsync(cancellationToken);
        var entity = await dbContext.Employees.AsNoTracking().FirstOrDefaultAsync(employee => employee.Key == key, cancellationToken);
        if (entity is null)
        {
            return null;
        }

        return (await BuildEmployeeRecordsAsync(dbContext, [entity], cancellationToken)).SingleOrDefault();
    }

    public async Task<EmployeeDirectoryRecord> AddEmployeeAsync(Employee employee, AgentData? agentData, CancellationToken cancellationToken = default)
    {
        await using var dbContext = await _dbContextFactory.CreateDbContextAsync(cancellationToken);
        await using var transaction = await dbContext.Database.BeginTransactionAsync(cancellationToken);

        dbContext.Employees.Add(ToEntity(employee));
        if (agentData is not null)
        {
            dbContext.AgentData.Add(ToEntity(agentData));
        }

        await dbContext.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);

        return await GetEmployeeAsync(employee.Id, cancellationToken)
            ?? throw new InvalidOperationException($"Employee '{employee.Id}' was not found after creation.");
    }

    public async Task<EmployeeDirectoryRecord?> UpdateEmployeeAsync(Employee employee, AgentData? agentData, CancellationToken cancellationToken = default)
    {
        await using var dbContext = await _dbContextFactory.CreateDbContextAsync(cancellationToken);
        await using var transaction = await dbContext.Database.BeginTransactionAsync(cancellationToken);

        var employeeEntity = await dbContext.Employees.FirstOrDefaultAsync(item => item.Id == employee.Id, cancellationToken);
        if (employeeEntity is null)
        {
            return null;
        }

        employeeEntity.Key = employee.Key;
        employeeEntity.Role = employee.Role;
        employeeEntity.Mission = employee.Mission;
        employeeEntity.Status = (short)employee.Status;
        employeeEntity.SpecialtiesJson = JsonSerializer.Serialize(employee.Specialties, JsonOptions);
        employeeEntity.MetadataJson = JsonSerializer.Serialize(employee.Metadata, JsonOptions);
        employeeEntity.UpdatedAtUtc = employee.UpdatedAtUtc;

        var agentEntity = await dbContext.AgentData.FirstOrDefaultAsync(item => item.EmployeeId == employee.Id, cancellationToken);
        if (agentData is null)
        {
            if (agentEntity is not null)
            {
                dbContext.AgentData.Remove(agentEntity);
            }
        }
        else if (agentEntity is null)
        {
            dbContext.AgentData.Add(ToEntity(agentData));
        }
        else
        {
            agentEntity.Provider = (short)agentData.Provider;
            agentEntity.Model = agentData.Model;
            agentEntity.SessionId = agentData.SessionId;
            agentEntity.SystemPrompt = agentData.SystemPrompt;
            agentEntity.LastUsedAtUtc = agentData.LastUsedAtUtc;
            agentEntity.SessionUpdatedAtUtc = agentData.SessionUpdatedAtUtc;
        }

        await dbContext.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);

        return await GetEmployeeAsync(employee.Id, cancellationToken);
    }

    public async Task<bool> DeleteEmployeeAsync(Guid id, CancellationToken cancellationToken = default)
    {
        await using var dbContext = await _dbContextFactory.CreateDbContextAsync(cancellationToken);
        var entity = await dbContext.Employees.FirstOrDefaultAsync(employee => employee.Id == id, cancellationToken);
        if (entity is null)
        {
            return false;
        }

        dbContext.Employees.Remove(entity);
        await dbContext.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task RemoveAgentDataByPersonIdAsync(Guid personId, CancellationToken cancellationToken = default)
    {
        await using var dbContext = await _dbContextFactory.CreateDbContextAsync(cancellationToken);
        var employeeIds = await dbContext.Employees
            .Where(employee => employee.PersonId == personId)
            .Select(employee => employee.Id)
            .ToArrayAsync(cancellationToken);

        if (employeeIds.Length == 0)
        {
            return;
        }

        var agentData = await dbContext.AgentData
            .Where(agent => employeeIds.Contains(agent.EmployeeId))
            .ToArrayAsync(cancellationToken);

        dbContext.AgentData.RemoveRange(agentData);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    private static async Task<IReadOnlyList<EmployeeDirectoryRecord>> BuildEmployeeRecordsAsync(
        IdeaForgeDbContext dbContext,
        IReadOnlyList<EmployeeEntity> employees,
        CancellationToken cancellationToken)
    {
        if (employees.Count == 0)
        {
            return Array.Empty<EmployeeDirectoryRecord>();
        }

        var personIds = employees.Select(static employee => employee.PersonId).Distinct().ToArray();
        var employeeIds = employees.Select(static employee => employee.Id).ToArray();

        var people = await dbContext.Persons
            .AsNoTracking()
            .Where(person => personIds.Contains(person.Id))
            .ToDictionaryAsync(person => person.Id, cancellationToken);

        var agentData = await dbContext.AgentData
            .AsNoTracking()
            .Where(agent => employeeIds.Contains(agent.EmployeeId))
            .ToDictionaryAsync(agent => agent.EmployeeId, cancellationToken);

        return employees
            .Where(employee => people.ContainsKey(employee.PersonId))
            .Select(employee => new EmployeeDirectoryRecord
            {
                Person = Map(people[employee.PersonId]),
                Employee = Map(employee),
                AgentData = agentData.TryGetValue(employee.Id, out var agent) ? Map(agent) : null
            })
            .OrderBy(static record => record.Person.DisplayName)
            .ToArray();
    }

    private static Person Map(PersonEntity entity) =>
        new()
        {
            Id = entity.Id,
            Kind = (PersonKind)entity.Kind,
            DisplayName = entity.DisplayName,
            Description = entity.Description,
            Metadata = JsonSerializer.Deserialize<Dictionary<string, string>>(entity.MetadataJson, JsonOptions) ?? [],
            CreatedAtUtc = entity.CreatedAtUtc,
            UpdatedAtUtc = entity.UpdatedAtUtc
        };

    private static Employee Map(EmployeeEntity entity) =>
        new()
        {
            Id = entity.Id,
            PersonId = entity.PersonId,
            Key = entity.Key,
            Role = entity.Role,
            Mission = entity.Mission,
            Status = (AgentStatus)entity.Status,
            Specialties = JsonSerializer.Deserialize<List<string>>(entity.SpecialtiesJson, JsonOptions) ?? [],
            Metadata = JsonSerializer.Deserialize<Dictionary<string, string>>(entity.MetadataJson, JsonOptions) ?? [],
            CreatedAtUtc = entity.CreatedAtUtc,
            UpdatedAtUtc = entity.UpdatedAtUtc
        };

    private static AgentData Map(AgentDataEntity entity) =>
        new()
        {
            EmployeeId = entity.EmployeeId,
            Provider = (AgentProviderKind)entity.Provider,
            Model = entity.Model,
            SessionId = entity.SessionId,
            SystemPrompt = entity.SystemPrompt,
            LastUsedAtUtc = entity.LastUsedAtUtc,
            SessionUpdatedAtUtc = entity.SessionUpdatedAtUtc
        };

    private static PersonEntity ToEntity(Person person) =>
        new()
        {
            Id = person.Id,
            Kind = (short)person.Kind,
            DisplayName = person.DisplayName,
            Description = person.Description,
            MetadataJson = JsonSerializer.Serialize(person.Metadata, JsonOptions),
            CreatedAtUtc = person.CreatedAtUtc,
            UpdatedAtUtc = person.UpdatedAtUtc
        };

    private static EmployeeEntity ToEntity(Employee employee) =>
        new()
        {
            Id = employee.Id,
            PersonId = employee.PersonId,
            Key = employee.Key,
            Role = employee.Role,
            Mission = employee.Mission,
            Status = (short)employee.Status,
            SpecialtiesJson = JsonSerializer.Serialize(employee.Specialties, JsonOptions),
            MetadataJson = JsonSerializer.Serialize(employee.Metadata, JsonOptions),
            CreatedAtUtc = employee.CreatedAtUtc,
            UpdatedAtUtc = employee.UpdatedAtUtc
        };

    private static AgentDataEntity ToEntity(AgentData agentData) =>
        new()
        {
            EmployeeId = agentData.EmployeeId,
            Provider = (short)agentData.Provider,
            Model = agentData.Model,
            SessionId = agentData.SessionId,
            SystemPrompt = agentData.SystemPrompt,
            LastUsedAtUtc = agentData.LastUsedAtUtc,
            SessionUpdatedAtUtc = agentData.SessionUpdatedAtUtc
        };
}
