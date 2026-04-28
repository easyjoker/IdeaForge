using System.Text.Json;
using IdeaForge.Agents.Abstractions;
using IdeaForge.Application.Agents;
using IdeaForge.Infrastructure.Persistence;
using Npgsql;

namespace IdeaForge.Infrastructure.Agents;

public sealed class PostgreSqlAgentProfileRepository : IAgentProfileRepository
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);
    private readonly PostgreSqlOptions _options;

    public PostgreSqlAgentProfileRepository(PostgreSqlOptions options)
    {
        _options = options;
    }

    public async Task<IReadOnlyList<AgentProfile>> ListAsync(CancellationToken cancellationToken = default)
    {
        const string sql = """
            select
                p.id as person_id, p.kind as person_kind, p.display_name, p.description as person_description, p.metadata as person_metadata, p.created_at as person_created_at, p.updated_at as person_updated_at,
                e.id as employee_id, e.person_id as employee_person_id, e.key, e.role, e.mission, e.status, e.specialties, e.metadata as employee_metadata, e.created_at as employee_created_at, e.updated_at as employee_updated_at,
                a.provider, a.model, a.session_id, a.system_prompt, a.provider_settings, a.last_used_at, a.session_updated_at
            from public.employees e
            join public.persons p on p.id = e.person_id
            join public.agent_data a on a.person_id = p.id
            order by p.display_name;
            """;

        await using var connection = await OpenConnectionAsync(cancellationToken);
        await using var command = new NpgsqlCommand(sql, connection);
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);

        var items = new List<AgentProfile>();
        while (await reader.ReadAsync(cancellationToken))
        {
            items.Add(Map(reader));
        }

        return items;
    }

    public async Task<AgentProfile?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        const string sql = """
            select
                p.id as person_id, p.kind as person_kind, p.display_name, p.description as person_description, p.metadata as person_metadata, p.created_at as person_created_at, p.updated_at as person_updated_at,
                e.id as employee_id, e.person_id as employee_person_id, e.key, e.role, e.mission, e.status, e.specialties, e.metadata as employee_metadata, e.created_at as employee_created_at, e.updated_at as employee_updated_at,
                a.provider, a.model, a.session_id, a.system_prompt, a.provider_settings, a.last_used_at, a.session_updated_at
            from public.employees e
            join public.persons p on p.id = e.person_id
            join public.agent_data a on a.person_id = p.id
            where e.id = @id;
            """;

        await using var connection = await OpenConnectionAsync(cancellationToken);
        await using var command = new NpgsqlCommand(sql, connection);
        command.Parameters.AddWithValue("id", id);

        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        return await reader.ReadAsync(cancellationToken) ? Map(reader) : null;
    }

    public async Task<AgentProfile?> GetByKeyAsync(string key, CancellationToken cancellationToken = default)
    {
        const string sql = """
            select
                p.id as person_id, p.kind as person_kind, p.display_name, p.description as person_description, p.metadata as person_metadata, p.created_at as person_created_at, p.updated_at as person_updated_at,
                e.id as employee_id, e.person_id as employee_person_id, e.key, e.role, e.mission, e.status, e.specialties, e.metadata as employee_metadata, e.created_at as employee_created_at, e.updated_at as employee_updated_at,
                a.provider, a.model, a.session_id, a.system_prompt, a.provider_settings, a.last_used_at, a.session_updated_at
            from public.employees e
            join public.persons p on p.id = e.person_id
            join public.agent_data a on a.person_id = p.id
            where e.key = @key;
            """;

        await using var connection = await OpenConnectionAsync(cancellationToken);
        await using var command = new NpgsqlCommand(sql, connection);
        command.Parameters.AddWithValue("key", key);

        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        return await reader.ReadAsync(cancellationToken) ? Map(reader) : null;
    }

    public async Task<AgentProfile> AddAsync(AgentProfile profile, CancellationToken cancellationToken = default)
    {
        await using var connection = await OpenConnectionAsync(cancellationToken);
        await using var transaction = await connection.BeginTransactionAsync(cancellationToken);

        const string personSql = """
            insert into public.persons
            (
                id, kind, display_name, description, metadata, created_at, updated_at
            )
            values
            (
                @person_id, @person_kind, @display_name, @person_description, cast(@person_metadata as jsonb), @person_created_at, @person_updated_at
            );
            """;

        await using (var personCommand = new NpgsqlCommand(personSql, connection, transaction))
        {
            AddPersonParameters(personCommand, profile.Person);
            await personCommand.ExecuteNonQueryAsync(cancellationToken);
        }

        const string employeeSql = """
            insert into public.employees
            (
                id, person_id, key, role, mission, status, specialties, metadata, created_at, updated_at
            )
            values
            (
                @employee_id, @employee_person_id, @key, @role, @mission, @status, cast(@specialties as jsonb), cast(@employee_metadata as jsonb), @employee_created_at, @employee_updated_at
            );
            """;

        await using (var employeeCommand = new NpgsqlCommand(employeeSql, connection, transaction))
        {
            AddEmployeeParameters(employeeCommand, profile.Employee);
            await employeeCommand.ExecuteNonQueryAsync(cancellationToken);
        }

        const string agentDataSql = """
            insert into public.agent_data
            (
                person_id, provider, model, session_id, system_prompt, provider_settings, last_used_at, session_updated_at
            )
            values
            (
                @person_id, @provider, @model, @session_id, @system_prompt, cast(@provider_settings as jsonb), @last_used_at, @session_updated_at
            );
            """;

        await using (var agentDataCommand = new NpgsqlCommand(agentDataSql, connection, transaction))
        {
            AddAgentDataParameters(agentDataCommand, profile.AgentData);
            await agentDataCommand.ExecuteNonQueryAsync(cancellationToken);
        }

        await transaction.CommitAsync(cancellationToken);
        return profile;
    }

    public async Task<AgentProfile> UpdateAsync(AgentProfile profile, CancellationToken cancellationToken = default)
    {
        await using var connection = await OpenConnectionAsync(cancellationToken);
        await using var transaction = await connection.BeginTransactionAsync(cancellationToken);

        const string personSql = """
            update public.persons
            set
                kind = @person_kind,
                display_name = @display_name,
                description = @person_description,
                metadata = cast(@person_metadata as jsonb),
                created_at = @person_created_at,
                updated_at = @person_updated_at
            where id = @person_id;
            """;

        await using (var personCommand = new NpgsqlCommand(personSql, connection, transaction))
        {
            AddPersonParameters(personCommand, profile.Person);
            await personCommand.ExecuteNonQueryAsync(cancellationToken);
        }

        const string employeeSql = """
            update public.employees
            set
                person_id = @employee_person_id,
                key = @key,
                role = @role,
                mission = @mission,
                status = @status,
                specialties = cast(@specialties as jsonb),
                metadata = cast(@employee_metadata as jsonb),
                created_at = @employee_created_at,
                updated_at = @employee_updated_at
            where id = @employee_id;
            """;

        await using (var employeeCommand = new NpgsqlCommand(employeeSql, connection, transaction))
        {
            AddEmployeeParameters(employeeCommand, profile.Employee);
            await employeeCommand.ExecuteNonQueryAsync(cancellationToken);
        }

        const string agentDataSql = """
            update public.agent_data
            set
                provider = @provider,
                model = @model,
                session_id = @session_id,
                system_prompt = @system_prompt,
                provider_settings = cast(@provider_settings as jsonb),
                last_used_at = @last_used_at,
                session_updated_at = @session_updated_at
            where person_id = @person_id;
            """;

        await using (var agentDataCommand = new NpgsqlCommand(agentDataSql, connection, transaction))
        {
            AddAgentDataParameters(agentDataCommand, profile.AgentData);
            await agentDataCommand.ExecuteNonQueryAsync(cancellationToken);
        }

        await transaction.CommitAsync(cancellationToken);
        return profile;
    }

    public async Task<AgentExecutionRecord> AddExecutionAsync(AgentExecutionRecord execution, CancellationToken cancellationToken = default)
    {
        const string sql = """
            insert into public.agent_executions
            (
                id, employee_id, provider, model, session_id, prompt, output_summary, success, error_message, started_at, completed_at
            )
            values
            (
                @id, @employee_id, @provider, @model, @session_id, @prompt, @output_summary, @success, @error_message, @started_at, @completed_at
            );
            """;

        await using var connection = await OpenConnectionAsync(cancellationToken);
        await using var command = new NpgsqlCommand(sql, connection);
        command.Parameters.AddWithValue("id", execution.Id);
        command.Parameters.AddWithValue("employee_id", execution.EmployeeId);
        command.Parameters.AddWithValue("provider", (short)execution.Provider);
        command.Parameters.AddWithValue("model", execution.Model);
        command.Parameters.AddWithValue("session_id", (object?)execution.SessionId ?? DBNull.Value);
        command.Parameters.AddWithValue("prompt", execution.Prompt);
        command.Parameters.AddWithValue("output_summary", (object?)execution.OutputSummary ?? DBNull.Value);
        command.Parameters.AddWithValue("success", execution.Success);
        command.Parameters.AddWithValue("error_message", (object?)execution.ErrorMessage ?? DBNull.Value);
        command.Parameters.AddWithValue("started_at", execution.StartedAtUtc);
        command.Parameters.AddWithValue("completed_at", execution.CompletedAtUtc);
        await command.ExecuteNonQueryAsync(cancellationToken);
        return execution;
    }

    public async Task<IReadOnlyList<AgentExecutionRecord>> ListExecutionsAsync(Guid employeeId, CancellationToken cancellationToken = default)
    {
        const string sql = """
            select id, employee_id, provider, model, session_id, prompt, output_summary, success, error_message, started_at, completed_at
            from public.agent_executions
            where employee_id = @employee_id
            order by completed_at desc;
            """;

        await using var connection = await OpenConnectionAsync(cancellationToken);
        await using var command = new NpgsqlCommand(sql, connection);
        command.Parameters.AddWithValue("employee_id", employeeId);
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);

        var items = new List<AgentExecutionRecord>();
        while (await reader.ReadAsync(cancellationToken))
        {
            items.Add(new AgentExecutionRecord
            {
                Id = reader.GetGuid(reader.GetOrdinal("id")),
                EmployeeId = reader.GetGuid(reader.GetOrdinal("employee_id")),
                Provider = (AgentProviderKind)reader.GetInt16(reader.GetOrdinal("provider")),
                Model = reader.GetString(reader.GetOrdinal("model")),
                SessionId = reader.IsDBNull(reader.GetOrdinal("session_id")) ? null : reader.GetString(reader.GetOrdinal("session_id")),
                Prompt = reader.GetString(reader.GetOrdinal("prompt")),
                OutputSummary = reader.IsDBNull(reader.GetOrdinal("output_summary")) ? null : reader.GetString(reader.GetOrdinal("output_summary")),
                Success = reader.GetBoolean(reader.GetOrdinal("success")),
                ErrorMessage = reader.IsDBNull(reader.GetOrdinal("error_message")) ? null : reader.GetString(reader.GetOrdinal("error_message")),
                StartedAtUtc = reader.GetFieldValue<DateTimeOffset>(reader.GetOrdinal("started_at")),
                CompletedAtUtc = reader.GetFieldValue<DateTimeOffset>(reader.GetOrdinal("completed_at"))
            });
        }

        return items;
    }

    private async Task<NpgsqlConnection> OpenConnectionAsync(CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(_options.ConnectionString))
        {
            throw new InvalidOperationException("PostgreSQL connection string is not configured.");
        }

        var connection = new NpgsqlConnection(_options.ConnectionString);
        await connection.OpenAsync(cancellationToken);
        return connection;
    }

    private static void AddPersonParameters(NpgsqlCommand command, Person person)
    {
        command.Parameters.AddWithValue("person_id", person.Id);
        command.Parameters.AddWithValue("person_kind", (short)person.Kind);
        command.Parameters.AddWithValue("display_name", person.DisplayName);
        command.Parameters.AddWithValue("person_description", (object?)person.Description ?? DBNull.Value);
        command.Parameters.AddWithValue("person_metadata", JsonSerializer.Serialize(person.Metadata, JsonOptions));
        command.Parameters.AddWithValue("person_created_at", person.CreatedAtUtc);
        command.Parameters.AddWithValue("person_updated_at", person.UpdatedAtUtc);
    }

    private static void AddEmployeeParameters(NpgsqlCommand command, Employee employee)
    {
        command.Parameters.AddWithValue("employee_id", employee.Id);
        command.Parameters.AddWithValue("employee_person_id", employee.PersonId);
        command.Parameters.AddWithValue("key", employee.Key);
        command.Parameters.AddWithValue("role", (object?)employee.Role ?? DBNull.Value);
        command.Parameters.AddWithValue("mission", (object?)employee.Mission ?? DBNull.Value);
        command.Parameters.AddWithValue("status", (short)employee.Status);
        command.Parameters.AddWithValue("specialties", JsonSerializer.Serialize(employee.Specialties, JsonOptions));
        command.Parameters.AddWithValue("employee_metadata", JsonSerializer.Serialize(employee.Metadata, JsonOptions));
        command.Parameters.AddWithValue("employee_created_at", employee.CreatedAtUtc);
        command.Parameters.AddWithValue("employee_updated_at", employee.UpdatedAtUtc);
    }

    private static void AddAgentDataParameters(NpgsqlCommand command, AgentData agentData)
    {
        command.Parameters.AddWithValue("person_id", agentData.PersonId);
        command.Parameters.AddWithValue("provider", (short)agentData.Provider);
        command.Parameters.AddWithValue("model", agentData.Model);
        command.Parameters.AddWithValue("session_id", (object?)agentData.SessionId ?? DBNull.Value);
        command.Parameters.AddWithValue("system_prompt", (object?)agentData.SystemPrompt ?? DBNull.Value);
        command.Parameters.AddWithValue("provider_settings", JsonSerializer.Serialize(agentData.ProviderSettings, JsonOptions));
        command.Parameters.AddWithValue("last_used_at", (object?)agentData.LastUsedAtUtc ?? DBNull.Value);
        command.Parameters.AddWithValue("session_updated_at", (object?)agentData.SessionUpdatedAtUtc ?? DBNull.Value);
    }

    private static Dictionary<string, string> DeserializeProviderSettings(string json)
    {
        var settings = JsonSerializer.Deserialize<Dictionary<string, string>>(json, JsonOptions) ?? [];
        return new Dictionary<string, string>(settings, StringComparer.OrdinalIgnoreCase);
    }

    private static AgentProfile Map(NpgsqlDataReader reader)
    {
        var specialtiesJson = reader.GetString(reader.GetOrdinal("specialties"));
        var personMetadataJson = reader.GetString(reader.GetOrdinal("person_metadata"));
        var employeeMetadataJson = reader.GetString(reader.GetOrdinal("employee_metadata"));

        return new AgentProfile
        {
            Person = new Person
            {
                Id = reader.GetGuid(reader.GetOrdinal("person_id")),
                Kind = (PersonKind)reader.GetInt16(reader.GetOrdinal("person_kind")),
                DisplayName = reader.GetString(reader.GetOrdinal("display_name")),
                Description = reader.IsDBNull(reader.GetOrdinal("person_description")) ? null : reader.GetString(reader.GetOrdinal("person_description")),
                Metadata = JsonSerializer.Deserialize<Dictionary<string, string>>(personMetadataJson, JsonOptions) ?? [],
                CreatedAtUtc = reader.GetFieldValue<DateTimeOffset>(reader.GetOrdinal("person_created_at")),
                UpdatedAtUtc = reader.GetFieldValue<DateTimeOffset>(reader.GetOrdinal("person_updated_at"))
            },
            Employee = new Employee
            {
                Id = reader.GetGuid(reader.GetOrdinal("employee_id")),
                PersonId = reader.GetGuid(reader.GetOrdinal("employee_person_id")),
                Key = reader.GetString(reader.GetOrdinal("key")),
                Role = reader.IsDBNull(reader.GetOrdinal("role")) ? null : reader.GetString(reader.GetOrdinal("role")),
                Mission = reader.IsDBNull(reader.GetOrdinal("mission")) ? null : reader.GetString(reader.GetOrdinal("mission")),
                Status = (AgentStatus)reader.GetInt16(reader.GetOrdinal("status")),
                Specialties = JsonSerializer.Deserialize<List<string>>(specialtiesJson, JsonOptions) ?? [],
                Metadata = JsonSerializer.Deserialize<Dictionary<string, string>>(employeeMetadataJson, JsonOptions) ?? [],
                CreatedAtUtc = reader.GetFieldValue<DateTimeOffset>(reader.GetOrdinal("employee_created_at")),
                UpdatedAtUtc = reader.GetFieldValue<DateTimeOffset>(reader.GetOrdinal("employee_updated_at"))
            },
            AgentData = new AgentData
            {
                PersonId = reader.GetGuid(reader.GetOrdinal("person_id")),
                Provider = (AgentProviderKind)reader.GetInt16(reader.GetOrdinal("provider")),
                Model = reader.GetString(reader.GetOrdinal("model")),
                SessionId = reader.IsDBNull(reader.GetOrdinal("session_id")) ? null : reader.GetString(reader.GetOrdinal("session_id")),
                SystemPrompt = reader.IsDBNull(reader.GetOrdinal("system_prompt")) ? null : reader.GetString(reader.GetOrdinal("system_prompt")),
                ProviderSettings = DeserializeProviderSettings(reader.GetString(reader.GetOrdinal("provider_settings"))),
                LastUsedAtUtc = reader.IsDBNull(reader.GetOrdinal("last_used_at")) ? null : reader.GetFieldValue<DateTimeOffset>(reader.GetOrdinal("last_used_at")),
                SessionUpdatedAtUtc = reader.IsDBNull(reader.GetOrdinal("session_updated_at")) ? null : reader.GetFieldValue<DateTimeOffset>(reader.GetOrdinal("session_updated_at"))
            }
        };
    }
}
